using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls
{
    public class TextBox : Entry, IEbCustomControl
    {
        public static readonly BindableProperty BorderOnFocusProperty =
            BindableProperty.Create(nameof(BorderOnFocus), typeof(Color), typeof(TextBox));

        public int BorderThickness { set; get; } = 1;

        public float BorderRadius { set; get; } = 10.0f;

        public Color BorderColor { set; get; } = Color.FromHex("cccccc");

        public Color BgColor { set; get; }

        public bool EnableFocus { set; get; }

        public Color BorderOnFocus
        {
            get { return (Color)GetValue(BorderOnFocusProperty); }
            set { SetValue(BorderOnFocusProperty, value); }
        }

        public TextBox() { }
    }
}
