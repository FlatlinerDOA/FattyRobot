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
    using SpeechTranslator;
    using Windows.Foundation;
    using Windows.Graphics.Display;
    using Windows.Media.Capture;
    using Windows.Media.MediaProperties;
    using Windows.Storage;
    using Windows.UI.Xaml.Controls;

    /// <summary>
    /// Used for audio and video capture
    /// </summary>
    public sealed class WebCamera : InterpreterBase
    {
        /// <summary>
        /// Out: When Photo is successfully captured
        /// </summary>
        public const string PhotoTaken = "PhotoTaken";

        /// <summary>
        /// In: Request a photo to be taken
        /// </summary>
        public const string TakePhoto = "TakePhoto";

        private readonly Subject<Intent> outputs = new Subject<Intent>();
        private List<Windows.Devices.Enumeration.DeviceInformation> deviceList;
        private bool isPreviewing = false;
        private bool isRecording = false;
        private Windows.Media.Capture.MediaCapture mediaCapture;
        private CaptureElement previewElement;
        private Windows.Media.MediaProperties.MediaEncodingProfile profile;
        private bool isListening;

        public WebCamera(IScheduler userInterface) : this(userInterface, null)
        {
        }

        public WebCamera(IScheduler userInterface, CaptureElement preview) : base(userInterface)
        {
            this.previewElement = preview;
            this.Interpretations.Add(TakePhoto, this.TakePhotoAsync);
        }

        public MediaCapture MediaCapture
        {
            get
            {
                return this.mediaCapture;
            }
        }

        public bool UseAudioOnly
        {
            get; set;
        }

        public object PreviewElement
        {
            get
            {
                return this.previewElement;
            }
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

        protected override IObservable<Intent> InitializeAsync()
        {
            return Observable.FromAsync(this.InitializeAudioOnlyAsync);
        }

        private int ConvertVideoRotationToMFRotation(VideoRotation rotation)
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

        private async Task<Intent> InitializeAudioOnlyAsync()
        {
            this.mediaCapture = new MediaCapture();
            var settings = new Windows.Media.Capture.MediaCaptureInitializationSettings();
            settings.StreamingCaptureMode = Windows.Media.Capture.StreamingCaptureMode.Audio;
            settings.MediaCategory = Windows.Media.Capture.MediaCategory.Other;
            settings.AudioProcessing = Windows.Media.AudioProcessing.Default;
            await mediaCapture.InitializeAsync(settings);
            return this.Ready();
        }

        private async Task<Intent> InitializeVideoAsync()
        {
            isListening = false;

            // Check Microphone Plugged in
            bool permissionGained = await AudioCapturePermissions.RequestMicrophoneCapture();
            if (!permissionGained)
            {
                return new Intent("Error")
                {
                    { "Message", "I could not connect to the Microphone, can you make sure my Microphone is plugged in?" }
                };
            }

            var devices = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(Windows.Devices.Enumeration.DeviceClass.VideoCapture);
            deviceList = new List<Windows.Devices.Enumeration.DeviceInformation>();

            if (devices.Count > 0)
            {
                for (var i = 0; i < devices.Count; i++)
                {
                    deviceList.Add(devices[i]);
                }

                var captureSettings = this.InitVideoCaptureSettings();
                await this.InitMediaCaptureAsync(captureSettings);
            }

            return this.Ready();
        }

        private MediaCaptureInitializationSettings InitVideoCaptureSettings()
        {
            // Set the Capture Setting
            var captureInitSettings = new Windows.Media.Capture.MediaCaptureInitializationSettings();
            captureInitSettings.AudioDeviceId = "";
            captureInitSettings.VideoDeviceId = "";
            captureInitSettings.StreamingCaptureMode = Windows.Media.Capture.StreamingCaptureMode.AudioAndVideo;
            captureInitSettings.PhotoCaptureSource = Windows.Media.Capture.PhotoCaptureSource.VideoPreview;

            if (deviceList.Count > 0)
            {
                captureInitSettings.VideoDeviceId = deviceList[0].Id;
            }

            return captureInitSettings;
        }

        private async Task InitMediaCaptureAsync(MediaCaptureInitializationSettings captureInitSettings)
        {
            this.mediaCapture = null;
            this.mediaCapture = new Windows.Media.Capture.MediaCapture();

            await this.mediaCapture.InitializeAsync(captureInitSettings);

            // Add video stabilization effect during Live Capture
            //await _mediaCapture.AddEffectAsync(MediaStreamType.VideoRecord, Windows.Media.VideoEffects.VideoStabilization, null); //this will be deprecated soon
            Windows.Media.Effects.VideoEffectDefinition def = new Windows.Media.Effects.VideoEffectDefinition(Windows.Media.VideoEffects.VideoStabilization);
            await this.mediaCapture.AddVideoEffectAsync(def, MediaStreamType.VideoRecord);

            this.CreateProfile();

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

        private IObservable<Intent> TakePhotoAsync(Intent arg)
        {
            return Observable.FromAsync(this.TakePhotoAsync);
        }

        private async Task<Intent> TakePhotoAsync()
        {
            var fileName = "Fatty-" + DateTime.Today.ToString("yyyy-MM-dd");
            var photoFile = await KnownFolders.CameraRoll.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);
            ImageEncodingProperties imageProperties = ImageEncodingProperties.CreateJpeg();
            await mediaCapture.CapturePhotoToStorageFileAsync(imageProperties, photoFile);
            return new Intent(PhotoTaken)
            {
                { "FileName", photoFile.Name },
                { "Path", photoFile.Path }
            };

            ////var sequenceCapture = await this.mediaCapture.PrepareLowLagPhotoSequenceCaptureAsync(ImageEncodingProperties.CreateJpeg());
            ////var captures = Observable.FromEventPattern<Windows.Foundation.TypedEventHandler<LowLagPhotoSequenceCapture, PhotoCapturedEventArgs>, PhotoCapturedEventArgs>(
            ////    handler => sequenceCapture.PhotoCaptured += handler,
            ////    handler => sequenceCapture.PhotoCaptured -= handler);

            ////await sequenceCapture.StartAsync();
            ////var intent = await captures.Take(1).Select(c => new Intent(PhotoTaken) { { "Time", c.EventArgs.CaptureTimeOffset.ToString() } });
            ////await sequenceCapture.StopAsync();
            ////await sequenceCapture.FinishAsync();
            ////return intent;
        }
    }
}