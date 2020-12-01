using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Views.Base;
using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls
{
    public class EmptyBox : StackLayout
    {
        public static readonly BindableProperty MessageProperty =
            BindableProperty.Create(nameof(Message), typeof(string), typeof(EmptyBox));

        public static readonly BindableProperty ShowReloadButtonProperty =
            BindableProperty.Create(nameof(ShowReloadButton), typeof(bool), typeof(EmptyBox));

        public static readonly BindableProperty ReloadButtonTextProperty =
            BindableProperty.Create(nameof(ReloadButtonText), typeof(string), typeof(EmptyBox), defaultValue: "Reload");

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public bool ShowReloadButton
        {
            get { return (bool)GetValue(ShowReloadButtonProperty); }
            set { SetValue(ShowReloadButtonProperty, value); }
        }

        public string ReloadButtonText
        {
            get { return (string)GetValue(ReloadButtonTextProperty); }
            set { SetValue(ReloadButtonTextProperty, value); }
        }

        public event EbEventHandler ReloadClicked;

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

            if (ShowReloadButton)
            {
                Button button = new Button
                {
                    Text = ReloadButtonText,
                    BackgroundColor = (Color)HelperFunctions.GetResourceValue("Primary_Color"),
                    TextColor = Color.White,
                    HorizontalOptions = LayoutOptions.Center,
                    Margin = new Thickness(0, 15, 0, 0),
                    Padding = 0,
                    CornerRadius = 6,
                    HeightRequest = 45
                };

                button.Clicked += (sender, e) => ReloadClicked?.Invoke(button, null);
                this.Children.Add(button);
            }
        }
    }
}
