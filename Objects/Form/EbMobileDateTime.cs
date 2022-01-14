using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Structures;
using System;
using System.ComponentModel;
using System.Globalization;
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

        private EbXDatePicker datePicker;

        private EbXTimePicker timePicker;

        public override object SQLiteToActual(object value)
        {
            if (this.EbDbType == EbDbTypes.Date)
                return Convert.ToDateTime(value).Date.ToString("yyyy-MM-dd");
            else if (this.EbDbType == EbDbTypes.DateTime)
                return Convert.ToDateTime(value).Date.ToString("yyyy-MM-dd HH:mm:ss");

            return value.ToString();
        }

        public override object ActualToSQLite(object value)
        {
            try
            {
                if (this.EbDbType == EbDbTypes.Date)
                    return DateTime.ParseExact(value?.ToString(), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                else if (this.EbDbType == EbDbTypes.DateTime)
                    return DateTime.ParseExact(value?.ToString(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {

            }
            return value;
        }

        public override View Draw(FormMode Mode, NetworkMode Network)
        {
            View control;

            TapGestureRecognizer gesture = new TapGestureRecognizer();
            gesture.Tapped += Icon_Tapped;

            if (EbDateType == EbDateType.Time)
            {
                timePicker = new EbXTimePicker
                {
                    IsEnabled = !this.ReadOnly,
                    BorderColor = Color.Transparent
                };
                timePicker.PropertyChanged += PropertyChanged;
                control = timePicker;
            }
            else
            {
                datePicker = new EbXDatePicker
                {
                    IsEnabled = !this.ReadOnly,
                    Date = DateTime.Now,
                    BorderColor = Color.Transparent
                };
                if (this.BlockBackDatedEntry) datePicker.MinimumDate = DateTime.Now;
                if (this.BlockFutureDatedEntry) datePicker.MaximumDate = DateTime.Now;

                datePicker.PropertyChanged += PropertyChanged;
                control = datePicker;
            }

            Label icon = new Label
            {
                Style = (Style)HelperFunctions.GetResourceValue("DatePickerIcon"),
                Text = EbDateType == EbDateType.Time ? "\uf017" : "\uf073",
                GestureRecognizers = { gesture }
            };

            this.XControl = new InputGroup(control, icon) { XBackgroundColor = XBackground, HasShadow = false };

            return base.Draw(Mode, Network);
        }

        private void PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is EbXTimePicker)
            {
                if (e.PropertyName == EbXTimePicker.TimeProperty.PropertyName)
                    this.ValueChanged();
            }
            else if (sender is EbXDatePicker)
            {
                if (e.PropertyName == EbXDatePicker.DateProperty.PropertyName)
                    this.ValueChanged();
            }
        }

        private void Icon_Tapped(object sender, EventArgs e)
        {
            if (this.ReadOnly) return;

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
                EbLog.Warning(ex.StackTrace);
            }
        }

        public override void Reset()
        {
            if (this.EbDateType == EbDateType.Time)
                timePicker.ClearValue(EbXTimePicker.TimeProperty);
            else
                datePicker.ClearValue(EbXDatePicker.DateProperty);
        }

        public override bool Validate()
        {
            return base.Validate();
        }

        public override void SetAsReadOnly(bool disable)
        {
            base.SetAsReadOnly(disable);

            Color bg = disable ? EbMobileControl.ReadOnlyBackground : Color.Transparent;

            (this.XControl as InputGroup).XBackgroundColor = bg;
        }

        public override void SetValidation(bool status, string message)
        {
            base.SetValidation(status, message);

            Color border = status ? EbMobileControl.DefaultBorder : EbMobileControl.ValidationError;

            (this.XControl as InputGroup).BorderColor = border;
        }
    }
}
