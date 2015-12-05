namespace Fatty.Brain.Inputs
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using Modules;
    public sealed class FacialRecognition : InterpreterBase
    {
        public FacialRecognition()
        {
            this.Interpretations.Add("InterpretPhoto", this.InterpretPhoto);
        }

        private IObservable<Intent> InterpretPhoto(Intent intent)
        {
            return Observable.Empty<Intent>();
        }
    }
}
