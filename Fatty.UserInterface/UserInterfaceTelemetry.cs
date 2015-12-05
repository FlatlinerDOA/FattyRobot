using System;
using System.Collections.ObjectModel;
using Fatty.Brain;

namespace Fatty.UserInterface
{
    public sealed class UserInterfaceTelemetry : IObserver<Intent>
    {
        private ObservableCollection<Intent> logs;

        public UserInterfaceTelemetry(ObservableCollection<Intent> logs)
        {
            this.logs = logs;
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(Intent value)
        {
            this.logs.Add(value);
        }
    }
}