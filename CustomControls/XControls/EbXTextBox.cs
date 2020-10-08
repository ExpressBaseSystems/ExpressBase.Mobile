using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls
{
    public class EbXTextBox : Entry, IEbCustomControl
    {
        public static readonly BindableProperty BorderOnFocusProperty =
            BindableProperty.Create(nameof(BorderOnFocus), typeof(Color), typeof(EbXTextBox));

        public static readonly BindableProperty XBackgroundColorProperty =
           BindableProperty.Create(nameof(XBackgroundColor), typeof(Color), typeof(EbXTextBox));

        public int BorderThickness { set; get; } = 1;

        public float BorderRadius { set; get; } = 10.0f;

        public Color BorderColor { set; get; } = Color.FromHex("cccccc");

        public Color XBackgroundColor
        {
            get { return (Color)GetValue(XBackgroundColorProperty); }
            set { SetValue(XBackgroundColorProperty, value); }
        }

        public bool EnableFocus { set; get; }

        public Color BorderOnFocus
        {
            get { return (Color)GetValue(BorderOnFocusProperty); }
            set { SetValue(BorderOnFocusProperty, value); }
        }

        public EbXTextBox() { }
    }
}
