namespace Fatty.Brain.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    public abstract class InterpreterBase : IObserver<Intent>, IObservable<Intent>
    {
        private readonly Dictionary<string, Func<Intent, IObservable<Intent>>> interpretations = new Dictionary<string, Func<Intent, IObservable<Intent>>>();

        private readonly Subject<Intent> outputs = new Subject<Intent>();

        public Dictionary<string, Func<Intent, IObservable<Intent>>> Interpretations
        {
            get
            {
                return this.interpretations;
            }
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
            return this.InitializeAsync().SelectMany(this.outputs).Subscribe(observer);
        }

        protected virtual IObservable<Unit> InitializeAsync()
        {
            return Observable.Return(new Unit());
        }
    }
}
