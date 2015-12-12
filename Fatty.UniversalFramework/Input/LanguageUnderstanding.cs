using System;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Fatty.Brain;
using Fatty.Brain.Modules;

namespace Fatty.UniversalFramework.Input
{
    public sealed class LanguageUnderstanding : InterpreterBase
    {
        private const string BaseUrl = "https://api.projectoxford.ai/luis/v1/application?id=cd20706f-a0eb-4424-81ac-b81276456d57&subscription-key=96ad298e0cd746f1bcf8df52a6c31f40&q=";

        public LanguageUnderstanding()
        {
            this.Interpretations.Add("Heard", heard => this.TryToUnderstand(heard.Values.FirstOrDefault()));
        }

        private IObservable<Intent> TryToUnderstand(string text)
        {
            return Observable.FromAsync(() => this.TryToUnderstandAsync(text));
        }

        private async Task<Intent> TryToUnderstandAsync(string text)
        {
            var client = new HttpClient();
            var requestUrl = BaseUrl + Uri.EscapeUriString(text.ToLowerInvariant());
            var response = await client.GetAsync(requestUrl);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var query = Newtonsoft.Json.JsonConvert.DeserializeObject<LuisQuery>(json);
                if (query != null)
                {
                    var intent = query.Intents.FirstOrDefault();
                    if (intent != null)
                    {
                        if (intent.Intent == "None")
                        {
                            return null;
                        }

                        var result = new Intent(intent.Intent, intent.Score, 2);
                        foreach (var entity in query.Entities)
                        {
                            result.Add(entity.Type, entity.Entity);
                        }

                        return result;
                    }
                }
            }

            return null;
        }
    }
}