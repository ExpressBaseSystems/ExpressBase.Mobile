using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ExpressBase.Mobile
{
    public class EbWebForm : EbForm
    { 

    }

    public class EbControlWrapper
    {
        public string TableName { get; set; }

        public string Path { get; set; }

        public string Root { get; set; }

        public EbControl Control { get; set; }
    }
}
