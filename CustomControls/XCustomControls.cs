using ExpressBase.Mobile.Structures;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls
{
    public class XCustomControl
    {
        public XCustomControl() { }

        public XCustomControl(EbMobileControl Ctrl) { }

        public EbMobileControl EbControl { set; get; }

        public View XControl { set; get; }

        public StackLayout XView
        {
            get
            {
                return new StackLayout
                {
                    Padding = new Thickness(15,10,15,10),
                    IsVisible = !(this.EbControl.Hidden),
                    Children =
                    {
                        new Label { Text = this.EbControl.Label },
                        XControl
                    }
                };
            }
        }

        public string Name { set; get; }

        public EbDbTypes DbType { set; get; }

        public virtual object GetValue() { return null; }

        public virtual bool SetValue(object value) { return false; }

        public virtual void SetAsReadOnly(bool Enable)
        {
            if (Enable == true)
                this.XControl.IsEnabled = false;
            else
                this.XControl.IsEnabled = true;
        }
    }

    public class XEntry : XCustomControl
    {
        public XEntry() { }

        public XEntry(EbMobileControl Ctrl)
        {
            this.Name = Ctrl.Name;
            this.DbType = Ctrl.EbDbType;
            this.EbControl = Ctrl;

            this.XControl = new TextBox();
        }

        public override object GetValue()
        {
            return (this.XControl as TextBox).Text;
        }

        public override bool SetValue(object value)
        {
            if (value == null)
                return false;
            (this.XControl as TextBox).Text = value.ToString();
            return true;
        }
    }

    public class XNumericEntry : XCustomControl
    {
        public XNumericEntry() { }

        public XNumericEntry(EbMobileControl Ctrl)
        {
            this.Name = Ctrl.Name;
            this.DbType = Ctrl.EbDbType;
            this.EbControl = Ctrl;

            this.XControl = new TextBox();
            (this.XControl as TextBox).Keyboard = Keyboard.Numeric;
        }

        public override object GetValue()
        {
            return (this.XControl as TextBox).Text;
        }

        public override bool SetValue(object value)
        {
            if (value == null)
                return false;
            (this.XControl as TextBox).Text = value.ToString();
            return true;
        }
    }

    public class XDatePicker : XCustomControl
    {
        public XDatePicker() { }

        public XDatePicker(EbMobileControl Ctrl)
        {
            this.Name = Ctrl.Name;
            this.DbType = Ctrl.EbDbType;
            this.EbControl = Ctrl;

            this.XControl = new CustomDatePicker();

            (this.XControl as CustomDatePicker).Date = DateTime.Now;
            (this.XControl as CustomDatePicker).Format = "yyyy-MM-dd";
        }

        public override object GetValue()
        {
            return (this.XControl as CustomDatePicker).Date.ToString("yyyy-MM-dd");
        }

        public override bool SetValue(object value)
        {
            if (value == null)
                return false;
            (this.XControl as CustomDatePicker).Date = Convert.ToDateTime(value);
            return true;
        }
    }

    public class XCheckBox : XCustomControl
    {
        public XCheckBox() { }

        public XCheckBox(EbMobileControl Ctrl)
        {
            this.Name = Ctrl.Name;
            this.DbType = Ctrl.EbDbType;
            this.EbControl = Ctrl;

            this.XControl = new CheckBox();
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
            (this.XControl as CheckBox).IsChecked = (val == 0) ? false : true;
            return true;
        }
    }
}
