
namespace Fatty.UserInterface
{
    using System;
    using System.Reactive.Linq;
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

            return changes.SubscribeOnDispatcher().Select(t => this.Parse(this.textBox.Text)).Subscribe(observer);
        }

        private Intent Parse(string text)
        {
            // TODO: Support args parsing
            return new Intent(text);
        }
    }
}
