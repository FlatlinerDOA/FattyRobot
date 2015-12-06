
namespace Fatty.UserInterface
{
    using System;
    using System.Reactive.Linq;
    using Brain.Extensions;
    using Fatty.Brain;
    using Windows.UI.Xaml.Controls;

    public sealed class TextInput : IObservable<Intent>
    {
        private TextBox textBox;

        public TextInput(TextBox textBox)
        {
            this.textBox = textBox;
        }

        public IDisposable Subscribe(IObserver<Intent> observer)
        {
            var changes = Observable.FromEventPattern<TextChangedEventHandler, TextChangedEventArgs>(
                handler => this.textBox.TextChanged += handler,
                handler => this.textBox.TextChanged -= handler);

            return changes.SubscribeOnDispatcher().Select(t =>
            {
                var result = this.Parse(this.textBox.Text);
                if (result != null)
                {
                    this.textBox.Text = string.Empty;
                }

                return result;
            }).Subscribe(observer);
        }

        private Intent Parse(string text)
        {
            if (string.IsNullOrWhiteSpace(text) || !text.Contains("\n"))
            {
                // No intent yet
                return null;
            }

            text = text.Trim('\r', '\n');
            var intent = new Intent(text.SubstringToFirst(" "));
            var args = text.SubstringFromFirst(" ").Split(';');
            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    var key = args[i].SubstringToFirst("=");
                    var value = args[i].SubstringFromFirst("=");
                    intent[key] = value;
                }
            }

            return intent;
        }
    }
}
