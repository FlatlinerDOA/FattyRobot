namespace Fatty.Brain
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Subjects;

    interface IBrain
    {
        IDisposable Start(IEnumerable<IObservable<Intent>> inputs, IEnumerable<IObserver<Intent>> outputs);
    }
}
