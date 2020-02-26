using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Structures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Models
{
    public class MobileFormData
    {
        public string MasterTable { set; get; }

        public int LocationId { set; get; }

        public List<MobileTable> Tables { set; get; }

        public MobileFormData()
        {
            Tables = new List<MobileTable>();
        }

        public MobileFormData(string masterTableName)
        {
            MasterTable = masterTableName;
            Tables = new List<MobileTable>();
        }

        public WebformData ToWebFormData()
        {

            return null;
        }
    }

    public class MobileTable : List<MobileTableRow>
    {
        public string TableName { set; get; }

        public Dictionary<string, List<FileWrapper>> Files { set; get; }

        public MobileTable()
        {
            Files = new Dictionary<string, List<FileWrapper>>();
        }

        public MobileTable(string tableName)
        {
            TableName = tableName;
            Files = new Dictionary<string, List<FileWrapper>>();
        }
    }

    public class MobileTableRow
    {
        public int RowId { set; get; }

        public bool IsUpdate { get { return (RowId > 0); } }

        public List<MobileTableColumn> Columns { set; get; }

        public MobileTableRow()
        {
            Columns = new List<MobileTableColumn>();
        }

        public MobileTableRow(int rowId)
        {
            RowId = rowId;
            Columns = new List<MobileTableColumn>();
        }

        public void AppendEbColValues()
        {
            this.Columns.Add(new MobileTableColumn { Name = "eb_loc_id", Type = EbDbTypes.Int32, Value = Settings.LocationId });
            this.Columns.Add(new MobileTableColumn { Name = "eb_created_at_device", Type = EbDbTypes.DateTime, Value = DateTime.Now });

            INativeHelper helper = DependencyService.Get<INativeHelper>();

            this.Columns.Add(new MobileTableColumn { Name = "eb_device_id", Type = EbDbTypes.String, Value = helper.DeviceId });
            //<manufacturer>(<model> <platform>:<osversion>)-<appversion>
            string appversion = string.Format("{0}({1} {2}:{3})-{4}", DeviceInfo.Manufacturer, DeviceInfo.Model, DeviceInfo.Platform, DeviceInfo.VersionString, helper.AppVersion);
            this.Columns.Add(new MobileTableColumn { Name = "eb_appversion", Type = EbDbTypes.String, Value = appversion });
        }
    }

    public class MobileTableColumn
    {
        public string Name { get; set; }

        public object Value { get; set; }

        public EbDbTypes Type { set; get; }

        public EbMobileControl Control { set; get; }

        public MobileTableColumn() { }

        public MobileTableColumn(string name, EbDbTypes type, object value)
        {
            Name = name;
            Type = type;
            Value = value;
        }
    }

    public class FileWrapper
    {
        public string Name { set; get; }

        public string FileName { set; get; }

        public byte[] Bytea { set; get; }

        public string ControlName { set; get; }

        public int FileRefId { set; get; }
    }
}
