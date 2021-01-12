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

        public override View Draw(FormMode Mode, NetworkMode Network)
        {
            XControl = new EbXTextBox
            {
                IsReadOnly = true,
                XBackgroundColor = EbMobileControl.ReadOnlyBackground
            };

            return base.Draw(Mode, Network);
        }

        public override void SetValue(object value)
        {
            (this.XControl as EbXTextBox).Text = value?.ToString();
        }

        public override bool Validate()
        {
            bool isNull = string.IsNullOrEmpty((this.XControl as EbXTextBox).Text);
            if (this.Required && isNull)
                return false;

            return true;
        }
    }
}
