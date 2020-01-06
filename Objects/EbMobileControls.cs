using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Structures;
using System;
using System.Collections.Generic;
using System.Text;

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

        public virtual Type XControlType { get { return null; } }

        public virtual object SQLiteToActual(object value) { return value; }
    }

    public class EbMobileTextBox : EbMobileControl
    {
        public override EbDbTypes EbDbType { get { return EbDbTypes.String; } set { } }

        public override Type XControlType { get { return typeof(TextBox); } }

        public int MaxLength { get; set; }

        public TextTransform TextTransform { get; set; }

        public TextMode TextMode { get; set; }

        public bool AutoCompleteOff { get; set; }

        public bool AutoSuggestion { get; set; }
    }

    public class EbMobileNumericBox : EbMobileControl
    {
        public override EbDbTypes EbDbType { get { return EbDbTypes.Decimal; } set { } }

        public override Type XControlType { get { return typeof(NumericTextBox); } }

        public int MaxLength { get; set; }

        public int DecimalPlaces { get; set; }

        public int MaxLimit { get; set; }

        public int MinLimit { get; set; }

        public bool IsCurrency { get; set; }
    }

    public class EbMobileDateTime : EbMobileControl
    {
        public EbDateType EbDateType { get; set; }

        public override EbDbTypes EbDbType { get { return (EbDbTypes)this.EbDateType; } set { } }

        public override Type XControlType { get { return typeof(CustomDatePicker); } }

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
    }

    public class EbMobileSimpleSelect : EbMobileControl
    {
        public override EbDbTypes EbDbType
        {
            get
            {
                if (!string.IsNullOrEmpty(DataSourceRefId))
                {
                    if (this.ValueMember != null)
                    {
                        return this.ValueMember.EbDbType;
                    }
                    else
                    {
                        return EbDbTypes.String;
                    }
                }
                else
                {
                    return EbDbTypes.String;
                }
            }
            set { }
        }

        public List<EbMobileSSOption> Options { set; get; }

        public override Type XControlType { get { return typeof(PowerSelect); } }

        public bool IsMultiSelect { get; set; }

        public string DataSourceRefId { get; set; }

        public List<EbMobileDataColumn> Columns { set; get; }

        public EbMobileDataColumn DisplayMember { set; get; }

        public EbMobileDataColumn ValueMember { set; get; }

        public EbScript OfflineQuery { set; get; }

        public List<Param> Parameters { set; get; }
    }

    public class EbMobileSSOption : EbMobilePageBase
    {
        public string EbSid { get; set; }

        public override string DisplayName { get; set; }

        public string Value { get; set; }
    }

    public class EbMobileFileUpload : EbMobileControl
    {
        public bool EnableCameraSelect { set; get; }

        public bool EnableFileSelect { set; get; }

        public bool MultiSelect { set; get; }

        public bool EnableEdit { set; get; }
    }

    public class EbMobileBoolean : EbMobileControl
    {
        public override EbDbTypes EbDbType { get { return EbDbTypes.BooleanOriginal; } set { } }

        public override Type XControlType { get { return typeof(CustomCheckBox); } }

        public override object SQLiteToActual(object value)
        {
            if (Convert.ToInt32(value) == 0)
                return false;
            else
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
