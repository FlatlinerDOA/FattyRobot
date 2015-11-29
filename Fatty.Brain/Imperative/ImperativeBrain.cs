namespace Fatty.Brain.Imperative
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;

    public sealed class ImperativeBrain
    {
        public ImperativeBrain()
        {
            this.Background = TaskPoolScheduler.Default;
            this.HumanInterface = CurrentThreadScheduler.Instance;
            this.InputModules = new List<IObservable<Intent>>();
            this.OutputModules = new List<IObserver<Intent>>();
        }

        public IScheduler Background
        {
            get; set;
        }

        public IScheduler HumanInterface
        {
            get; set;
        }

        public List<IObservable<Intent>> InputModules
        {
            get;
            private set;
        }

        public List<IObserver<Intent>> OutputModules
        {
            get;
            private set;
        }

        public IDisposable Start()
        {
            var t = from setOfIntents in Observable.Merge(this.InputModules).Buffer(TimeSpan.FromMilliseconds(500))
                    let bestIntent = setOfIntents.OrderByDescending(p => p.Probability).FirstOrDefault()
                    where bestIntent != null
                    select bestIntent;

            // Priority 1  Probability 80% 1 * 0.8 = .8
            // Priority 1  Probability 50% 1 * 0.5 = 0.5
            // Priority 2  Probability 100%  1 * (1/2) = 0.5
            // Prioirty 3  Probability 100%  1 * (1/3) = 0.25
            var sharedSubscription = t.Publish();
            foreach (var observer in this.OutputModules)
            {
                sharedSubscription.Subscribe(observer);
            }

            return sharedSubscription.Connect();
        }
    }
}
