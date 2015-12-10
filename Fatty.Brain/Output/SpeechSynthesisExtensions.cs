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
        public static IObservable<Intent> Say(this InterpreterBase source, string text)
        {
            return Observable.Return(
                new Intent("Say")
                {
                    {
                        "Text", text
                    }
                });
        }

        public static IObservable<Intent> Say(this InterpreterBase source, string text, params string[] args)
        {
            return Observable.Return(
                new Intent("Say")
                {
                    {
                        "Text", string.Format(text, args)
                    }
                });
        }
    }
}
