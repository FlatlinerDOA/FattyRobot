namespace Fatty.Brain.Modules
{
    using System;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;

    public static class SpeechRecognitionExtensions
    {
        public static IObservable<Intent> WhenIHear(this InterpreterBase source, string text)
        {
            return source.Where(i => string.Equals(i.Name, text, StringComparison.OrdinalIgnoreCase));
        }
    }
}
