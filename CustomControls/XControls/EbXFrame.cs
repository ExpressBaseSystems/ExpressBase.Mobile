using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace ExpressBase.Mobile.CustomControls
{
    [Preserve(AllMembers = true)]
    public class EbXFrame : Frame
    {
        public static readonly BindableProperty ShadowColorProperty = 
            BindableProperty.Create(nameof(ShadowColor), typeof(Color), typeof(EbXFrame), Color.Transparent);

        public static readonly BindableProperty BorderWidthProperty = 
            BindableProperty.Create(nameof(BorderWidth), typeof(float), typeof(EbXFrame));

        public Color ShadowColor
        {
            get { return (Color)GetValue(ShadowColorProperty); }
            set { SetValue(ShadowColorProperty, value); }
        }


        public float BorderWidth
        {
            get { return (float)GetValue(BorderWidthProperty); }
            set { SetValue(BorderWidthProperty, value); }
        }
    }
}
