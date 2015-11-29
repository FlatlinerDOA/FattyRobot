namespace Fatty.Brain.Inputs
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Subjects;

    public sealed class WebCamera : IObserver<Intent>, IObservable<Intent>
    {
        private readonly Subject<Intent> outputs = new Subject<Intent>();

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(Intent value)
        {
            if (value.Name == "TakePhoto")
            {
                this.outputs.OnNext(new Intent("InterpretPhoto", 1.0d) {
                    { "s", "" } });
            }
        }

        public IDisposable Subscribe(IObserver<Intent> observer)
        {
            return this.outputs;
        }
    }
}
