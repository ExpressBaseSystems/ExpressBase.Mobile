using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.CustomControls.XControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbMobileDataLabel : EbMobileDashBoardControl, IGridAlignment
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

        private EbXLabel label;

        public override View Draw()
        {
            label = new EbXLabel
            {
                XBackgroundColor = Color.FromHex(this.BackgroundColor),
                BorderRadius = this.BorderRadius,
                BorderThickness = this.BorderThickness,
                BorderColor = Color.FromHex(this.BorderColor),
                Text = this.Text
            };

            if (this.RenderAsIcon && string.IsNullOrEmpty(Icon))
            {
                label.FontFamily = (OnPlatform<string>)HelperFunctions.GetResourceValue("FontAwesome");
                label.Text = this.Icon.ToFontIcon();
            }

            if (this.Font != null)
            {
                label.SetFont(this.Font);
            }

            label.SetHorrizontalAlign(this.HorrizontalAlign);
            label.SetVerticalAlign(this.VerticalAlign);

            return label;
        }

        public override void SetBindingValue(EbDataRow row)
        {
            if (!string.IsNullOrEmpty(this.BindingParam))
            {
                object value = GetBinding(row, this.BindingParam);
                label.Text = value?.ToString();
            } 
        }
    }
}
