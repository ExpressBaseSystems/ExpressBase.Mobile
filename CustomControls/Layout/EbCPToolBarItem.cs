using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Views.Base;
using System.Windows.Input;
using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls
{
    public class EbCPToolBarItem : Grid
    {
        public static readonly BindableProperty IconProperty =
            BindableProperty.Create(nameof(Icon), typeof(string), typeof(EbCPToolBarItem));

        public static readonly BindableProperty TextProperty =
            BindableProperty.Create(nameof(Text), typeof(string), typeof(EbCPToolBarItem));

        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(EbCPToolBarItem));

        public event EbEventHandler Clicked;

        public string Name { set; get; }

        public EbCPToolBarItem()
        {
            this.ColumnDefinitions.Add(new ColumnDefinition { Width = 30 });
            this.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

            TapGestureRecognizer gesture = new TapGestureRecognizer();
            gesture.Tapped += Gesture_Tapped;
            this.GestureRecognizers.Add(gesture);
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if (Icon != null)
            {
                Label lbl = new Label
                {
                    Text = Icon,
                    FontSize = 18,
                    TextColor = Color.FromHex("#737373"),
                    VerticalOptions = LayoutOptions.Center,
                    FontFamily = (OnPlatform<string>)HelperFunctions.GetResourceValue("FontAwesome")
                };
                this.Children.Add(lbl, 0, 0);
            }

            if (Text != null)
            {
                Label txt = new Label
                {
                    Text = Text,
                    FontSize = 15,
                    TextColor = Color.FromHex("#333333"),
                    VerticalOptions = LayoutOptions.Center,
                    LineBreakMode = LineBreakMode.TailTruncation,
                    FontFamily = (OnPlatform<string>)HelperFunctions.GetResourceValue("Roboto-Regular")
                };
                this.Children.Add(txt, 1, 0);
            }
        }

        private void Gesture_Tapped(object sender, System.EventArgs e)
        {
            if (Clicked != null)
                Clicked.Invoke(sender, e);
            if (Command != null)
                Command.Execute(null);

            EbCPLayout.SecondaryItemClicked(Name);
        }

        public string Icon
        {
            get { return (string)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
    }
}