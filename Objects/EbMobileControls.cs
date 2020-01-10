using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Structures;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbMobileControl : EbMobilePageBase
    {
        public virtual string Label { set; get; }

        public virtual EbDbTypes EbDbType { get { return EbDbTypes.String; } set { } }

        public virtual bool Hidden { set; get; }

        public virtual bool Unique { get; set; }

        public virtual bool ReadOnly { get; set; }

        public virtual bool DoNotPersist { get; set; }

        public string SQLiteType
        {
            get
            {
                if (this.EbDbType == EbDbTypes.String)
                    return "TEXT";
                else if (this.EbDbType == EbDbTypes.Int16 || this.EbDbType == EbDbTypes.Int32)
                    return "INT";
                else if (this.EbDbType == EbDbTypes.Decimal || this.EbDbType == EbDbTypes.Double)
                    return "REAL";
                else if (this.EbDbType == EbDbTypes.Date || this.EbDbType == EbDbTypes.DateTime)
                    return "DATETIME";
                else if (this.EbDbType == EbDbTypes.Boolean)
                    return "INT";
                else
                    return "TEXT";
            }
        }

        //mobile prop
        public virtual object SQLiteToActual(object value) { return value; }

        //mobile prop
        public View XControl { set; get; }

        public virtual void InitXControl() { }

        public virtual StackLayout XView
        {
            get
            {
                return new StackLayout
                {
                    Padding = new Thickness(15, 10, 15, 10),
                    IsVisible = !(this.Hidden),
                    Children =
                    {
                        new Label { Text = this.Label },
                        XControl
                    }
                };
            }
        }

        public virtual object GetValue() { return null; }

        public virtual bool SetValue(object value) { return false; }

        public virtual void SetAsReadOnly(bool Enable)
        {
            if (Enable == true)
                this.XControl.IsEnabled = false;
            else
                this.XControl.IsEnabled = true;
        }

        public virtual MobileTableColumn GetMobileTableColumn()
        {
            object value = this.GetValue();
            if (value == null)
                return null;

            return new MobileTableColumn
            {
                Name = this.Name,
                Type = this.EbDbType,
                Value = value
            };
        }
    }

    public class EbMobileTextBox : EbMobileControl
    {
        public override EbDbTypes EbDbType { get { return EbDbTypes.String; } set { } }

        public int MaxLength { get; set; }

        public TextTransform TextTransform { get; set; }

        public TextMode TextMode { get; set; }

        public bool AutoCompleteOff { get; set; }

        public bool AutoSuggestion { get; set; }

        public override void InitXControl()
        {
            XControl = new TextBox();
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

    public class EbMobileNumericBox : EbMobileControl
    {
        public override EbDbTypes EbDbType { get { return EbDbTypes.Decimal; } set { } }

        public int MaxLength { get; set; }

        public int DecimalPlaces { get; set; }

        public int MaxLimit { get; set; }

        public int MinLimit { get; set; }

        public bool IsCurrency { get; set; }

        public override void InitXControl()
        {
            XControl = new TextBox();
            (XControl as TextBox).Keyboard = Keyboard.Numeric;
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

    public class EbMobileDateTime : EbMobileControl
    {
        public EbDateType EbDateType { get; set; }

        public override EbDbTypes EbDbType { get { return (EbDbTypes)this.EbDateType; } set { } }

        public bool IsNullable { get; set; }

        public TimeShowFormat ShowTimeAs_ { get; set; }

        public DateShowFormat ShowDateAs_ { get; set; }

        public override object SQLiteToActual(object value)
        {
            if (this.EbDbType == EbDbTypes.Date)
            {
                return Convert.ToDateTime(value).Date.ToString("yyyy-MM-dd");
            }
            return value.ToString();
        }

        public override void InitXControl()
        {
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

        public override void InitXControl()
        {
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

    public class EbMobileTableLayout : EbMobileControl
    {
        public EbMobileTableLayout()
        {
            this.CellCollection = new List<EbMobileTableCell>();
        }

        public int RowCount { set; get; }

        public int ColumCount { set; get; }

        public List<EbMobileTableCell> CellCollection { set; get; }

        public override bool Hidden { set; get; }

        public override bool Unique { get; set; }
    }

    public class EbMobileTableCell : EbMobilePageBase
    {
        public EbMobileTableCell()
        {
            this.ControlCollection = new List<EbMobileControl>();
        }

        public int RowIndex { set; get; }

        public int ColIndex { set; get; }

        public int Width { set; get; }

        public List<EbMobileControl> ControlCollection { set; get; }
    }

    public class EbMobileDataColumn : EbMobileControl
    {
        public string TextFormat { get; set; }

        public int TableIndex { get; set; }

        public int ColumnIndex { get; set; }

        public string ColumnName { get; set; }

        public EbDbTypes Type { get; set; }

        public EbFont Font { get; set; }
    }
}
