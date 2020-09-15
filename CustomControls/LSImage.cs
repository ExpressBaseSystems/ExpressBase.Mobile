using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls
{
    public class LSImage : Image
    {
        public double InitialWidth { set; get; }

        public bool CalcHeight { set; get; }

        public void SetExpansion(EbMobileDataColumn dc)
        {
            if (dc.VerticalAlign == MobileVerticalAlign.Fill)
                this.CalcHeight = true;
            else
                this.HeightRequest = dc.Height;

            if (dc.HorrizontalAlign != MobileHorrizontalAlign.Fill)
                this.WidthRequest = dc.Width;
        }
    }
}
