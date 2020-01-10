using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Structures;
using System;
using System.Collections.Generic;

namespace ExpressBase.Mobile
{
    public class DbTypedValue
    {
        public EbDbTypes Type { set; get; }

        public object Value { set; get; }
    }

    public class EbMobileContainer : EbMobilePageBase
    {

    }

    public class EbMobileVisualization : EbMobileContainer
    {
        public string DataSourceRefId { set; get; }

        public string SourceFormRefId { set; get; }

        public EbScript OfflineQuery { set; get; }

        public EbMobileTableLayout DataLayout { set; get; }

        public string LinkRefId { get; set; }
    }
}
