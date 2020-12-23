using ExpressBase.Mobile.CustomControls.XControls;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbMobileLabel : EbMobileControl, INonPersistControl, IGridAlignment, IMobileUIControl
    {
        public string Text { get; set; }

        public bool RenderAsIcon { set; get; }

        public string Icon { set; get; }

        public EbFont Font { get; set; }

        public MobileTextWrap TextWrap { set; get; }

        public int BorderRadius { get; set; }

        public string BackgroundColor { get; set; }

        public string BorderColor { get; set; }

        public int BorderThickness { get; set; }

        public int RowSpan { set; get; }

        public int ColumnSpan { set; get; }

        public MobileHorrizontalAlign HorrizontalAlign { set; get; }

        public MobileVerticalAlign VerticalAlign { set; get; }

        public int Height { set; get; }

        public int Width { set; get; }

        public EbThickness Padding { set; get; }

        public EbMobileStaticParameter BindingParam { set; get; }

        public EbXLabel Draw()
        {
            EbXLabel label = new EbXLabel
            {
                Text = this.Text,
                XBackgroundColor = Color.FromHex(BackgroundColor ?? "#ffffff"),
                BorderRadius = BorderRadius,
                BorderColor = Color.FromHex(BackgroundColor ?? "#ffffff"),
                Padding = this.Padding == null ? 0 : this.Padding.ConvertToXValue(),
                BorderThickness = BorderThickness
            };

            if (RenderAsIcon)
            {
                label.FontFamily = (OnPlatform<string>)HelperFunctions.GetResourceValue("FontAwesome");
            }
            return label;
        }
    }
}
