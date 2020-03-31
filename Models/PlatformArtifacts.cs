using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Models
{
    public class AppCollection
    {
        public List<AppData> Applications { get; set; }
    }

    public class CustomColor
    {
        public string BackGround { set; get; }

        public string TextColor { set; get; }
    }

    public class ColorSet
    {
        public static IList<CustomColor> Colors
        {
            get
            {
                return new List<CustomColor>
                {
                    new CustomColor{ BackGround = "eaf7f9", TextColor = "3bb1cd" },
                    new CustomColor{ BackGround = "fef0f2", TextColor = "ef7c93" },
                    new CustomColor{ BackGround = "eef2fe", TextColor = "5a8ff4" },
                    new CustomColor{ BackGround = "fcf4ec", TextColor = "e0a14e" },
                    new CustomColor{ BackGround = "fdf0fb", TextColor = "e364d0" },
                    new CustomColor{ BackGround = "f2f4f6", TextColor = "8da2b2" }
                };
            }
        }
    }

    public class AppData
    {
        public int AppId { set; get; }

        public string AppName { set; get; }

        public string AppIcon { set; get; }

        public string Description { set; get; } = "No description";

        public string AppNotation
        {
            get
            {
                string notation = string.Empty;
                foreach (var item in AppName.Split(' ').Take(2))
                    notation += item[0];
                return notation.ToUpper();
            }
        }

        public Color BackgroundColor { set; get; }

        public Color TextColor { set; get; }
    }

    public class EbStageInfo
    {
        public string StageUniqueId { set; get; }

        public string StageName { set; get; }

        public List<EbStageActions> StageActions { set; get; }

        public List<Param> Data { set; get; }

        public EbStageInfo()
        {
            StageActions = new List<EbStageActions>();
        }
    }

    public class EbStageActions
    {
        public string ActionName { set; get; }

        public string ActionUniqueId { set; get; }
    }

    public class EbMyAction
    {
        public int Id { set; get; }

        public DateTime StartDate { set; get; }

        public DateTime EndDate { set; get; }

        public int StageId { set; get; }

        public string WebFormRefId { set; get; }

        public int WebFormDataId { set; get; }

        public int ApprovalLinesId { set; get; }

        public string Description { set; get; }

        public EbStageInfo StageInfo { set; get; }

        public string EndsOn
        {
            get
            {
                if (EndDate != null)
                    return "Ends on " + EndDate.ToString("MMMM dd, yyyy");
                else
                    return "End date not specified";
            }
        }

        public string DescriptionL1
        {
            get
            {
                return (Description == null) ? "?" : Description[0].ToString().ToUpper();
            }
        }
    }

    public class MyActionsResponse
    {
        public List<EbMyAction> Actions { get; set; }

        public MyActionsResponse()
        {
            Actions = new List<EbMyAction>();
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
                        icon = "f03a";//"f022";
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

        public Color IconColor
        {
            get
            {
                if (_page == null) ToPage();
                if (string.IsNullOrEmpty(_page.IconColor))
                    return Color.FromHex("0046bb");
                else
                    return Color.FromHex(_page.IconColor);
            }
        }

        public Color IconBackground
        {
            get
            {
                if (_page == null) ToPage();
                if (string.IsNullOrEmpty(_page.IconBackground))
                    return Color.White;
                else
                    return Color.FromHex(_page.IconBackground);
            }
        }

        public Color TextColor
        {
            get
            {
                if (string.IsNullOrEmpty(_page.IconColor))
                    return Color.FromHex("333333");
                else
                    return Color.FromHex(_page.IconColor);
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
