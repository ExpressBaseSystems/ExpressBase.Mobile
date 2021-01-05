using ExpressBase.Mobile.CustomControls.XControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbMobileDataLabel : EbMobileDashBoardControl, IGridAlignment, IMobileUIControl
    {
        public string Text { get; set; }

        public EbFont Font { get; set; }

        public string BindingParam { set; get; }

        public bool RenderAsIcon { set; get; }

        public string Icon { set; get; }

        public int RowSpan { set; get; }

        public int ColumnSpan { set; get; }

        public MobileHorrizontalAlign HorrizontalAlign { set; get; }

        public MobileVerticalAlign VerticalAlign { set; get; }

        public MobileTextWrap TextWrap { set; get; }

        public MobileTextAlign HorrizontalTextAlign { set; get; }

        public MobileTextAlign VerticalTextAlign { set; get; }

        public int Height { set; get; }

        public int Width { set; get; }

        private EbXLabel label;

        public override View Draw()
        {
            label = new EbXLabel(this)
            {
                Text = Text
            };

            if (RenderAsIcon && !string.IsNullOrEmpty(Icon))
            {
                label.FontFamily = (OnPlatform<string>)HelperFunctions.GetResourceValue("FontAwesome");
                label.Text = Icon.ToFontIcon();
            }

            if (Font != null)
            {
                label.SetFont(Font);
            }

            label.SetHorrizontalAlign(HorrizontalAlign);
            label.SetVerticalAlign(VerticalAlign);
            label.SetTextWrap(TextWrap);
            label.SetTextAlignment(HorrizontalTextAlign, VerticalTextAlign);

            return label;
        }

        public override void SetBindingValue(EbDataSet dataSet)
        {
            if (!string.IsNullOrEmpty(BindingParam))
            {
                object value = GetBinding(dataSet, BindingParam);
                label.Text = value?.ToString();
            }
        }
    }
}
