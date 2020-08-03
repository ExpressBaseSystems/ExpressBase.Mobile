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

        public CustomDatePicker Picker { set; get; }

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

            var bg = this.ReadOnly ? Color.FromHex("eeeeee") : Color.Transparent;

            Picker = new CustomDatePicker
            {
                IsEnabled = !this.ReadOnly,
                Date = DateTime.Now,
                BorderColor = Color.Transparent
            };

            var icon = new Label
            {
                Padding = 10,
                FontSize = 18,
                VerticalOptions = LayoutOptions.Center,
                Text = "\uf073",
                FontFamily = (OnPlatform<string>)HelperFunctions.GetResourceValue("FontAwesome")
            };

            this.XControl = new InputGroup(Picker, icon) { BgColor = bg };
        }

        public override object GetValue()
        {
            if(this.EbDbType == EbDbTypes.Date)
                return Picker.Date.ToString("yyyy-MM-dd");
            else
                return Picker.Date.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public override bool SetValue(object value)
        {
            if (value == null)
                return false;
            Picker.Date = Convert.ToDateTime(value);
            return true;
        }

        public override void Reset()
        {
            Picker.ClearValue(CustomDatePicker.DateProperty);
        }
    }
}
