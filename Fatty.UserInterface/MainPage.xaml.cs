using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Runtime.InteropServices.WindowsRuntime;
using Fatty.Brain;
using Fatty.Brain.Imperative;
using Fatty.Brain.Modules.Cognition;
using Fatty.UniversalFramework.Input;
using Fatty.UniversalFramework.Output;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Fatty.UserInterface
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ImperativeBrain brain;
        private IDisposable subscription;

        public MainPage()
        {
            this.InitializeComponent();
            this.Logs = new ObservableCollection<Intent>();
            this.brain = new ImperativeBrain();
        }

        public ObservableCollection<Intent> Logs
        {
            get;
            private set;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.DataContext = this;
            var normalScheduler = new CoreDispatcherScheduler(this.Dispatcher, Windows.UI.Core.CoreDispatcherPriority.Normal);
            var lowScheduler = new CoreDispatcherScheduler(this.Dispatcher, Windows.UI.Core.CoreDispatcherPriority.Low);
            var modules = new object[]
            {
                new TextInput(this.control),
                new UserInterfaceTelemetry(this.Logs, lowScheduler),
                new SpeechSynthesis(this.Dispatcher, this.media),
                new Playful(),
                new WebCamera(normalScheduler, this.capturePreview),
            };

            this.subscription = this.brain.Start(
                modules.OfType<IObservable<Intent>>(), 
                modules.OfType<IObserver<Intent>>());
        }
    }
}
