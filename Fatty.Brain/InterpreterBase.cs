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

        private readonly ReplaySubject<Intent> inputs = new ReplaySubject<Intent>(3);

        private readonly ReplaySubject<Intent> outputs = new ReplaySubject<Intent>(1);

        public InterpreterBase() : this(Scheduler.Default)
        {
        }

        public InterpreterBase(IScheduler observeOn)
        {
            this.Name = this.GetType().Name;
            this.observeOn = observeOn;
        }

        public Dictionary<string, Func<Intent, IObservable<Intent>>> Interpretations
        {
            get
            {
                return this.interpretations;
            }
        }

        public IObservable<Intent> Inputs
        {
            get
            {
                return this.inputs;
            }
        }

        public string Name
        {
            get;
            private set;
        }

        public void OnCompleted()
        {
            Debug.WriteLine("Done with " + this.GetType().Name);
        }

        public void OnError(Exception error)
        {
            Debug.Assert(error == null, error.ToString());
        }

        public virtual void OnNext(Intent value)
        {
            if (this.interpretations.ContainsKey(value.Name))
            {
                this.observeOn.Schedule(() =>
                {
                    this.interpretations[value.Name](value).Subscribe(this.Send);
                });
            }
            else
            {
                this.inputs.OnNext(value);
            }
        }

        public void Send(Intent value)
        {
            this.outputs.OnNext(value);
        }

        public IDisposable Subscribe(IObserver<Intent> observer)
        {
            return this.InitializeAsync().Merge(this.outputs)
                .Finally(() => Debug.WriteLine("Stopping listening to " + this.Name))
                .Subscribe(observer);
        }

        protected Intent Ready()
        {
            return new Intent(this.Name) { { "Status", "Ready" } };
        }

        protected virtual IObservable<Intent> InitializeAsync()
        {
            return Observable.Return(this.Ready());
        }
    }
}
