using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Structures;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

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

        public string Description { set; get; } = "No description";
    }

    public class MobilePageCollection
    {
        public List<MobilePagesWraper> Pages { set; get; }

        public EbDataSet Data { set; get; }

        public List<string> TableNames { set; get; }

        public MobilePageCollection()
        {
            this.Pages = new List<MobilePagesWraper>();
        }

        public List<EbMobilePage> GetForms()
        {
            List<EbMobilePage> form_collection = new List<EbMobilePage>();
            try
            {
                foreach (MobilePagesWraper pages in this.Pages)
                {
                    EbMobilePage mpage = pages.JsonToPage();
                    if (mpage != null && mpage.Container is EbMobileForm)
                    {
                        form_collection.Add(mpage);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return form_collection;
        }
    }

    public class PageTypeGroup : List<MobilePagesWraper>
    { 
        public string GroupHeader { get; set; }

        public IList<MobilePagesWraper> Pages => this;
    }

    public class MobilePagesWraper
    {
        public string ObjectIcon
        {
            get
            {
                if (this.JsonToPage().Container is EbMobileForm)
                {
                    return "form.png";
                }
                else if (this.JsonToPage().Container is EbMobileVisualization)
                {
                    return "list.png";
                }
                else
                {
                    return "list.png";
                }
            }
        }

        public string DisplayName { set; get; }

        public string Name { set; get; }

        public string Version { set; get; }

        public string Json { set; get; }

        public EbMobilePage JsonToPage()
        {
            if (string.IsNullOrEmpty(Json))
                return null;
            string regexed = EbSerializers.JsonToNETSTD(this.Json);
            return EbSerializers.Json_Deserialize<EbMobilePage>(regexed);
        }
    }

    public class MobileFormData
    {
        public string MasterTable { set; get; }

        public int LocationId { set; get; }

        public List<MobileTable> Tables { set; get; }

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
    }
}
