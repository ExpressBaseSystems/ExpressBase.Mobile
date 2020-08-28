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

        private Color Background => this.ReadOnly ? Color.FromHex("eeeeee") : Color.Transparent;

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
            View control;

            TapGestureRecognizer gesture = new TapGestureRecognizer();
            gesture.Tapped += Icon_Tapped;

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
                if (this.BlockBackDatedEntry) datePicker.MinimumDate = DateTime.UtcNow;
                if (this.BlockFutureDatedEntry) datePicker.MaximumDate = DateTime.UtcNow;
                control = datePicker;
            }

            Label icon = new Label
            {
                Style = (Style)HelperFunctions.GetResourceValue("DatePickerIcon"),
                Text = EbDateType == EbDateType.Time ? "\uf017" : "\uf073",
                GestureRecognizers = { gesture }
            };

            this.XControl = new InputGroup(control, icon) { BgColor = Background };
        }

        private void Icon_Tapped(object sender, EventArgs e)
        {
            if (EbDateType == EbDateType.Time)
                timePicker?.Focus();
            else
                datePicker?.Focus();
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

        public override void SetValue(object value)
        {
            try
            {
                if (value != null)
                {
                    if (this.EbDateType == EbDateType.Time)
                        timePicker.Time = TimeSpan.Parse(value.ToString());
                    else
                        datePicker.Date = Convert.ToDateTime(value);
                }
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
                EbLog.StackTrace(ex.StackTrace);
            }
        }

        public override void Reset()
        {
            if (this.EbDateType == EbDateType.Time)
                timePicker.ClearValue(CustomTimePicker.TimeProperty);
            else
                datePicker.ClearValue(CustomDatePicker.DateProperty);
        }

        public override bool Validate()
        {
            return base.Validate();
        }
    }
}
