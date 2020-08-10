using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Structures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Models
{
    public interface IEbApiStatusCode
    {
        HttpStatusCode StatusCode { set; get; }
    }

    public class EbMobileSolutionData : IEbApiStatusCode
    {
        public List<AppData> Applications { set; get; }

        public List<EbLocation> Locations { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public EbDataSet GetOfflineData()
        {
            EbDataSet ds = new EbDataSet();
            Applications?.ForEach(item =>
            {
                if (item.OfflineData != null)
                    ds.Tables.AddRange(item.OfflineData.Tables);
            });
            return ds;
        }

        public void ClearOfflineData()
        {
            Applications?.ForEach(item =>
            {
                item.OfflineData = null;
            });
        }
    }

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

        public EbMobileSettings AppSettings { set; get; }

        public List<MobilePagesWraper> MobilePages { set; get; }

        public List<WebObjectsWraper> WebObjects { set; get; }

        public EbDataSet OfflineData { set; get; }

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

    public class EbApiMeta
    {
        public string RefId { set; get; }

        public string Name { set; get; }

        public string DisplayName { set; get; }

        public string Version { set; get; }
    }

    public class EbMobileSettings
    {
        public EbApiMeta MenuApi { set; get; }

        public bool HasMenuPreloadApi
        {
            get { return (MenuApi != null); }
        }
    }

    public class ApiMessage
    {
        public string Status { get; set; }

        public string Description { get; set; }

        public string ExecutedOn { set; get; }

        public string ExecutionTime { set; get; }
    }

    public class ApiResponse
    {
        public string Name { set; get; }

        public string Version { set; get; }

        public ApiMessage Message { get; set; }
    }
}
