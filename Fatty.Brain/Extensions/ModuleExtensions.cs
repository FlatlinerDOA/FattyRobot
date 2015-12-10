using System;
using System.Linq;
using System.Reactive.Linq;

namespace Fatty.Brain.Modules
{
    public static class ModuleExtensions
    {
        public static IObservable<Intent> IsReady(this InterpreterBase interpreter)
        {
            return Observable.Return(new Intent("Ready") { { "Module", interpreter.GetType().Name } });
        }

        public static IObservable<T> WhenReady<T>(this IObservable<Intent> intents, T result, params string[] moduleNames)
        {
            var moduleStatusChanges = moduleNames.Select(moduleName => intents.Where(i => i.IsNamed(moduleName)));
            return Observable.CombineLatest(moduleStatusChanges)
                .Where(changes => changes.All(i => i["Status"] == "Ready"))
                .Take(1)
                .SelectMany(_ => Observable.Return(result));
        }
    }
}