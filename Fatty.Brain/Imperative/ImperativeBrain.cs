namespace Fatty.Brain.Imperative
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    public sealed class ImperativeBrain : IBrain
    {
        public ImperativeBrain()
        {
            this.Background = TaskPoolScheduler.Default;
            this.HumanInterface = CurrentThreadScheduler.Instance;
        }

        public IScheduler Background
        {
            get; set;
        }

        public IScheduler HumanInterface
        {
            get; set;
        }

        public IDisposable Start(IEnumerable<IObservable<Intent>> inputModules, IEnumerable<IObserver<Intent>> outputModules)
        {
            var t = from setOfIntents in Observable.Merge(inputModules).Buffer(TimeSpan.FromMilliseconds(500))
                    let bestIntent = setOfIntents.OrderByDescending(p => p.Probability * p.Priority).FirstOrDefault()
                    where bestIntent != null
                    select bestIntent;

            // Priority 1  Probability 80% 1 * 0.8 = .8
            // Priority 1  Probability 50% 1 * 0.5 = 0.5
            // Priority 2  Probability 100%  1 * (1/2) = 0.5
            // Prioirty 3  Probability 100%  1 * (1/3) = 0.25
            var sharedSubscription = t.Publish();
            foreach (var observer in outputModules)
            {
                sharedSubscription.Subscribe(observer);
            }

            return sharedSubscription.Connect();
        }
    }
}
