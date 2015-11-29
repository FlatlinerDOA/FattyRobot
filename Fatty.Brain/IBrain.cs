namespace Fatty.Brain
{
    using System;

    interface IBrain
    {
        IObserver<Intent> Inputs
        {
            get;
        }

        IObservable<object> Actions
        {
            get;
        }
    }
}
