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

        public MobileFormData()
        {
            Tables = new List<MobileTable>();
        }
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

        public MobileTableRow()
        {
            Columns = new List<MobileTableColumn>();
        }

        public void AppendEbColValues()
        {
            this.Columns.Add(new MobileTableColumn { Name = "eb_created_by", Type = EbDbTypes.Int32, Value = Settings.UserId });
            this.Columns.Add(new MobileTableColumn { Name = "eb_created_at", Type = EbDbTypes.DateTime, Value = DateTime.Now });
            this.Columns.Add(new MobileTableColumn { Name = "eb_lastmodified_by", Type = EbDbTypes.Int32, Value = Settings.UserId });
            this.Columns.Add(new MobileTableColumn { Name = "eb_lastmodified_at", Type = EbDbTypes.DateTime, Value = DateTime.Now });
            this.Columns.Add(new MobileTableColumn { Name = "eb_del", Type = EbDbTypes.Int32, Value = 0 });
            this.Columns.Add(new MobileTableColumn { Name = "eb_void", Type = EbDbTypes.Int32, Value = 0 });
            this.Columns.Add(new MobileTableColumn { Name = "eb_loc_id", Type = EbDbTypes.Int32, Value = Settings.LocationId });
            this.Columns.Add(new MobileTableColumn { Name = "eb_synced", Type = EbDbTypes.Int32, Value = 0 });
            //this.Columns.Add(new MobileTableColumn { Name = "eb_synced_at", Type = EbDbTypes.DateTime, Value = DateTime.Now });
        }
    }

    public class MobileTableColumn
    {
        public string Name { get; set; }

        public object Value { get; set; }

        public EbDbTypes Type { set; get; }

        public EbMobileControl Control { set; get; }
    }
}
