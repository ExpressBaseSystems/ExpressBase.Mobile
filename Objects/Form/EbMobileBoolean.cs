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

        public override View Draw(FormMode Mode, NetworkMode Network)
        {
            CheckBox check = new CheckBox
            {
                Color = (Color)HelperFunctions.GetResourceValue("Primary_Color")
            };

            check.CheckedChanged += (sender, arg) => this.ValueChanged();
            XControl = check;

            return base.Draw(Mode, Network);
        }

        public override object GetValue()
        {
            return (this.XControl as CheckBox).IsChecked;
        }

        public override void SetValue(object value)
        {
            try
            {
                if (value != null)
                {
                    bool isChecked = false;

                    if (value is int)
                        isChecked = Convert.ToInt32(value) != 0;
                    else if (value is bool boolean)
                        isChecked = boolean;
                    else if (value is string s)
                        isChecked = bool.Parse(s);

                    (this.XControl as CheckBox).IsChecked = isChecked;
                }
            }
            catch (Exception ex)
            {
                EbLog.Error("Boolean setvalue error");
                EbLog.Error(ex.Message);
            }
        }

        public override void Reset()
        {
            (this.XControl as CheckBox).ClearValue(CheckBox.IsCheckedProperty);
        }

        public override bool Validate()
        {
            if (this.Required && !(this.XControl as CheckBox).IsChecked)
                return false;

            return true;
        }
    }
}
