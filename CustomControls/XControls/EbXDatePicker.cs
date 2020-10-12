using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls
{
    public class EbXDatePicker : DatePicker, IEbCustomControl
    {
        public static readonly BindableProperty XBackgroundColorProperty =
           BindableProperty.Create(nameof(XBackgroundColor), typeof(Color), typeof(EbXDatePicker));

        public static readonly BindableProperty BorderColorProperty =
           BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(EbXDatePicker), defaultValue: Color.FromHex("cccccc"));

        public int BorderThickness { set; get; } = 1;

        public float BorderRadius { set; get; } = 10.0f;

        public Color BorderColor
        {
            get { return (Color)GetValue(BorderColorProperty); }
            set { SetValue(BorderColorProperty, value); }
        }

        public Color XBackgroundColor
        {
            get { return (Color)GetValue(XBackgroundColorProperty); }
            set { SetValue(XBackgroundColorProperty, value); }
        }

        public EbXDatePicker() { }
    }
}
