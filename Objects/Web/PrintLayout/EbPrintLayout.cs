using ExpressBase.Mobile.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressBase.Mobile
{
    public enum ThermalPrintTemplates
    {
        None = 0,
        SalesInvoiceV1 = 1
    }

    public class EbPrintLayout : EbObject
    {
        public EbScript OfflineQuery { get; set; }

        public ThermalPrintTemplates ThermalPrintTemplate { set; get; }
    }
}
