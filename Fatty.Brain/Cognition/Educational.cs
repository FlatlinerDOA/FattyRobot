using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fatty.Brain.Modules;
using Fatty.Brain.Modules.Output;

namespace Fatty.Brain.Cognition
{
    public sealed class Educational : InterpreterBase
    {
        public Educational()
        {
            this.Interpretations.Add("Spell", _ => this.Spell(_.Values.First()));
            ////this.Interpretations.Add("Explain", this.Lookup());
        }

        private IObservable<Intent> Spell(string text)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                sb.Append(text[i]);
                sb.Append(", ");
            }

            sb.Append(text);
            return this.Say(sb.ToString());
        }
    }
}
