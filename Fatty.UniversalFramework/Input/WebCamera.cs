namespace Fatty.UniversalFramework.Input
{
    using System;
    using System.Collections.Generic;
    using System.Reactive;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Threading.Tasks;
    using Brain;
    using Brain.Modules;
    using Windows.Foundation;
    using Windows.Graphics.Display;
    using Windows.Media.Capture;
    using Windows.Media.MediaProperties;
    using Windows.UI.Xaml.Controls;

    public sealed class WebCamera : InterpreterBase
    {
        private MediaCaptureInitializationSettings captureInitSettings;
        private List<Windows.Devices.Enumeration.DeviceInformation> deviceList;
        private Windows.Media.MediaProperties.MediaEncodingProfile profile;
        private Windows.Media.Capture.MediaCapture mediaCapture;

        private CaptureElement previewElement;

        private bool isRecording = false;

        private bool isPreviewing = false;

        private readonly Subject<Intent> outputs = new Subject<Intent>();

        public WebCamera(IScheduler takePhotoScheduler) : this(takePhotoScheduler, null)
        {
        }

        public WebCamera(IScheduler takePhotoScheduler, CaptureElement preview)
        {
            this.previewElement = preview;
            this.Interpretations.Add("TakePhoto", this.TakePhotoAndRequestIntepretation);
        }

        private IObservable<Intent> TakePhotoAndRequestIntepretation(Intent arg)
        {
            return Observable.FromAsync(this.TakePhotoAsync);
        }

        public MediaCapture MediaCapture
        {
            get
            {
                return this.mediaCapture;
            }
        }

        public object PreviewElement
        {
            get
            {
                return this.previewElement;
            }
        }

        protected override IObservable<Unit> InitializeAsync()
        {
            return Observable.FromAsync(this.EnumerateCamerasAsync);
        }

        private async Task EnumerateCamerasAsync()
        {
            var devices = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(Windows.Devices.Enumeration.DeviceClass.VideoCapture);
            deviceList = new List<Windows.Devices.Enumeration.DeviceInformation>();

            if (devices.Count > 0)
            {
                for (var i = 0; i < devices.Count; i++)
                {
                    deviceList.Add(devices[i]);
                }

                this.InitCaptureSettings();
                await InitMediaCaptureAsync();
            }
        }

        private async Task InitMediaCaptureAsync()
        {
            this.mediaCapture = null;
            this.mediaCapture = new Windows.Media.Capture.MediaCapture();

            await this.mediaCapture.InitializeAsync(this.captureInitSettings);

            // Add video stabilization effect during Live Capture
            //await _mediaCapture.AddEffectAsync(MediaStreamType.VideoRecord, Windows.Media.VideoEffects.VideoStabilization, null); //this will be deprecated soon
            Windows.Media.Effects.VideoEffectDefinition def = new Windows.Media.Effects.VideoEffectDefinition(Windows.Media.VideoEffects.VideoStabilization);
            await mediaCapture.AddVideoEffectAsync(def, MediaStreamType.VideoRecord);

            CreateProfile();

            // start preview

            if (this.previewElement != null)
            {
                this.previewElement.Source = this.mediaCapture;
            }

            DisplayInformation.AutoRotationPreferences = DisplayOrientations.None;

            //// set the video Rotation
            //    _mediaCapture.SetPreviewRotation(VideoRotation.Clockwise90Degrees);
            //    _mediaCapture.SetRecordRotation(VideoRotation.Clockwise90Degrees);
        }

        //Create a profile
        private void CreateProfile()
        {
            profile = Windows.Media.MediaProperties.MediaEncodingProfile.CreateMp4(Windows.Media.MediaProperties.VideoEncodingQuality.Qvga);

            // Use MediaEncodingProfile to encode the profile
            System.Guid MFVideoRotationGuild = new System.Guid("C380465D-2271-428C-9B83-ECEA3B4A85C1");
            int MFVideoRotation = ConvertVideoRotationToMFRotation(VideoRotation.None);
            profile.Video.Properties.Add(MFVideoRotationGuild, PropertyValue.CreateInt32(MFVideoRotation));


            // add the mediaTranscoder 
            var transcoder = new Windows.Media.Transcoding.MediaTranscoder();
            transcoder.AddVideoEffect(Windows.Media.VideoEffects.VideoStabilization);
        }

        private void InitCaptureSettings()
        {
            // Set the Capture Setting
            captureInitSettings = null;
            captureInitSettings = new Windows.Media.Capture.MediaCaptureInitializationSettings();
            captureInitSettings.AudioDeviceId = "";
            captureInitSettings.VideoDeviceId = "";
            captureInitSettings.StreamingCaptureMode = Windows.Media.Capture.StreamingCaptureMode.AudioAndVideo;
            captureInitSettings.PhotoCaptureSource = Windows.Media.Capture.PhotoCaptureSource.VideoPreview;

            if (deviceList.Count > 0)
            {
                captureInitSettings.VideoDeviceId = deviceList[0].Id;
            }
        }

        int ConvertVideoRotationToMFRotation(VideoRotation rotation)
        {
            int MFVideoRotation = 0;
            switch (rotation)
            {
                case VideoRotation.Clockwise90Degrees:
                    MFVideoRotation = 90;
                    break;
                case VideoRotation.Clockwise180Degrees:
                    MFVideoRotation = 180;
                    break;
                case VideoRotation.Clockwise270Degrees:
                    MFVideoRotation = 270;
                    break;
            }
            return MFVideoRotation;
        }

        public async Task CleanupCaptureResourcesAsync()
        {
            if (this.isRecording && this.mediaCapture != null)
            {
                await this.mediaCapture.StopRecordAsync();
                this.isRecording = false;
            }

            if (this.isPreviewing && MediaCapture != null)
            {
                await this.mediaCapture.StopPreviewAsync();
                this.isPreviewing = false;
            }

            if (this.mediaCapture != null)
            {
                if (this.previewElement != null)
                {
                    this.previewElement.Source = null;
                }

                this.mediaCapture.Dispose();
            }
        }

        private async Task<Intent> TakePhotoAsync()
        {
            var sequenceCapture = await this.mediaCapture.PrepareLowLagPhotoSequenceCaptureAsync(ImageEncodingProperties.CreateJpeg());
            var captures = Observable.FromEventPattern<Windows.Foundation.TypedEventHandler<LowLagPhotoSequenceCapture, PhotoCapturedEventArgs>, PhotoCapturedEventArgs>(
                handler => sequenceCapture.PhotoCaptured += handler,
                handler => sequenceCapture.PhotoCaptured -= handler);

            await sequenceCapture.StartAsync();
            var intent = await captures.Take(1).Select(c => new Intent("InterpretPhoto") { { "Time", c.EventArgs.CaptureTimeOffset.ToString() } });
            await sequenceCapture.StopAsync();
            await sequenceCapture.FinishAsync();
            return intent;
        }
    }
}
