using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Structures;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressBase.Mobile.Models
{
    public class AppCollection
    {
        public List<AppData> Applications { get; set; }
    }

    public class AppData
    {
        public int AppId { set; get; }

        public string AppName { set; get; }

        public string AppIcon { set; get; }
    }

    public class MobilePageCollection
    {
        public List<MobilePagesWraper> Pages { set; get; }

        public MobilePageCollection()
        {
            this.Pages = new List<MobilePagesWraper>();
        }
    }

    public class MobilePagesWraper
    {
        public string DisplayName { set; get; }

        public string Name { set; get; }

        public string Version { set; get; }

        public string Json { set; get; }
    }

    public class MobileFormData
    {
        public string MasterTable { set; get; }

        public int LocationId { set; get; }

        public List<MobileTable> Tables {set;get;}
    }

    public class MobileTable : List<MobileTableRow>
    {
        public string TableName { set; get; }
    }

    public class MobileTableRow
    {
        public int RowId { set; get; }

        public bool IsUpdate { set; get; }

        public List<MobileTableColumn> Columns { set; get; }
    }

    public class MobileTableColumn
    {
        public string Name { get; set; }

        public object Value { get; set; }

        public EbDbTypes Type { set; get; }

        public EbMobileControl Control { set; get; }
    }
}
