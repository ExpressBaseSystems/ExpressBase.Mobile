using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Structures;
using Plugin.Media;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls
{
    public interface ICustomElement
    {
        EbMobileControl EbControl { set; get; }

        string Name { set; get; }

        EbDbTypes DbType { set; get; }

        object GetValue();

        bool SetValue(object value);

        void SetAsReadOnly(bool Enable);
    }

    public class TextBox : Entry, ICustomElement
    {
        public EbMobileControl EbControl { set; get; }

        public TextBox() { }

        public TextBox(EbMobileTextBox EbTextBox)
        {
            this.EbControl = EbTextBox;
            this.Name = EbTextBox.Name;
            this.DbType = EbTextBox.EbDbType;
            this.IsVisible = !(this.EbControl.Hidden);
        }

        public EbDbTypes DbType { set; get; }

        public string Name { set; get; }

        public object GetValue()
        {
            return this.Text;
        }

        public bool SetValue(object value)
        {
            if (value == null)
                return false;
            this.Text = value.ToString();
            return true;
        }

        public void SetAsReadOnly(bool Enable)
        {
            if (Enable == true)
                this.IsEnabled = false;
            else
                this.IsEnabled = true;
        }
    }

    public class NumericTextBox : Entry, ICustomElement
    {
        public EbMobileControl EbControl { set; get; }

        public NumericTextBox() { }

        public NumericTextBox(EbMobileNumericBox EbTextBox)
        {
            this.EbControl = EbTextBox;
            this.Name = EbTextBox.Name;
            this.DbType = EbTextBox.EbDbType;
            Keyboard = Keyboard.Numeric;
            this.IsVisible = !(this.EbControl.Hidden);
        }

        public EbDbTypes DbType { set; get; }

        public string Name { set; get; }

        public object GetValue()
        {
            return this.Text;
        }

        public bool SetValue(object value)
        {
            if (value == null)
                return false;
            this.Text = value.ToString();
            return true;
        }

        public void SetAsReadOnly(bool Enable)
        {
            if (Enable == true)
                this.IsEnabled = false;
            else
                this.IsEnabled = true;
        }
    }

    public class XButton : Button
    {
        public XButton() { }
    }

    public class CustomToolBar
    {
        public IList<ToolbarItem> ToolBar { set; get; }

        public CustomToolBar()
        {
            this.ToolBar = new List<ToolbarItem>();
            this.ToolBar.Add(new ToolbarItem
            {
                Text = "item1"
            });
        }
    }

    public class CustomDatePicker : DatePicker, ICustomElement
    {
        public EbMobileControl EbControl { set; get; }

        public EbDbTypes DbType { set; get; }

        public string Name { set; get; }

        public CustomDatePicker() { }

        public CustomDatePicker(EbMobileDateTime EbDate)
        {
            EbControl = EbDate;
            Date = DateTime.Now;
            Name = EbDate.Name;
            DbType = EbDate.EbDbType;
            Format = "yyyy-MM-dd";
            this.IsVisible = !(this.EbControl.Hidden);
        }

        public object GetValue()
        {
            return this.Date.ToString("yyyy-MM-dd");
        }

        public bool SetValue(object value)
        {
            if (value == null)
                return false;
            this.Date = Convert.ToDateTime(value);
            return true;
        }

        public void SetAsReadOnly(bool Enable)
        {
            if (Enable == true)
                this.IsEnabled = false;
            else
                this.IsEnabled = true;
        }
    }

    public class CustomCheckBox : CheckBox, ICustomElement
    {
        public EbMobileControl EbControl { set; get; }

        public EbDbTypes DbType { set; get; }

        public string Name { set; get; }

        public CustomCheckBox() { }

        public CustomCheckBox(EbMobileBoolean EbBool)
        {
            EbControl = EbBool;
            Name = EbBool.Name;
            this.IsVisible = !(this.EbControl.Hidden);
        }

        public object GetValue()
        {
            return this.IsChecked;
        }

        public bool SetValue(object value)
        {
            if (value == null)
                return false;
            int val = Convert.ToInt32(value);
            this.IsChecked = (val == 0) ? false : true;
            return true;
        }

        public void SetAsReadOnly(bool Enable)
        {
            if (Enable == true)
                this.IsEnabled = false;
            else
                this.IsEnabled = true;
        }
    }
}
