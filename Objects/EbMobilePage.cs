using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Structures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

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

    public abstract class EbMobilePageBase : EbObject
    {

    }

    public class EbMobilePage : EbMobilePageBase
    {
        public override string RefId { get; set; }

        public override string DisplayName { get; set; }

        public override string Description { get; set; }

        public override string VersionNumber { get; set; }

        public override string Status { get; set; }

        public EbMobileContainer Container { set; get; }

        public bool HideFromMenu { set; get; }
    } 

    public class EbScript
    {
        public EbScript()
        {
            Code = Code ?? string.Empty;
        }

        public string Code { get; set; }

        public ScriptingLanguage Lang { get; set; }
    }

    public class EbFont
    {
        public string FontName { get; set; }

        public int Size { get; set; }

        public FontStyle Style { get; set; }

        public string color { get; set; }

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
