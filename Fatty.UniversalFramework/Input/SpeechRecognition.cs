namespace Fatty.Brain.Modules.Input
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Concurrency;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;
    using System.Reactive.Threading.Tasks;
    using System.Text;
    using System.Threading.Tasks;
    using Fatty.Brain.Modules.Cognition;
    using Windows.Foundation;
    using Windows.Globalization;
    using Windows.Media.SpeechRecognition;
    using Windows.UI.Core;

    public sealed class SpeechRecognition : InterpreterBase
    {
        public const string Listen = "Listen";

        public const string Listening = "Listening";

        public const string StopListening = "StopListening";

        public const string StoppedListening = "StoppedListening";

        private SpeechRecognizer speechRecognizer;

        private MultipleAssignmentDisposable speechSubscriptions = new MultipleAssignmentDisposable();

        private bool isListening;

        private static uint HResultPrivacyStatementDeclined = 0x80045509;

        public SpeechRecognition(IScheduler coreDispatcher) : base(coreDispatcher)
        {
            this.Interpretations.Add(Listen, this.ListenAsync);
            this.Interpretations.Add(StopListening, this.StopListeningAsync);

            this.Interpretations.Add("Saying", _ => Observable.Return(new Intent("StopListening")));
            this.Interpretations.Add("DoneSaying", _ => Observable.Return(new Intent("Listen")));
        }

        private IObservable<Intent> ListenAsync(Intent arg)
        {
            return Observable.FromAsync(this.ListenAsync);
        }

        private IObservable<Intent> StopListeningAsync(Intent arg)
        {
            return Observable.FromAsync(this.StopListeningAsync);
        }

        private async Task<Intent> ListenAsync()
        {
            this.isListening = true;
            await this.speechRecognizer.ContinuousRecognitionSession.StartAsync();
            return new Intent("Listening");
        }

        private async Task<Intent> StopListeningAsync()
        {
            this.isListening = false;
            await this.speechRecognizer.ContinuousRecognitionSession.StopAsync();
            return new Intent("StoppedListening");
        }

        protected override IObservable<Intent> InitializeAsync()
        {
            return this.InitializeRecognizerAsync().ToObservable();
        }

        private async Task<Intent> InitializeRecognizerAsync()
        {
            this.speechSubscriptions.Disposable = Disposable.Empty;

            this.speechRecognizer = new SpeechRecognizer();

            SpeechRecognitionCompilationResult result = await this.speechRecognizer.CompileConstraintsAsync();
            if (result.Status != SpeechRecognitionResultStatus.Success)
            {
                return new Intent("Error") { { "Message", "My ability to hear and understand you is currently impaired" } };
            }

            // Handle continuous recognition events. Completed fires when various error states occur. ResultGenerated fires when
            // some recognized phrases occur, or the garbage rule is hit. HypothesisGenerated fires during recognition, and
            // allows us to provide incremental feedback based on what the user's currently saying.
            var subs = new CompositeDisposable
            {
                Observable.FromEventPattern<TypedEventHandler<SpeechContinuousRecognitionSession, SpeechContinuousRecognitionCompletedEventArgs>, SpeechContinuousRecognitionCompletedEventArgs>(
                    handler => speechRecognizer.ContinuousRecognitionSession.Completed += handler,
                    handler => speechRecognizer.ContinuousRecognitionSession.Completed -= handler)
                    .SubscribeOn(this.ObserveOn)
                    .ObserveOn(this.ObserveOn)
                    .Subscribe(e => this.Completed(e.EventArgs)),
                Observable.FromEventPattern<TypedEventHandler<SpeechContinuousRecognitionSession, SpeechContinuousRecognitionResultGeneratedEventArgs>, SpeechContinuousRecognitionResultGeneratedEventArgs>(
                    handler => speechRecognizer.ContinuousRecognitionSession.ResultGenerated += handler,
                    handler => speechRecognizer.ContinuousRecognitionSession.ResultGenerated -= handler)
                    .SubscribeOn(this.ObserveOn)
                    .ObserveOn(this.ObserveOn)
                    .Subscribe(e => this.ResultGenerated(e.EventArgs)),
                Observable.FromEventPattern<TypedEventHandler<SpeechRecognizer, SpeechRecognitionHypothesisGeneratedEventArgs>, SpeechRecognitionHypothesisGeneratedEventArgs>(
                    handler => speechRecognizer.HypothesisGenerated += handler,
                    handler => speechRecognizer.HypothesisGenerated -= handler)
                    .SubscribeOn(this.ObserveOn)
                    .ObserveOn(this.ObserveOn)
                    .Subscribe(e => this.HypothesisGenerated(e.EventArgs)),
            };

            try
            {
                await this.ListenAsync();
            }
            catch (Exception ex)
            {
                this.isListening = false;
                if ((uint)ex.HResult == HResultPrivacyStatementDeclined)
                {
                    return new Intent("Error") { { "Message", "Please accept the privacy statement, don't worry I won't tell anyone what we talk about." } };
                }
                else
                {
                    return new Intent("Error") { { "Message", ex.Message } };
                }

            }

            this.speechSubscriptions.Disposable = subs;
            return this.Ready();
        }

        private void HypothesisGenerated(SpeechRecognitionHypothesisGeneratedEventArgs args)
        {
            this.Send(new Intent("Hearing", 0.1, 3) { { "Text", args.Hypothesis.Text } });
        }

        private void ResultGenerated(SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {            
            this.Send(new Intent("Heard", args.Result.RawConfidence, 1) { { "Text", args.Result.Text } });
        }

        private async void Completed(SpeechContinuousRecognitionCompletedEventArgs args)
        {
            if (args.Status == SpeechRecognitionResultStatus.TimeoutExceeded)
            {
                this.Send(new Intent("Listen"));
            }

            this.Send(new Intent("Ignored") { { "Status", args.Status.ToString() } });
        }
    }
}
