using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Fatty.Brain.Extensions;
using Fatty.Brain.Modules;
using Fatty.Brain.Modules.Output;

namespace Fatty.Brain.Cognition
{
    public sealed class Manners : InterpreterBase
    {
        private readonly RandomQueue<string> greetings = new RandomQueue<string>()
        {
            "What's up everybody? My name is Fatty",
            "What's going on everyone? Fatty is my name",
            "Hello! My name is Fatty, the Hamster Robot!",
        };

        private readonly RandomQueue<string> apologies = new RandomQueue<string>()
        {
            "I'm very sorry, {0}",
            "Sorry {0}",
            "Sorry about this, {0}",
        };

        private readonly RandomQueue<string> thanks = new RandomQueue<string>()
        {
            "No problem",
            "You're most welcome!",
            "Don't mention it",
        };

        private readonly RandomQueue<string> askFor = new RandomQueue<string>()
        {
            "Please may I {0}",
            "If it's not too much to ask, can I {0}",
            "Is it possible for me to {0}",
        };

        public Manners()
        {
            this.Interpretations.Add("No", _ => Observable.Return(new Intent("ApologizeFor", _)));
            this.Interpretations.Add("AskFor", _ => this.Say(this.askFor.Next(), _.Values.First()));
            this.Interpretations.Add("Thanks", _ => this.Say(this.thanks.Next()));
            this.Interpretations.Add("Error", error => Observable.Return(new Intent("ApologizeFor", error)));
            this.Interpretations.Add("ApologizeFor", _ => this.Say(this.apologies.Next(), _.Values.First()));
            this.Interpretations.Add("Hello", _ => this.Say(this.greetings.Next()));
        }

        protected override IObservable<Intent> InitializeAsync()
        {
            return from allReady in this.Inputs.WhenReady(new Unit(), "SpeechSynthesis")
                   from delay in Observable.Timer(TimeSpan.FromSeconds(1))
                   from intent in this.Say(this.greetings.Next())
                   select intent;
        }
    }
}
