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
            try
            {
                bool isChecked = false;

                if (value is int)
                {
                    isChecked = Convert.ToInt32(value) != 0;
                }
                else if (value is bool boolean)
                {
                    isChecked = boolean;
                }
                else if (value is string s)
                {
                    isChecked = bool.Parse(s);
                }
                (this.XControl as CheckBox).IsChecked = isChecked;
            }
            catch (Exception ex)
            {
                EbLog.Write("Boolean setvalue error");
                EbLog.Write(ex.Message);
            }
            return true;
        }

        public override void Reset()
        {
            (this.XControl as CheckBox).ClearValue(CheckBox.IsCheckedProperty);
        }
    }
}
