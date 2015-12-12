
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
                if (string.IsNullOrWhiteSpace(this.textBox.Text) || !this.textBox.Text.Contains("\n"))
                {
                    return null;
                }

                var result = Intent.Parse(this.textBox.Text);
                if (result != null)
                {
                    this.textBox.Text = string.Empty;
                }

                return result;
            }).Subscribe(observer);
        }
    }
}
