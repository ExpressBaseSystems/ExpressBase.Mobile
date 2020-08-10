using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Structures;
using System;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbMobileDateTime : EbMobileControl
    {
        public EbDateType EbDateType { get; set; }

        public override EbDbTypes EbDbType { get { return (EbDbTypes)this.EbDateType; } set { } }

        public bool IsNullable { get; set; }

        public bool BlockBackDatedEntry { set; get; }

        public bool BlockFutureDatedEntry { set; get; }

        private CustomDatePicker datePicker;

        private CustomTimePicker timePicker;

        public override object SQLiteToActual(object value)
        {
            if (this.EbDbType == EbDbTypes.Date)
                return Convert.ToDateTime(value).Date.ToString("yyyy-MM-dd");
            else if (this.EbDbType == EbDbTypes.DateTime)
                return Convert.ToDateTime(value).Date.ToString("yyyy-MM-dd HH:mm:ss");

            return value.ToString();
        }

        public override void InitXControl(FormMode Mode, NetworkMode Network)
        {
            base.InitXControl(Mode, Network);
            Color bg = this.ReadOnly ? Color.FromHex("eeeeee") : Color.Transparent;

            View control;

            if (EbDateType == EbDateType.Time)
            {
                timePicker = new CustomTimePicker
                {
                    IsEnabled = !this.ReadOnly,
                    BorderColor = Color.Transparent
                };
                control = timePicker;
            }
            else
            {
                datePicker = new CustomDatePicker
                {
                    IsEnabled = !this.ReadOnly,
                    Date = DateTime.UtcNow,
                    BorderColor = Color.Transparent
                };

                if (BlockBackDatedEntry) datePicker.MinimumDate = DateTime.UtcNow;
                if (BlockFutureDatedEntry) datePicker.MaximumDate = DateTime.UtcNow;

                control = datePicker;
            }

            Label icon = new Label
            {
                Padding = 10,
                FontSize = 18,
                VerticalOptions = LayoutOptions.Center,
                Text = EbDateType == EbDateType.Time ? "\uf017" : "\uf073",
                FontFamily = (OnPlatform<string>)HelperFunctions.GetResourceValue("FontAwesome")
            };

            this.XControl = new InputGroup(control, icon) { BgColor = bg };
        }

        public override object GetValue()
        {
            string value;
            switch (this.EbDbType)
            {
                case EbDbTypes.Date:
                    value = datePicker.Date.ToString("yyyy-MM-dd");
                    break;
                case EbDbTypes.DateTime:
                    value = datePicker.Date.ToString("yyyy-MM-dd HH:mm:ss");
                    break;
                case EbDbTypes.Time:
                    value = timePicker.Time.ToString();
                    break;
                default:
                    value = null;
                    break;
            }
            return value;
        }

        public override bool SetValue(object value)
        {
            if (value == null)
                return false;

            try
            {
                if (this.EbDateType == EbDateType.Time)
                {
                    timePicker.Time = TimeSpan.Parse(value.ToString());
                }
                else
                {
                    datePicker.Date = Convert.ToDateTime(value);
                }
            }
            catch (Exception ex)
            {
                EbLog.Write(ex.Message);
                EbLog.Write(ex.StackTrace);
                return false;
            }
            return true;
        }

        public override void Reset()
        {
            if (this.EbDateType == EbDateType.Time)
                timePicker.ClearValue(CustomTimePicker.TimeProperty);
            else
                datePicker.ClearValue(CustomDatePicker.DateProperty);
        }
    }
}
