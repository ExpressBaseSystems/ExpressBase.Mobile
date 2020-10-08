using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls
{
    public class EbXCheckBox : CheckBox, IEbCustomControl
    {
        public static readonly BindableProperty XBackgroundColorProperty =
          BindableProperty.Create(nameof(XBackgroundColor), typeof(Color), typeof(EbXCheckBox));

        public int BorderThickness { set; get; }

        public float BorderRadius { set; get; }

        public Color BorderColor { set; get; }

        public Color XBackgroundColor
        {
            get { return (Color)GetValue(XBackgroundColorProperty); }
            set { SetValue(XBackgroundColorProperty, value); }
        }

        public EbXCheckBox() { }
    }
}
