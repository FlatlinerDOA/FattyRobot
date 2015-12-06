namespace Fatty.Brain.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reactive;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;

    public abstract class InterpreterBase : IObserver<Intent>, IObservable<Intent>
    {
        private readonly Dictionary<string, Func<Intent, IObservable<Intent>>> interpretations = new Dictionary<string, Func<Intent, IObservable<Intent>>>(StringComparer.OrdinalIgnoreCase);
        private IScheduler observeOn;
        private readonly Subject<Intent> outputs = new Subject<Intent>();

        public InterpreterBase(IScheduler observeOn)
        {
            this.observeOn = observeOn;
        }

        public Dictionary<string, Func<Intent, IObservable<Intent>>> Interpretations
        {
            get
            {
                return this.interpretations;
            }
        }

        public void OnCompleted()
        {
            Debug.WriteLine("Done with " + this.GetType().Name);
        }

        public void OnError(Exception error)
        {
            Debug.Assert(error == null, error.ToString());
        }

        public void OnNext(Intent value)
        {
            if (this.interpretations.ContainsKey(value.Name))
            {
                this.observeOn.Schedule(() =>
                {
                    this.interpretations[value.Name](value).Subscribe(this.Send);
                });
            }
        }

        public void Send(Intent value)
        {
            this.outputs.OnNext(value);
        }

        public IDisposable Subscribe(IObserver<Intent> observer)
        {
            return this.InitializeAsync().SelectMany(_ => this.outputs).Finally(() => Debug.WriteLine("Stopping listening to " + this.GetType().Name)).Subscribe(observer);
        }

        protected virtual IObservable<Unit> InitializeAsync()
        {
            return Observable.Return(new Unit());
        }
    }
}
