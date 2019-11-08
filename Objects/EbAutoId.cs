using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressBase.Mobile
{
    public class EbAutoId : EbControlUI
    {
        public EbAutoIdPattern Pattern { get; set; }
    }

    public class EbAutoIdPattern
    {
        public string sPattern { get; set; }

        public int SerialLength { get; set; }
    }
}
