using ExpressBase.Mobile.Structures;
using System;
using System.Collections.Generic;

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
                if (Type == EbDbTypes.DateTime)
                    return Convert.ToDateTime(_value).ToString("yyyy-MM-dd");
                else if (Type == EbDbTypes.Date)
                    return Convert.ToDateTime(_value).ToString("yyyy-MM-dd");
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

    public class EbMobileDashBoard : EbMobileContainer
    {
        public List<EbMobileDashBoardControls> ChildControls { get; set; }

        public EbMobileDashBoard()
        {
            ChildControls = new List<EbMobileDashBoardControls>();
        }
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
