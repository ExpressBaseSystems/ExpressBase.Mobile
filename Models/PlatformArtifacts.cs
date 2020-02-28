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

    public class EbMyActions
    {
        public int Id { set; get; }

        public DateTime StartDate { set; get; }

        public DateTime EndDate { set; get; }

        public int StageId { set; get; }

        public string WebFormRefId { set; get; }

        public int WebFormDataId { set; get; }

        public int ApprovalLinesId { set; get; }

        public string Description { set; get; }
    }

    public class MyActionsResponse
    {
        public List<EbMyActions> Actions { get; set; }

        public MyActionsResponse()
        {
            Actions = new List<EbMyActions>();
        }
    }

    public class MobilePageCollection
    {
        public List<MobilePagesWraper> Pages { set; get; }

        public List<WebObjectsWraper> WebObjects { set; get; }

        public EbDataSet Data { set; get; }

        public List<string> TableNames { set; get; }

        public MobilePageCollection()
        {
            this.Pages = new List<MobilePagesWraper>();
            this.WebObjects = new List<WebObjectsWraper>();
        }
    }

    public class MobilePagesWraper
    {
        private EbMobilePage _page;

        public string DisplayName { set; get; }

        public string Name { set; get; }

        public string Version { set; get; }

        public string Json { set; get; }

        public string RefId { set; get; }

        public string ObjectIcon
        {
            get
            {
                if (_page == null) ToPage();

                string icon = _page.Icon;

                if (string.IsNullOrEmpty(icon))
                {
                    if (_page.Container is EbMobileForm)
                        icon = "f298";
                    else if (_page.Container is EbMobileVisualization)
                        icon = "f022";
                    else if (_page.Container is EbMobileDashBoard)
                        icon = "f0e4";
                    else if (_page.Container is EbMobilePdf)
                        icon = "f1c1";
                    else
                        icon = "f0e4";
                }
                return icon;
            }
        }

        public string ContainerType
        {
            get
            {
                if (_page == null) ToPage();

                if (_page.Container is EbMobileForm)
                    return "Forms";
                else if (_page.Container is EbMobileVisualization)
                    return "List";
                else if (_page.Container is EbMobileDashBoard)
                    return "DashBoards";
                else if (_page.Container is EbMobilePdf)
                    return "Pdf";
                else
                    return "Miscellaneous";
            }
        }

        public bool IsHidden
        {
            get
            {
                if (_page == null) ToPage();
                return _page.HideFromMenu;
            }
        }

        public EbMobilePage ToPage()
        {
            string regexed = EbSerializers.JsonToNETSTD(this.Json);
            _page = EbSerializers.Json_Deserialize<EbMobilePage>(regexed);
            return _page;
        }
    }

    public class WebObjectsWraper
    {
        public string DisplayName { set; get; }

        public string Name { set; get; }

        public string Version { set; get; }

        public string RefId { set; get; }

        public string Json { set; get; }

        public int ObjectType { set; get; }
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
