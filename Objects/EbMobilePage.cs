using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Structures;
using Newtonsoft.Json;
using System;

namespace ExpressBase.Mobile
{
    public class EbObject
    {
        public virtual string Name { get; set; }

        public EbObject() { }

        public virtual string RefId { get; set; }

        public virtual string DisplayName { get; set; }

        public virtual string Description { get; set; }

        public virtual string VersionNumber { get; set; }

        public virtual string Status { get; set; }
    }

    public abstract class EbMobilePageBase : EbObject { }

    public class EbMobilePage : EbMobilePageBase
    {
        public EbMobileContainer Container { set; get; }

        public string Category { get; set; }

        public NetworkMode NetworkMode { get; set; }

        public bool HideFromMenu { set; get; }

        public string Icon { set; get; }

        public string IconColor { get; set; }

        public string IconBackground { get; set; }
    }

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
            return string.IsNullOrEmpty(Code);
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
}
