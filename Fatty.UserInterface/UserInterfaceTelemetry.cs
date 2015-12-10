using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive.Concurrency;
using System.Reactive.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Fatty.Brain;
using Fatty.Brain.Modules;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace Fatty.UserInterface
{
    public sealed class UserInterfaceTelemetry : InterpreterBase, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<Intent> logs;
        private IScheduler userInterface;
        private BitmapImage lastPhoto;

        public UserInterfaceTelemetry(ObservableCollection<Intent> logs, IScheduler userInterface) : base(userInterface)
        {
            this.logs = logs;
            this.userInterface = userInterface;
            this.Interpretations.Add("PhotoTaken", this.PreviewPhotoAsync);
        }

        public BitmapImage LastPhoto
        {
            get
            {
                return this.lastPhoto;
            }

            set
            {
                this.SetProperty(ref this.lastPhoto, value);
            }
        }
        private IObservable<Intent> PreviewPhotoAsync(Intent arg)
        {
            return this.PreviewPhotoAsync(arg["FileName"]).ToObservable();
        }

        private async Task<Intent> PreviewPhotoAsync(string path)
        {
            var file = await KnownFolders.CameraRoll.GetFileAsync(path);
            IRandomAccessStream photoStream = await file.OpenReadAsync();
            BitmapImage bitmap = new BitmapImage();
            bitmap.SetSource(photoStream);
            this.LastPhoto = bitmap;
            return null;
        }

        public override void OnNext(Intent value)
        {
            this.userInterface.Schedule(() =>
            {
                this.logs.Add(value);
            });

            base.OnNext(value);
        }

        private void SetProperty<T>(ref T variable, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(variable, newValue))
            {
                return;
            }

            variable = newValue;
            this.OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName)
        {
            var h = this.PropertyChanged;
            if (h != null)
            {
                h(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}