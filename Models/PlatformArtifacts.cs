using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;

namespace ExpressBase.Mobile.Models
{
    public interface IEbApiStatusCode
    {
        HttpStatusCode StatusCode { set; get; }
    }

    public class EbMobileSolutionData
    {
        public List<AppData> Applications { set; get; }

        public List<EbLocation> Locations { get; set; }

        public List<MobilePagesWraper> ProfilePages { set; get; }

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

        [JsonIgnore]
        public string AppNotation => AppName?.ToCharNotation(2).ToUpper();

        public bool HasMenuApi()
        {
            if (AppSettings != null)
            {
                return AppSettings.HasMenuPreloadApi && !App.Settings.CurrentUser.IsAdmin;
            }
            return false;
        }

        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            OfflineData = null;
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

            Data = new List<Param>();
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

        public int StageId { set; get; }

        public string WebFormRefId { set; get; }

        public int WebFormDataId { set; get; }

        public int ApprovalLinesId { set; get; }

        public string Description { set; get; }

        public MyActionTypes ActionType { set; get; }

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

        public string Notation => Description?.ToCharNotation(2).ToUpper();

        public bool IsTagVisible => ActionType == MyActionTypes.Approval;
    }

    public class MyActionsResponse
    {
        public List<EbMyAction> Actions { get; set; }

        public MyActionsResponse()
        {
            Actions = new List<EbMyAction>();
        }
    }

    public class ParticularActionResponse
    {
        public EbMyAction Action { set; get; }

        public EbStageInfo ActionInfo { set; get; }
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

        public Eb_Solution SolutionObj { get; set; }

        public string SignUpPage { set; get; }

        public string Message { set; get; }
    }

    public class EbSignUpUserInfo
    {
        public string AuthId { get; set; }

        public string UserName { get; set; }

        public int UserType { get; set; }

        public bool VerificationRequired { get; set; }

        public string VerifyEmail { get; set; }

        public string VerifyPhone { get; set; }

        public string Message { get; set; }

        public string Token { get; set; }
    }

    public class MobileVisDataRespnse
    {
        public string Message { set; get; }

        public EbDataSet Data { set; get; }

        public bool HasData()
        {
            return Data != null && Data.Tables.HasLength(2);
        }

        public bool TryGetFirstRow(int tableIndex, out EbDataRow row)
        {
            EbDataTable dt = Data.Tables[tableIndex];
            bool status = false;
            row = null;

            if (dt.Rows.Any())
            {
                row = dt.Rows[0];
                status = true;
            }
            return status;
        }
    }

    public class ApiFileData
    {
        public string FileName { set; get; }

        public string FileType { set; get; }

        public int FileRefId { set; get; }
    }

    public class MobileFormDataResponse
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

        public bool HasContent => (Bytea != null && Bytea.Length > 0);
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
