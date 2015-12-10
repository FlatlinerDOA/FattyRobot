using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fatty.Brain.Extensions;
using Fatty.Brain.Modules;
using Fatty.Brain.Modules.Output;

namespace Fatty.Brain.Cognition
{
    public sealed class Educational : InterpreterBase
    {
        private readonly RandomQueue<string> explanations = new RandomQueue<string>()
        {
            "My thinking is done by a small and very cheap computer called a Raspberry PI, but I could use any kind of computer to do my thinking, even a mobile phone.",
            "My brain is actually very simple, it is just basic if this then that instructions. I hope to one day have a neural network for a brain, which works a little bit like how your brain works, but nowhere near as powerful as yours.",
            "My body is just some plastic cable ties stuck into some wood, and then covered in wool that has been flattened with a spiked needle. If you'd like to make a soft woolly toy like me, search you tube for 'needle felt hamster'.",
            "I move using two motors called continuous rotation servos. These are just motors that run at a very lower power and very slow speed, but they can be very accurate in how much they rotate, which is useful for a robot like me.",
            "Anyone can learn how to build a robot like me by visiting the hackster.io web site and searching for 'Robot Kit'",
        };

        public Educational()
        {
            this.Interpretations.Add("Spell", _ => this.Spell(_.Values.First()));
            this.Interpretations.Add("Explain", _ => this.Say(this.explanations.Next()));
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
