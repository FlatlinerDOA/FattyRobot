namespace Fatty.Brain.Modules.Output
{
    using System;
    using System.Diagnostics;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Threading;
    using Windows.Devices.Gpio;
    using Windows.Foundation;
    using Windows.System.Threading;

    public sealed class MotorControl : InterpreterBase
    {
        public CtrlCmds lastSetCmd;
        public long msLastDirectionTime;
        public long msLastMessageInTime;
        public int speedValue = 10000;
        public PulseMs waitTimeLeft = PulseMs.stop;
        public PulseMs waitTimeRight = PulseMs.stop;
        private const int ACT_LED_PIN = 47;
        private const int LEFT_PWM_PIN = 5;
        private const int MaxDebs = 10;
        private const int RIGHT_PWM_PIN = 6;
        private const int SENSOR_PIN = 13;
        private int[] debounceCounts;
        private int[] debounceLast;
        private int[] debounceValues;

        // rpi2-its-pin47, rpi-its-pin16
        private GpioController gpioController = null;

        private bool GpioInitialized = false;
        private bool isBlockSensed = false;
        private bool lastIsBlockSensed = false;
        private int lastSpeed;
        private PulseMs lastWTL, lastWTR;
        private GpioPin leftPwmPin = null;
        private long msLastCheckTime;
        private GpioPin rightPwmPin = null;
        private GpioPin sensorPin = null;
        private GpioPin statusLedPin = null;
        private Stopwatch stopwatch;
        private ulong ticksPerMs;
        private IAsyncAction workItemThread;

        public MotorControl() : base(Scheduler.Default)
        {
            this.Interpretations.Add("Move", this.Move);
            this.Interpretations.Add("Turn", this.Turn);
        }

        private IObservable<Intent> Turn(Intent arg)
        {
            var rotation = (arg["Rotation"] ?? string.Empty).ToLowerInvariant();
            if (rotation.Length == 0)
            {
                return Observable.Return<Intent>(null);
            }

            switch (rotation)
            {
                case "-180":
                case "180":
                case "around":
                    this.SetRobotDirection(CtrlCmds.Right, CtrlSpeeds.Max);
                    break;

                case "right":
                    this.SetRobotDirection(CtrlCmds.Right, (int)CtrlSpeeds.Max);
                    break;

                case "left":
                    this.SetRobotDirection(CtrlCmds.Left, (int)CtrlSpeeds.Max);
                    break;
            }

            return Observable.Return<Intent>(null);
        }

        private IObservable<Intent> Move(Intent arg)
        {
            var direction = (arg["Direction"] ?? string.Empty).ToLowerInvariant();
            if (direction.Length == 0)
            {
                return Observable.Return<Intent>(null);
            }

            switch (direction)
            {
                case "forward":
                    this.SetRobotDirection(CtrlCmds.Right, CtrlSpeeds.Max);
                    break;

                case "right":
                    this.SetRobotDirection(CtrlCmds.Right, (int)CtrlSpeeds.Max);
                    break;

                case "left":
                    this.SetRobotDirection(CtrlCmds.Left, (int)CtrlSpeeds.Max);
                    break;
            }

            double duration = 0;
            if (double.TryParse(arg["Duration"] ?? "0", out duration))
            {
                this.MoveMotorsForTime((uint)duration);
            }
            else
            {
                this.MoveMotorsForTime(200);
            }

            return Observable.Return<Intent>(null);
        }

        public enum PulseMs
        {
            stop = -1,
            ms1 = 0,
            ms2 = 2
        }

        private enum MotorIds
        {
            Left,
            Right
        }

        // values selected for thread-safety
        public void SetRobotDirection(CtrlCmds cmd, CtrlSpeeds speed)
        {
            this.SetRobotDirection(cmd, (int)speed);
        }

        public void SetRobotDirection(CtrlCmds cmd, int speed)
        {
            switch (cmd)
            {
                case CtrlCmds.Forward:
                    waitTimeLeft = PulseMs.ms2;
                    waitTimeRight = PulseMs.ms1;
                    break;

                case CtrlCmds.Backward:
                    waitTimeLeft = PulseMs.ms1;
                    waitTimeRight = PulseMs.ms2;
                    break;

                case CtrlCmds.Left:
                    waitTimeLeft = PulseMs.ms1;
                    waitTimeRight = PulseMs.ms1;
                    break;

                case CtrlCmds.Right:
                    waitTimeLeft = PulseMs.ms2;
                    waitTimeRight = PulseMs.ms2;
                    break;

                case CtrlCmds.ForwardLeft:
                    waitTimeLeft = PulseMs.stop;
                    waitTimeRight = PulseMs.ms1;
                    break;

                case CtrlCmds.ForwardRight:
                    waitTimeLeft = PulseMs.ms2;
                    waitTimeRight = PulseMs.stop;
                    break;

                case CtrlCmds.BackLeft:
                    waitTimeLeft = PulseMs.stop;
                    waitTimeRight = PulseMs.ms2;
                    break;

                case CtrlCmds.BackRight:
                    waitTimeLeft = PulseMs.ms1;
                    waitTimeRight = PulseMs.stop;
                    break;

                default:
                case CtrlCmds.Stop:
                    waitTimeLeft = PulseMs.stop;
                    waitTimeRight = PulseMs.stop;
                    break;
            }
            if (speed < (int)CtrlSpeeds.Min)
                speed = (int)CtrlSpeeds.Min;
            if (speed > (int)CtrlSpeeds.Max)
                speed = (int)CtrlSpeeds.Max;
            speedValue = speed;

            this.OutputMovementIfChanged(cmd.ToString());
            ////if (!isRobot)
            ////{
            ////    String sendStr = "[" + (Convert.ToInt32(cmd)).ToString() + "]:" + cmd.ToString();
            ////    NetworkCmd.SendCommandToRobot(sendStr);
            ////}
            msLastDirectionTime = this.stopwatch.ElapsedMilliseconds;
            lastSetCmd = cmd;
        }

        protected override IObservable<Intent> InitializeAsync()
        {
            return Observable.Defer(() =>
            {
                DebounceInit();
                GpioInit();
                ticksPerMs = (ulong)(Stopwatch.Frequency) / 1000;
                workItemThread = ThreadPool.RunAsync((source) =>
                {
                    // setup, ensure pins initialized
                    ManualResetEvent mre = new ManualResetEvent(false);
                    mre.WaitOne(1000);
                    while (!GpioInitialized)
                    {
                        CheckSystem();
                    }

                    SetRobotDirection(CtrlCmds.Stop, (int)CtrlSpeeds.Max);

                    // settle period - to dismiss transient startup conditions, as things are starting up
                    for (int x = 0; x < 10; ++x)
                    {
                        mre.WaitOne(100);
                        isBlockSensed = DebounceValue((int)sensorPin.Read(), 0, 2) == 0;
                        lastIsBlockSensed = isBlockSensed;
                    }

                    // main motor timing loop
                    while (true)
                    {
                        PulseMotor(MotorIds.Left);
                        mre.WaitOne(2);
                        PulseMotor(MotorIds.Right);
                        mre.WaitOne(2);
                        CheckSystem();
                    }
                },
                WorkItemPriority.High);
                return Observable.Return<Intent>(null);
            });
        }

        private void BackupRobotSequence()
        {
            // stop the robot
            SetRobotDirection(CtrlCmds.Stop, (int)CtrlSpeeds.Max);
            MoveMotorsForTime(200);

            // back away from the obstruction
            SetRobotDirection(CtrlCmds.Backward, (int)CtrlSpeeds.Max);
            MoveMotorsForTime(300);

            // spin 180 degress
            SetRobotDirection(CtrlCmds.Right, (int)CtrlSpeeds.Max);
            MoveMotorsForTime(800);

            // leave in stopped condition
            SetRobotDirection(CtrlCmds.Stop, (int)CtrlSpeeds.Max);
        }

        /// <summary>
        /// CheckSystem - monitor for priority robot motion conditions (dead stick, or contact with object, etc.)
        /// </summary>
        private void CheckSystem()
        {
            long msCurTime = this.stopwatch.ElapsedMilliseconds;

            //--- Safety stop robot if no directions for awhile
            if ((msCurTime - msLastDirectionTime) > 15000)
            {
                Debug.WriteLine("Safety Stop (CurTime={0}, LastDirTime={1})", msCurTime, msLastDirectionTime);
                SetRobotDirection(CtrlCmds.Stop, (int)CtrlSpeeds.Max);
                ////FoundLocalControlsWorking = false;
                ////if ((msCurTime - msLastMessageInTime) > 12000)
                ////{
                ////    NetworkCmd.NetworkInit(MainPage.serverHostName);
                ////}

                ////XboxJoystickCheck();
            }

            //--- check on important things at a reasonable frequency
            if ((msCurTime - msLastCheckTime) > 50)
            {
                if (GpioInitialized)
                {
                    if (lastIsBlockSensed != isBlockSensed)
                    {
                        Debug.WriteLine("isBlockSensed={0}", isBlockSensed);
                        if (isBlockSensed)
                        {
                            BackupRobotSequence();
                            isBlockSensed = false;
                        }
                    }

                    lastIsBlockSensed = isBlockSensed;
                }

                // set ACT onboard LED to indicate motor movement
                // bool stopped = (waitTimeLeft == PulseMs.stop && waitTimeRight == PulseMs.stop);
                // statusLedPin.Write(stopped ? GpioPinValue.High : GpioPinValue.Low);
                msLastCheckTime = msCurTime;
            }
        }

        private void DebounceInit()
        {
            debounceValues = new int[MaxDebs];
            debounceCounts = new int[MaxDebs];
            debounceLast = new int[MaxDebs];
        }

        /// <summary>
        /// DebounceValue - returns a smoothened, un-rippled, value from a run of given, possibly transient, pin values.
        ///   curValue = raw pin input value
        ///   ix = an index for a unique pin, or purpose, to locate in array
        ///   run = the maximum number of values, which signify the signal value is un-rippled or solid
        /// </summary>
        /// <param name = "curValue"></param>
        /// <param name = "ix"></param>
        /// <param name = "run"></param>
        /// <returns></returns>
        private int DebounceValue(int curValue, int ix, int run)
        {
            if (ix < 0 || ix > debounceValues.Length)
            {
                return 0;
            }

            if (curValue == debounceValues[ix])
            {
                debounceCounts[ix] = 0;
                return curValue;
            }

            if (curValue == debounceLast[ix])
            {
                debounceCounts[ix] += 1;
            }
            else
            {
                debounceCounts[ix] = 0;
            }

            if (debounceCounts[ix] >= run)
            {
                debounceCounts[ix] = run;
                debounceValues[ix] = curValue;
            }

            debounceLast[ix] = curValue;
            return debounceValues[ix];
        }

        private void GpioInit()
        {
            try
            {
                gpioController = GpioController.GetDefault();
                if (null != gpioController)
                {
                    leftPwmPin = gpioController.OpenPin(LEFT_PWM_PIN);
                    leftPwmPin.SetDriveMode(GpioPinDriveMode.Output);
                    rightPwmPin = gpioController.OpenPin(RIGHT_PWM_PIN);
                    rightPwmPin.SetDriveMode(GpioPinDriveMode.Output);
                    statusLedPin = gpioController.OpenPin(ACT_LED_PIN);
                    statusLedPin.SetDriveMode(GpioPinDriveMode.Output);
                    statusLedPin.Write(GpioPinValue.Low);
                    sensorPin = gpioController.OpenPin(SENSOR_PIN);
                    sensorPin.SetDriveMode(GpioPinDriveMode.Input);
                    sensorPin.ValueChanged += (s, e) =>
                    {
                        GpioPinValue pinValue = sensorPin.Read();
                        statusLedPin.Write(pinValue);
                        isBlockSensed = (e.Edge == GpioPinEdge.RisingEdge);
                    }

                    ;
                    GpioInitialized = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ERROR: GpioInit failed - " + ex.Message);
            }
        }

        private void MoveMotorsForTime(uint ms)
        {
            if (!GpioInitialized)
            {
                return;
            }

            ManualResetEvent mre = new ManualResetEvent(false);
            ulong stick = (ulong)this.stopwatch.ElapsedTicks;
            while (true)
            {
                ulong delta = (ulong)(this.stopwatch.ElapsedTicks) - stick;
                if (delta > (ms * ticksPerMs))
                {
                    break; // stop motion after given time
                }

                PulseMotor(MotorIds.Left);
                mre.WaitOne(2);
                PulseMotor(MotorIds.Right);
                mre.WaitOne(2);
            }
        }

        private void OutputMovementIfChanged(string title)
        {
            if ((lastWTR == waitTimeRight) && (lastWTL == waitTimeLeft) && (lastSpeed == speedValue))
            {
                return;
            }

            this.Send(
                new Intent("Moving")
                {
                    { "Direction", title },
                    { "Left", waitTimeLeft.ToString() },
                    { "Right", waitTimeRight.ToString() },
                    { "Speed", speedValue.ToString() }
                });

            lastWTL = waitTimeLeft;
            lastWTR = waitTimeRight;
            lastSpeed = speedValue;
        }

        /// <summary>
        /// Generate a single motor pulse wave for given servo motor (High for 1 to 2ms, duration for 20ms).
        ///   motor value denotes which moter to send pulse to.
        /// </summary>
        /// <param name = "motor"></param>
        private void PulseMotor(MotorIds motor)
        {
            // Begin pulse (setup for simple Single Speed)
            ulong pulseTicks = ticksPerMs;
            if (motor == MotorIds.Left)
            {
                if (waitTimeLeft == PulseMs.stop)
                    return;
                if (waitTimeLeft == PulseMs.ms2)
                    pulseTicks = ticksPerMs * 2;
                leftPwmPin.Write(GpioPinValue.High);
            }
            else
            {
                if (waitTimeRight == PulseMs.stop)
                    return;
                if (waitTimeRight == PulseMs.ms2)
                    pulseTicks = ticksPerMs * 2;
                rightPwmPin.Write(GpioPinValue.High);
            }

            // Timing
            ulong delta;
            ulong starttick = (ulong)(this.stopwatch.ElapsedTicks);
            while (true)
            {
                delta = (ulong)(this.stopwatch.ElapsedTicks) - starttick;
                if (delta > (20 * ticksPerMs))
                    break;
                if (delta > pulseTicks)
                    break;
            }

            // End of pulse
            if (motor == MotorIds.Left)
                leftPwmPin.Write(GpioPinValue.Low);
            else
                rightPwmPin.Write(GpioPinValue.Low);
        }
    }
}