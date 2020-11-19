using ExpressBase.Mobile.Data;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbMobileDashBoardControl : EbMobilePageBase
    {
        public virtual View XView { set; get; }

        public EbThickness Margin { set; get; }

        public EbThickness Padding { set; get; }

        public virtual int BorderRadius { get; set; }

        public virtual string BorderColor { set; get; }

        public virtual string BackgroundColor { set; get; }

        public virtual bool BoxShadow { set; get; }

        public virtual void InitXControl() { }

        public virtual void InitXControl(EbDataRow DataRow) { }
    }
}
