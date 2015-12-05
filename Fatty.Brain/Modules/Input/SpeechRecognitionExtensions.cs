namespace Fatty.Brain.Modules
{
    using System;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;

    public static class SpeechRecognitionExtensions
    {
        public static IObservable<Intent> WhenIHear(this IObservable<Intent> source, string text)
        {
            return source.Where(i => i.Name == text);
        }
    }
}
