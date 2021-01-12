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

        public MobileTextAlign HorrizontalTextAlign { set; get; }

        public MobileTextAlign VerticalTextAlign { set; get; }

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

        public override View Draw()
        {
            EbXLabel label = new EbXLabel(this)
            {
                Text = this.Text
            };

            label.SetTextWrap(TextWrap);
            label.SetTextAlignment(HorrizontalTextAlign, VerticalTextAlign);

            if (RenderAsIcon && !string.IsNullOrEmpty(Icon))
            {
                label.FontFamily = (OnPlatform<string>)HelperFunctions.GetResourceValue("FontAwesome");
                label.Text = Icon.ToFontIcon();
            }
            return label;
        }
    }
}
