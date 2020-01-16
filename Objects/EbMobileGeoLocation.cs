using ExpressBase.Mobile.Structures;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressBase.Mobile
{
    public class EbMobileGeoLocation : EbMobileControl
    {
        public override EbDbTypes EbDbType { get { return EbDbTypes.String; } set { } }

        public bool HideSearchBox { set; get; }

        public override void InitXControl()
        {
            
        }

        public override object GetValue()
        {
            return null;
        }

        public override bool SetValue(object value)
        {
            if (value == null)
                return false;

            return true;
        }
    }
}
