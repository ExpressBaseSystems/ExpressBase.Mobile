using ExpressBase.Mobile.Structures;
using System;

namespace ExpressBase.Mobile
{
    public class DbTypedValue
    {
        public EbDbTypes Type { set; get; }

        private object _value;

        public object Value
        {
            set { _value = value; }
            get
            {
                if (Type == EbDbTypes.Date)
                    return Convert.ToDateTime(_value).ToString("yyyy-MM-dd");
                else if (Type == EbDbTypes.DateTime)
                    return Convert.ToDateTime(_value).ToString("yyyy-MM-dd HH:mm:ss");
                else
                    return _value;
            }
        }

        public DbTypedValue() { }

        public DbTypedValue(string Name, object Value, EbDbTypes Type)
        {
            if (Name == "eb_created_at_device")
                this.Type = EbDbTypes.DateTime;
            else
            {
                this.Type = Type;
                this.Value = Value;
            }
        }
    }

    public class EbMobileContainer : EbMobilePageBase
    {
        public NetworkMode NetworkType { set; get; }
    }

    public class EbMobilePdf : EbMobileContainer
    {
        public string Template { set; get; }

        public EbScript OfflineQuery { set; get; }

        public EbMobilePdf()
        {
            OfflineQuery = new EbScript();
        }
    }
}
