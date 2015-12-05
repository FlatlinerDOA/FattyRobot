using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fatty.Brain.Modules.Output;

namespace Fatty.Brain.Modules.Cognition
{
    public sealed class Playful : InterpreterBase
    {
        public Playful()
        {
            this.WhenIHear("No").Say("Sorry");
            this.WhenIHear("Games").Say("Yeah!");
        }
    }
}
