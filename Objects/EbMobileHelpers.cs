using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Structures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbScript
    {
        public string Code { get; set; }

        public ScriptingLanguage Lang { get; set; }

        public string GetCode()
        {
            return HelperFunctions.B64ToString(this.Code);
        }

        public bool IsEmpty()
        {
            return string.IsNullOrWhiteSpace(Code);
        }
    }

    public class EbFont
    {
        public string FontName { get; set; }

        public int Size { get; set; }

        public FontStyle Style { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        public bool Caps { get; set; }

        public bool Strikethrough { get; set; }

        public bool Underline { get; set; }
    }

    public class Param
    {
        public Param() { }

        public string Name { get; set; }

        public string Type { get; set; }

        public string Value { get; set; }

        public dynamic ValueTo
        {
            get
            {
                if (Type == ((int)EbDbTypes.Decimal).ToString())
                    return Convert.ToDecimal(Value);
                else if (Type == ((int)EbDbTypes.Int16).ToString())
                    return Convert.ToInt16(Value);
                else if (Type == ((int)EbDbTypes.Int32).ToString())
                    return Convert.ToInt32(Value);
                else if (Type == ((int)EbDbTypes.Int64).ToString())
                    return Convert.ToInt64(Value);
                else if (Type == ((int)EbDbTypes.Date).ToString())
                    return Convert.ToDateTime(Value);
                else if (Type == ((int)EbDbTypes.DateTime).ToString())
                    return Convert.ToDateTime(Value);
                else if (Type == ((int)EbDbTypes.Boolean).ToString())
                    return Convert.ToBoolean(Value);
                else
                    return Value;
            }
        }
    }

    public class EbMobileDataColToControlMap : EbMobilePageBase
    {
        public string ColumnName { set; get; }

        public EbDbTypes Type { get; set; }

        public EbMobileControlMeta FormControl { set; get; }
    }

    public class EbMobileControlMeta : EbMobilePageBase
    {
        public string ControlName { set; get; }

        public string ControlType { set; get; }
    }

    public class EbCTCMapper : EbMobilePageBase
    {
        public string ColumnName { set; get; }

        public string ControlName { set; get; }
    }

    public class EbThickness
    {
        public int Left { set; get; }

        public int Top { set; get; }

        public int Right { set; get; }

        public int Bottom { set; get; }

        public EbThickness() { }

        public EbThickness(int value)
        {
            Left = Top = Right = Bottom = value;
        }

        public EbThickness(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public Thickness ConvertToXValue()
        {
            return new Thickness(Left, Top, Right, Bottom);
        }
    }

    public class EbMobileValidator : EbMobilePageBase
    {
        public bool IsDisabled { get; set; }

        public bool IsWarningOnly { get; set; }

        public EbScript Script { get; set; }

        public string FailureMSG { get; set; }

        public bool IsEmpty()
        {
            return Script == null || string.IsNullOrEmpty(Script.Code);
        }
    }

    public class EbMobileStaticParameter : EbMobilePageBase
    {
        public override string Name { get; set; }

        public string Value { set; get; }

        public bool EnableSearch { set; get; }
    }

    public class EbMobileStaticListItem : EbMobilePageBase
    {
        public override string Name { get; set; }

        public List<EbMobileStaticParameter> Parameters { set; get; }

        public string LinkRefId { get; set; }

        public bool HasLink()
        {
            return !string.IsNullOrEmpty(LinkRefId);
        }

        public EbDataRow GetAsDataRow()
        {
            EbDataRow row = null;
            EbDataTable dt = new EbDataTable();

            if (Parameters != null && Parameters.Any())
            {
                row = dt.NewDataRow();
                dt.Rows.Add(row);

                List<object> dataArray = new List<object>();

                for (int i = 0; i < Parameters.Count; i++)
                {
                    EbMobileStaticParameter pm = Parameters[i];

                    dt.Columns.Add(new EbDataColumn
                    {
                        ColumnName = pm.Name,
                        ColumnIndex = i
                    });

                    dataArray.Add(pm.Value);
                }
                row.AddRange(dataArray.ToArray());
            }
            return row ?? dt.NewDataRow();
        }
    }
}
