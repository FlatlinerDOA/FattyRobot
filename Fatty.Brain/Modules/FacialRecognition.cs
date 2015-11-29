namespace Fatty.Brain.Inputs
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Subjects;

    public sealed class FacialRecognition : IObserver<Intent>, IObservable<Intent>
    {
        private readonly Dictionary<string, Func<Intent, IObservable<Intent>>> interpretations = new Dictionary<string, Func<Intent, IObservable<Intent>>>();
        private readonly Subject<Intent> outputs = new Subject<Intent>();

        public FacialRecognition()
        {
            this.interpretations.Add("InterpretPhoto", this.InterpretPhoto);
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(Intent value)
        {
            if (this.interpretations.ContainsKey(value.Name))
            {
                this.interpretations[value.Name](value).Subscribe(this.outputs);
            }
        }

        public IDisposable Subscribe(IObserver<Intent> observer)
        {
            return this.outputs.Subscribe(observer);
        }

        private IObservable<Intent> InterpretPhoto(Intent intent)
        {
            throw new NotImplementedException();
        }
    }
}
