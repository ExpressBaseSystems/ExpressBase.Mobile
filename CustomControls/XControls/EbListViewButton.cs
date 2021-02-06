using ExpressBase.Mobile.Helpers;
using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls
{
    public class EbListViewButton : Button
    {
        public EbListViewButton() { }

        public EbListViewButton(EbMobileDataColumn dc)
        {
            if (!string.IsNullOrEmpty(dc.BackgroundColor))
                BackgroundColor = Color.FromHex(dc.BackgroundColor);

            if (!string.IsNullOrEmpty(dc.BorderColor))
                BorderColor = Color.FromHex(dc.BorderColor);

            CornerRadius = dc.BorderRadius;
            BorderWidth = dc.BorderThickness;
            Padding = dc.Padding == null ? 0 : dc.Padding.ConvertToXValue();

            if (dc.Width > 0) this.WidthRequest = dc.Width;
            if (dc.Height > 0) this.HeightRequest = dc.Height;

            SetText(dc);

            if (dc.Font != null)
            {
                this.FontSize = dc.Font.Size;
                this.TextColor = Color.FromHex(dc.Font.Color ?? "#333333");
            }
        }

        private void SetText(EbMobileDataColumn dc)
        {
            this.TextTransform = TextTransform.None;

            if (string.IsNullOrEmpty(dc.TextFormat))
            {
                this.FontFamily = (OnPlatform<string>)HelperFunctions.GetResourceValue("FontAwesome");
                this.Text = "\uf041";
            }
            else
            {
                this.Text = dc.TextFormat;
            }
        }
    }
}
