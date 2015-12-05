using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fatty.Brain.Modules.Output
{
    public static class SpeechSynthesisExtensions
    {
        public static IObservable<Intent> Say(this IObservable<Intent> source, string text)
        {
            return source.Select(_ => new Intent("TextToSpeech")
                {
                    {
                        "Text", text
                    }
                });
        }
    }
}
