using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Structures;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbMobileAutoId : EbMobileControl
    {
        public override bool ReadOnly { get { return true; } }

        public override EbDbTypes EbDbType { get { return EbDbTypes.String; } set { } }

        public override void InitXControl(FormMode Mode,NetworkMode Network)
        {
            base.InitXControl(Mode, Network);

            this.XControl = new TextBox
            {
                IsReadOnly = true,
                BgColor = Color.FromHex("eeeeee")
            };
        }

        public override bool SetValue(object value)
        {
            if (value == null)
                return false;

            (this.XControl as TextBox).Text = value.ToString();
            return true;
        }
    }
}
