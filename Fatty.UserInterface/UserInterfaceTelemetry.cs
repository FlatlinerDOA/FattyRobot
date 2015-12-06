using System;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using Fatty.Brain;

namespace Fatty.UserInterface
{
    public sealed class UserInterfaceTelemetry : IObserver<Intent>
    {
        private ObservableCollection<Intent> logs;
        private IScheduler userInterface;

        public UserInterfaceTelemetry(ObservableCollection<Intent> logs, IScheduler userInterface)
        {
            this.logs = logs;
            this.userInterface = userInterface;
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(Intent value)
        {
            this.userInterface.Schedule(() =>
            {
                this.logs.Add(value);
            });
        }
    }
}