using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls
{
    public class EmptyBox : StackLayout
    {
        public static readonly BindableProperty MessageProperty =
            BindableProperty.Create(propertyName: "Message", typeof(string), typeof(string));

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public Label messageLabel;

        public EmptyBox() : base()
        {
            this.VerticalOptions = LayoutOptions.Center;

            this.Children.Add(new Image
            {
                Source = "mt.png",
                HeightRequest = 70
            });
            messageLabel = new Label
            {
                HorizontalTextAlignment = TextAlignment.Center
            };
            this.Children.Add(messageLabel);
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            messageLabel.Text = Message;
        }
    }
}
