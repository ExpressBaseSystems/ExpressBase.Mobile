using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
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
    }

    public class MobilePagesWraper
    {
        public string DisplayName { set; get; }

        public string Name { set; get; }

        public string Version { set; get; }

        public string Json { set; get; }

        public string RefId { set; get; }

        public string ObjectIcon
        {
            get
            {
                var p = ToPage();
                if (p.Container is EbMobileForm)
                    return "form.png";
                else if (p.Container is EbMobileVisualization)
                    return "list.png";
                else
                    return "list.png";
            }
        }

        public string ContainerType
        {
            get
            {
                var p = ToPage();
                if (p.Container is EbMobileForm)
                    return "Forms";
                else if (p.Container is EbMobileVisualization)
                    return "List";
                else
                    return "Miscellaneous";
            }
        }

        public bool IsHidden
        {
            get { return ToPage().HideFromMenu; }
        }

        public EbMobilePage ToPage()
        {
            string regexed = EbSerializers.JsonToNETSTD(this.Json);
            return EbSerializers.Json_Deserialize<EbMobilePage>(regexed);
        }
    }

    public class ValidateSidResponse
    {
        public bool IsValid { set; get; }

        public byte[] Logo { set; get; }
    }

    public class VisualizationLiveData
    {
        public string Message { set; get; }

        public EbDataSet Data { set; get; }
    }

    public class ApiFileData
    {
        public string FileName { set; get; }

        public string FileType { set; get; }

        public int FileRefId { set; get; }
    }
}
