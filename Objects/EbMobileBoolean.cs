using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Structures;
using System;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbMobileBoolean : EbMobileControl
    {
        public override EbDbTypes EbDbType { get { return EbDbTypes.BooleanOriginal; } set { } }

        public override object SQLiteToActual(object value)
        {
            if (Convert.ToInt32(value) == 0)
                return false;
            else
                return true;
        }

        public override void InitXControl(FormMode Mode, NetworkMode Network)
        {
            base.InitXControl(Mode, Network);
            this.XControl = new CheckBox
            {
                Color = (Color)HelperFunctions.GetResourceValue("Primary_Color")
            };
        }

        public override object GetValue()
        {
            return (this.XControl as CheckBox).IsChecked;
        }

        public override bool SetValue(object value)
        {
            if (value == null)
                return false;

            int val = Convert.ToInt32(value);
            (this.XControl as CheckBox).IsChecked = val != 0;
            return true;
        }

        public override void Reset()
        {
            (this.XControl as CheckBox).ClearValue(CheckBox.IsCheckedProperty);
        }
    }
}
