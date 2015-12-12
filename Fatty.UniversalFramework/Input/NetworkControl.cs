using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Fatty.Brain;
using Fatty.Brain.Extensions;
using Fatty.Brain.Modules;
using Windows.Foundation;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace Fatty.UniversalFramework.Input
{
    public sealed class NetworkControl : InterpreterBase
    {
        // if host name, be client, otherwise be a host
        private string connectToHostName;
        private const string hostPort = "8027";

        public NetworkControl(string connectToHostName) : base(Scheduler.Default)
        {
            // Host intents are things that the host is allowed to send to the client
            // Client sends everything to the host for remote telemetry purposes.
            this.connectToHostName = connectToHostName ?? string.Empty;
            this.isRobot = this.connectToHostName.Length > 0;
        }

        public override void OnNext(Intent value)
        {
            if (this.ignoreIntents.Contains(value.Name))
            {
                return;
            } 

            if (!this.isRobot)
            {
                // Forward them onto the client
                this.SendCommandToRobot(value);
                return;
            }
            else
            {
                this.PostSocketWrite(value.ToString());
            }

            base.OnNext(value);
        }

        protected override IObservable<Intent> InitializeAsync()
        {
            this.stopWatch = this.ObserveOn.StartStopwatch();
            return Observable.FromAsync(this.NetworkInitAsync);
        }

        public async Task<Intent> NetworkInitAsync()
        {
            ClearPrevious();

            Debug.WriteLine("NetworkInit() host={0}, port={1}", this.connectToHostName, hostPort);
            if (this.connectToHostName.Length > 0)
            {
                return await InitConnectionToHostAsync();
            }
            else
            {
                if (listener == null)
                {
                    return await StartListenerAsync();
                }

                return new Intent("HostAlreadyRunning");
            }
        }

        public long msLastSendTime;

        private string ctrlStringToSend;

        public void SendCommandToRobot(Intent intent)
        {
            this.SendCommandToRobot(intent.ToString());
        }

        public void SendCommandToRobot(string stringToSend)
        {
            ctrlStringToSend = stringToSend + "~";
            if (!this.isRobot)
            {
                this.Send(new Intent("SendingToRobot") { { "Intent", stringToSend } });
                PostSocketWrite(ctrlStringToSend);
            }

            Debug.WriteLine("Sending: " + ctrlStringToSend);
        }


        #region ----- host connection ----
        private StreamSocketListener listener;
        public async Task<Intent> StartListenerAsync()
        {
            try
            {
                listener = new StreamSocketListener();
                listener.ConnectionReceived += OnConnection;
                await listener.BindServiceNameAsync(hostPort);
                Debug.WriteLine("Listening on {0}", hostPort);
                return new Intent("HostStarted") { { "Port", hostPort.ToString() } };
            }
            catch (Exception e)
            {
                Debug.WriteLine("StartListener() - Unable to bind listener. " + e.Message);
                return new Intent("Error") { { "Message", e.Message } };
            }
        }

        private async void OnConnection(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            try
            {
                if (this.isRobot)
                {
                    DataReader reader = new DataReader(args.Socket.InputStream);
                    string str = string.Empty;
                    while (true)
                    {
                        uint len = await reader.LoadAsync(1);
                        if (len > 0)
                        {
                            byte b = reader.ReadByte();
                            str += Convert.ToChar(b);
                            if (b == '~')
                            {
                                Debug.WriteLine("Network Received: '{0}'", str);
                                var intent = Intent.Parse(str);
                                this.Send(intent);
                                ////Controllers.ParseCtrlMessage(str);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    string lastStringSent;
                    while (true)
                    {
                        DataWriter writer = new DataWriter(args.Socket.OutputStream);
                        lastStringSent = ctrlStringToSend;
                        writer.WriteString(lastStringSent);
                        await writer.StoreAsync();
                        msLastSendTime = this.ElapsedMilliseconds;

                        // re-send periodically
                        long msStart = msLastSendTime;
                        for (;;)
                        {
                            long msCurrent = this.ElapsedMilliseconds;
                            if ((msCurrent - msStart) > 3000)
                                break;
                            if (lastStringSent.CompareTo(ctrlStringToSend) != 0)
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("OnConnection() - " + e.Message);
            }
        }
        #endregion

        #region ----- client connection -----
        private StreamSocket socket;
        private bool socketIsConnected;

        private async Task<Intent> InitConnectionToHostAsync()
        {
            try
            {
                ClearPrevious();
                socket = new StreamSocket();

                HostName hostNameObj = new HostName(connectToHostName);
                await socket.ConnectAsync(hostNameObj, hostPort);
                Debug.WriteLine("Connected to {0}:{1}.", hostNameObj, hostPort);
                socketIsConnected = true;

                if (this.isRobot)
                {
                    PostSocketRead(1024);
                }

                return new Intent("ConnectedToHost")
                {
                    { "Host", this.connectToHostName },
                    { "Port", hostPort }
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine("InitConnectionToHost() - " + ex.Message);
                return new Intent("Error") { { "Message", ex.Message } };
            }
        }

        private void ClearPrevious()
        {
            if (socket != null)
            {
                socket.Dispose();
                socket = null;
                socketIsConnected = false;
            }
        }

        public async void OnDataReadCompletion(uint bytesRead, DataReader readPacket)
        {
            if (readPacket == null)
            {
                Debug.WriteLine("DataReader is null");
                return;
            }
            uint buffLen = readPacket.UnconsumedBufferLength;

            if (buffLen == 0)
            {
                // buflen==0 - assume server closed socket
                Debug.WriteLine("Attempting to disconnect and reconnecting to the server");
                this.Send(await InitConnectionToHostAsync());
                return;
            }

            string message = readPacket.ReadString(buffLen);
            Debug.WriteLine("Network Received (b={0},l={1}): '{2}'", bytesRead, buffLen, message);

            if (message.IndexOf("~") != -1)
            {
                this.Send(Intent.Parse(message.SubstringToFirst("~")));
            }
            
            PostSocketRead(1024);
        }

        DataReader readPacket;
        private bool isRobot;
        private IStopwatch stopWatch;
        private HashSet<string> ignoreIntents = new HashSet<string>(new[] { "ConnectedToHost", "HostStarted", "SendingToRobot" }, StringComparer.OrdinalIgnoreCase);

        public long ElapsedMilliseconds
        {
            get
            {
                return (long)this.stopWatch.Elapsed.TotalMilliseconds;
            }
        }

        private void PostSocketRead(int length)
        {
            if (socket == null || !socketIsConnected)
            {
                Debug.WriteLine("Rd: Socket not connected yet.");
                return;
            }

            try
            {
                var readBuf = new Windows.Storage.Streams.Buffer((uint)length);
                var readOp = socket.InputStream.ReadAsync(readBuf, (uint)length, InputStreamOptions.Partial);
                readOp.Completed = (IAsyncOperationWithProgress<IBuffer, uint> asyncAction, AsyncStatus asyncStatus) =>
                {
                    switch (asyncStatus)
                    {
                        case AsyncStatus.Completed:
                        case AsyncStatus.Error:
                            try
                            {
                                IBuffer localBuf = asyncAction.GetResults();
                                uint bytesRead = localBuf.Length;
                                readPacket = DataReader.FromBuffer(localBuf);
                                OnDataReadCompletion(bytesRead, readPacket);
                            }
                            catch (Exception exp)
                            {
                                Debug.WriteLine("Read operation failed:  " + exp.Message);
                            }
                            break;
                        case AsyncStatus.Canceled:
                            break;
                    }
                };
            }
            catch (Exception exp)
            {
                Debug.WriteLine("Failed to post a Read - " + exp.Message);
            }
        }

        private async void PostSocketWrite(string writeStr)
        {
            if (socket == null || !socketIsConnected)
            {
                Debug.WriteLine("Wr: Socket not connected yet.");
                return;
            }

            try
            {
                DataWriter writer = new DataWriter(socket.OutputStream);
                writer.WriteString(writeStr);
                await writer.StoreAsync();
                msLastSendTime = this.ElapsedMilliseconds;
            }
            catch (Exception exp)
            {
                Debug.WriteLine("Failed to Write - " + exp.Message);
            }
        }

        #endregion
    }
}
