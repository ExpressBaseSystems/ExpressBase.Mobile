using ExpressBase.Mobile.CustomControls.XControls;
using ExpressBase.Mobile.Helpers;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbMobileLabel : EbMobileControl, INonPersistControl, IMobileAlignment, IGridSpan
    {
        public string Text { get; set; }

        public bool RenderAsIcon { set; get; }

        public string Icon { set; get; }

        public EbFont Font { get; set; }

        public MobileTextWrap TextWrap { set; get; }

        public int BorderRadius { get; set; }

        public string BackgroundColor { get; set; }

        public int RowSpan { set; get; }

        public int ColumnSpan { set; get; }

        public MobileHorrizontalAlign HorrizontalAlign { set; get; }

        public MobileVerticalAlign VerticalAlign { set; get; }

        public int Height { set; get; }

        public int Width { set; get; }

        public EbMobileStaticParameter BindingParam { set; get; }

        public EbXLabel CreateXControl()
        {
            EbXLabel label = new EbXLabel
            {
                XBackgroundColor = Color.FromHex(BackgroundColor ?? "#ffffff"),
                BorderRadius = BorderRadius,
                BorderColor = Color.FromHex(BackgroundColor ?? "#ffffff")
            };

            if (RenderAsIcon)
            {
                label.FontFamily = (OnPlatform<string>)HelperFunctions.GetResourceValue("FontAwesome");
            }
            return label;
        }
    }
}
