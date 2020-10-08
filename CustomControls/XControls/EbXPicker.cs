using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls
{
    public class EbXPicker : Picker, IEbCustomControl
    {
        public static readonly BindableProperty XBackgroundColorProperty =
          BindableProperty.Create(nameof(XBackgroundColor), typeof(Color), typeof(EbXPicker));

        public int BorderThickness { set; get; } = 1;

        public float BorderRadius { set; get; } = 10.0f;

        public Color BorderColor { set; get; } = Color.FromHex("cccccc");

        public Color XBackgroundColor
        {
            get { return (Color)GetValue(XBackgroundColorProperty); }
            set { SetValue(XBackgroundColorProperty, value); }
        }

        public EbXPicker() { }
    }
}
