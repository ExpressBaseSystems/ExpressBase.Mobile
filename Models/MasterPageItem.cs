using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressBase.Mobile.Views
{
    public class MasterPageItem
    {
        public string Title { get; set; }

        public string IconSource { get; set; }

        public Type TargetType { get; set; }

        public string LinkType { set; get; }
    }
}
