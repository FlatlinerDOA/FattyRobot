namespace Fatty.Brain.Modules.Input
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fatty.Brain.Modules.Cognition;

    public sealed class SpeechRecognition : InterpreterBase
    {
        public const string Listen = "Listen";

        public SpeechRecognition(IScheduler coreDispatcher) : base(coreDispatcher)
        {
            this.Interpretations.Add(Listen, this.ListenAsync);
        }

        private IObservable<Intent> ListenAsync(Intent arg)
        {
            return Observable.FromAsync(this.ListenAsync);
        }

        private async Task<Intent> ListenAsync()
        {
            return null;
        }
    }
}
