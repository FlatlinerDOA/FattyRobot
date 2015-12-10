namespace Fatty.Brain.Inputs
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using Modules;
    public sealed class FacialRecognition : InterpreterBase
    {
        public FacialRecognition() : base(Scheduler.Default)
        {
            this.Interpretations.Add("InterpretPhoto", this.InterpretPhoto);
        }

        private IObservable<Intent> InterpretPhoto(Intent intent)
        {
            return Observable.Empty<Intent>();
        }
    }
}
