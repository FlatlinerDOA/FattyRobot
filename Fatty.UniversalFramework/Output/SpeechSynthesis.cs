using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Fatty.Brain;
using Fatty.Brain.Modules;
using SpeechTranslator;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Fatty.UniversalFramework.Output
{
    public sealed class SpeechSynthesis : InterpreterBase
    {
        public const string Say = "Say";

        public const string Stop = "Stop";

        public const string Saying = "Saying";

        public const string DoneSaying = "DoneSaying";

        private SpeechSynthesizer synthesizer;

        private bool isListening;

        private string voiceMatchLanguageCode = "en";

        private CoreDispatcher dispatcher;

        private MediaElement media;

        public SpeechSynthesis(CoreDispatcher dispatcher, MediaElement media) : base(CoreDispatcherScheduler.Current)
        {
            this.media = media;
            this.dispatcher = dispatcher;
            this.Interpretations.Add(Say, this.SayAsync);
            this.Interpretations.Add(Stop, this.StopAsync);
        }

        private IObservable<Intent> StopAsync(Intent arg)
        {
            this.media.Stop();
            return Observable.Return<Intent>(new Intent("Interrupted"));
        }

        private IObservable<Intent> SayAsync(Intent arg)
        {
            return Observable.FromAsync(() => this.ReadTextAsync(arg["Text"]));
        }

        private async Task<Intent> ReadTextAsync(string text)
        {
            SpeechSynthesisStream stream = await synthesizer.SynthesizeTextToStreamAsync(text);
            await this.dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                media.SetSource(stream, stream.ContentType);
                media.Play();
            });

            var done = new Intent(Saying) { { "Text", text } };
            return done;
        }

        protected override IObservable<Intent> InitializeAsync()
        {
            return Observable.Defer(() =>
            {
                this.InitializeSynthesizer();
                return Observable.Return(this.Ready()).Merge(this.media.MediaEnds().SubscribeOn(this.ObserveOn).ObserveOn(this.ObserveOn).Select(_ => new Intent("DoneSaying")));
            });
        }

        public void InitializeSynthesizer()
        {
            if (synthesizer == null)
            {
                synthesizer = new SpeechSynthesizer();
            }

            // select the language display
            var voices = SpeechSynthesizer.AllVoices;
            foreach (VoiceInformation voice in voices)
            {
                if (voice.Language.Contains(voiceMatchLanguageCode))
                {
                    synthesizer.Voice = voice;
                }
            }
        }
    }
}
