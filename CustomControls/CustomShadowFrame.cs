using ExpressBase.Mobile.Models;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace ExpressBase.Mobile.CustomControls
{
    [Preserve(AllMembers = true)]
    public class CustomShadowFrame : Frame
    {
        public MobilePagesWraper PageWraper { set; get; }

        public CustomShadowFrame() { }

        public CustomShadowFrame(MobilePagesWraper wrpr)
        {
            PageWraper = wrpr;
   
            this.BackgroundColor = wrpr.IconBackground;
            this.BorderColor = wrpr.IconBackground == Color.White ? Color.FromHex("fafafa") : wrpr.IconBackground;
        }
    }
}
