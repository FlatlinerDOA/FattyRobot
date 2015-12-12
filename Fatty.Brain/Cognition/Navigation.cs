using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Fatty.Brain.Modules;

namespace Fatty.Brain.Cognition
{
    public sealed class Navigation : InterpreterBase
    {
        public Navigation()
        {
            this.Interpretations.Add("Dance", this.Dance);
            this.Interpretations.Add("FindFace", this.FindFace);
        }

        private IObservable<Intent> FindFace(Intent arg)
        {
            var turnRight = Observable.Return(new Intent("Turn") { { "Rotation", "10" } });
            var turnLeft = Observable.Return(new Intent("Turn") { { "Rotation", "-10" } });
            var stopMoving = Observable.Return(new Intent("StopMoving"));
            return turnLeft.Concat(turnRight).Concat(stopMoving);
        }

        private IObservable<Intent> Dance(Intent arg)
        {
            var turnRight = Observable.Return(new Intent("Turn") { { "Rotation", "10" } });
            var turnLeft = Observable.Return(new Intent("Turn") { { "Rotation", "-10" } });
            var spinAround = Observable.Return(new Intent("Turn") { { "Rotation", "360" } });
            var stopMoving = Observable.Return(new Intent("StopMoving"));
            return turnLeft.Concat(turnRight).Concat(spinAround).Concat(stopMoving);
        }
    }
}
