using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
                try
                {
                    string notation = string.Empty;
                    foreach (var item in AppName.Split(' ').Take(2))
                    {
                        if (string.IsNullOrEmpty(item)) continue;
                        notation += item[0];
                    }
                    return notation.ToUpper();
                }
                catch (Exception ex)
                {
                    EbLog.Write(ex.Message);
                    return "??";
                }
            }
        }
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

        public string DaysAgo
        {
            get
            {
                if (StartDate != null)
                    return StartDate.SubtractByNow();
                else
                    return "... ago";
            }
        }

        public string Notation
        {
            get
            {
                try
                {
                    string notation = string.Empty;
                    foreach (var item in Description.Split(' ').Take(2))
                    {
                        if (string.IsNullOrEmpty(item)) continue;
                        notation += item[0];
                    }
                    return notation.ToUpper();
                }
                catch (Exception ex)
                {
                    EbLog.Write(ex.Message);
                    return "??";
                }
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
                    icon = this.GetDefaultIcon();
                return icon;
            }
        }

        public string ContainerType
        {
            get
            {
                if (_page == null) ToPage();
                return ContainerLabels.GetLabel(_page.Container);
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

        public EbMobilePage ToPage()
        {
            string regexed = EbSerializers.JsonToNETSTD(this.Json);
            _page = EbSerializers.Json_Deserialize<EbMobilePage>(regexed);
            return _page;
        }

        public string GetDefaultIcon()
        {
            if (_page == null) ToPage();

            if (_page.Container is EbMobileForm)
                return "f298";
            else if (_page.Container is EbMobileVisualization)
                return "f03a";//"f022";
            else if (_page.Container is EbMobileDashBoard)
                return "f0e4";
            else if (_page.Container is EbMobilePdf)
                return "f1c1";
            else
                return "f0e4";
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

    public class MobileFormLiveData
    {
        public string Message { set; get; }

        public WebformData Data { set; get; }
    }

    public class SolutionQrMeta
    {
        public string Sid { set; get; }

    }

    public class ApiFileResponse
    {
        public string ContentType { set; get; }

        public byte[] Bytea { set; get; }

        public HttpStatusCode StatusCode { set; get; }

        public bool HasContent
        {
            get
            {
                return (Bytea != null && Bytea.Length > 0);
            }
        }
    }
}
