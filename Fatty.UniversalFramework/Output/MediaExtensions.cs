using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Fatty.UniversalFramework.Output
{
    public static class MediaExtensions
    {
        public static IObservable<Unit> MediaEnds(this MediaElement source)
        {
            var t = Observable.FromEventPattern<RoutedEventHandler, RoutedEventArgs>(
                handler => source.MediaEnded += handler,
                handler => source.MediaEnded -= handler).Select(_ => new Unit())
                .Take(1);
            return t;
        }

        public static IObservable<MediaElementState> CurrentStateChanges(this MediaElement source)
        {
            var t = Observable.FromEventPattern<RoutedEventHandler, RoutedEventArgs>(
                handler => source.CurrentStateChanged += handler,
                handler => source.CurrentStateChanged -= handler).Select(_ => source.CurrentState);
            return t;
        }
    }
}
