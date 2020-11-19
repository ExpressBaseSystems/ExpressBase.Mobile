using ExpressBase.Mobile.Configuration;
using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.Services
{
    public class SettingsServices : BaseService, ISettingsService
    {
        public SolutionInfo CurrentSolution { set; get; }

        public User CurrentUser { set; get; }

        public string RToken { set; get; }

        public string BToken { set; get; }

        public AppData CurrentApplication { set; get; }

        public EbLocation CurrentLocation { set; get; }

        public List<MobilePagesWraper> MobilePages { set; get; }

        public AppVendor Vendor { set; get; }

        public string Sid => CurrentSolution?.SolutionName;

        public string RootUrl => ApiConstants.PROTOCOL + CurrentSolution?.RootUrl;

        public string UserName => CurrentUser?.Email;

        public string UserDisplayName => CurrentUser?.FullName;

        public int UserId => CurrentUser.UserId;

        public int AppId => CurrentApplication == null ? 0 : CurrentApplication.AppId;

        public int CurrentLocId => CurrentLocation == null ? 1 : CurrentLocation.LocId;

        public LoginType LoginType
        {
            get
            {
                LoginType lType = Vendor.DefaultLoginType;

                if (CurrentSolution != null && CurrentSolution.LoginType != LoginType.NULL)
                    lType = CurrentSolution.LoginType;

                return lType;
            }
        }

        public string AppDirectory => EbBuildConfig.VendorName.ToUpper();

        public async Task InitializeSettings()
        {
            try
            {
                CurrentSolution = this.GetSolution();

                if (CurrentSolution == null && Vendor.BuildType == AppBuildType.Embedded)
                {
                    await CreateEmbeddedSolution();
                }

                if (CurrentSolution != null)
                {
                    App.DataDB.SetDbPath(CurrentSolution.SolutionName);
                }

                RToken = await this.GetRToken();
                BToken = await this.GetBToken();

                CurrentApplication = this.GetCurrentApplication();

                if (CurrentApplication != null)
                {
                    MobilePages = CurrentApplication.MobilePages;
                }

                CurrentUser = this.GetUser();
                CurrentLocation = this.GetCurrentLocation();
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
        }

        private async Task CreateEmbeddedSolution()
        {
            SolutionInfo sln = new SolutionInfo
            {
                SolutionName = Vendor.SolutionURL.Split(CharConstants.DOT)[0],
                RootUrl = Vendor.SolutionURL,
                IsCurrent = true,
            };
            CurrentSolution = sln;

            try
            {
                await Store.SetJSONAsync(AppConst.MYSOLUTIONS, new List<SolutionInfo> { sln });
                await Store.SetJSONAsync(AppConst.SOLUTION_OBJ, sln);

                App.DataDB.CreateDB(sln.SolutionName);
                await HelperFunctions.CreateDirectory();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void InitializeConfig()
        {
            EbBuildConfig conf = EbSerializers.DeserializeJsonFile<EbBuildConfig>("Configuration.Config.json");
            Vendor = conf.Current;
            HelperFunctions.SetResourceValue("Primary_Color", Vendor.GetPrimaryColor());
            HelperFunctions.SetResourceValue("PrimaryLower_Color", Vendor.GetPrimaryLowerColor());
        }

        public void Reset()
        {
            CurrentSolution = null; CurrentUser = null; RToken = null; BToken = null; CurrentApplication = null; CurrentLocation = null;
        }

        private SolutionInfo GetSolution() => Store.GetJSON<SolutionInfo>(AppConst.SOLUTION_OBJ);

        private AppData GetCurrentApplication() => Store.GetJSON<AppData>(AppConst.CURRENT_APP);

        private async Task<string> GetRToken() => await Store.GetValueAsync(AppConst.RTOKEN);

        private async Task<string> GetBToken() => await Store.GetValueAsync(AppConst.BTOKEN);

        private User GetUser() => Store.GetJSON<User>(AppConst.USER_OBJECT);

        private EbLocation GetCurrentLocation() => Store.GetJSON<EbLocation>(AppConst.CURRENT_LOCOBJ);

        public async Task<EbMobileSolutionData> GetSolutionDataAsync(bool export, int timeout = 0, Action<ResponseStatus> callback = null)
        {
            int to_calc = export ? ApiConstants.TIMEOUT_IMPORT : ApiConstants.TIMEOUT_STD;

            RestClient client = new RestClient(App.Settings.RootUrl)
            {
                Timeout = timeout == 0 ? to_calc : timeout
            };

            RestRequest request = new RestRequest(ApiConstants.GET_SOLUTION_DATA, Method.GET);

            request.AddParameter("export", export);
            request.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
            request.AddHeader(AppConst.RTOKEN, App.Settings.RToken);

            EbMobileSolutionData solData = null;
            try
            {
                IRestResponse response = await client.ExecuteAsync(request);
                if (response.IsSuccessful)
                {
                    solData = JsonConvert.DeserializeObject<EbMobileSolutionData>(response.Content);
                    await ImportSolutionData(solData, export);
                }
                else
                {
                    callback?.Invoke(response.ResponseStatus);
                    EbLog.Info("get_solution_data api failure, callback invoked");
                }
            }
            catch (Exception ex)
            {
                EbLog.Error("Error on get_solution_data request" + ex.Message);
            }
            return solData;
        }

        private async Task ImportSolutionData(EbMobileSolutionData solData, bool export)
        {
            if (solData == null)
                return;

            if (export)
            {
                EbDataSet offlineDs = solData.GetOfflineData();

                await Task.Run(async () =>
                {
                    await CommonServices.Instance.LoadLocalData(offlineDs);
                });
            }

            await Store.SetJSONAsync(AppConst.APP_COLLECTION, solData.Applications);
            await SetLocationInfo(solData.Locations);

            if (this.CurrentApplication != null)
            {
                this.CurrentApplication = solData.Applications.Find(item => item.AppId == this.CurrentApplication.AppId);
                await Store.SetJSONAsync(AppConst.CURRENT_APP, this.CurrentApplication);
            }
        }

        private async Task SetLocationInfo(List<EbLocation> locations)
        {
            if (locations == null)
                return;
            try
            {
                if (CurrentLocation == null)
                {
                    EbLocation loc = locations.Find(item => item.LocId == CurrentUser.Preference.DefaultLocation);
                    await Store.SetJSONAsync(AppConst.CURRENT_LOCOBJ, loc);
                    App.Settings.CurrentLocation = loc;
                }

                await Store.SetJSONAsync(AppConst.USER_LOCATIONS, locations);
            }
            catch (Exception ex)
            {
                EbLog.Error("Failed to set location :: " + ex.Message);
            }
        }
    }
}
