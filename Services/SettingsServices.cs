using ExpressBase.Mobile.Configuration;
using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.Services
{
    public class SettingsServices
    {
        public SolutionInfo CurrentSolution { set; get; }

        public User CurrentUser { set; get; }

        public string RToken { set; get; }

        public string BToken { set; get; }

        public AppData CurrentApplication { set; get; }

        public EbLocation CurrentLocation { set; get; }

        public List<MobilePagesWraper> MobilePages { set; get; }

        public AppVendor Vendor { set; get; }

        public string Sid
        {
            get => CurrentSolution?.SolutionName;
        }

        public string RootUrl
        {
            get => "https://" + CurrentSolution?.RootUrl;
        }

        public string UserName
        {
            get => CurrentUser?.Email;
        }

        public string UserDisplayName
        {
            get => CurrentUser?.FullName;
        }

        public int UserId
        {
            get => CurrentUser.UserId;
        }

        public int AppId
        {
            get => (CurrentApplication != null) ? CurrentApplication.AppId : 0;
        }

        public int CurrentLocId
        {
            get => CurrentLocation.LocId;
        }

        public async Task InitializeSettings()
        {
            try
            {
                CurrentSolution = this.GetSolution();

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
                EbLog.Write(ex.Message);
            }
        }

        public void InitializeConfig()
        {
            Config conf = EbSerializers.DeserializeJsonFile<Config>("Configuration.Config.json");
            Vendor = conf.Current;
            HelperFunctions.SetResourceValue("Primary_Color", Vendor.GetPrimaryColor());
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

        public async Task<EbMobileSolutionData> GetSolutionDataAsync(bool export)
        {
            RestClient client = new RestClient(App.Settings.RootUrl);
            RestRequest request = new RestRequest("api/get_solution_data", Method.GET);

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
                }
            }
            catch (Exception ex)
            {
                EbLog.Write("Error on get_solution_data request" + ex.Message);
            }

            if (solData != null)
            {
                if (export)
                {
                    EbDataSet offlineDs = solData.GetOfflineData();
                    solData.ClearOfflineData();

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
            return solData;
        }

        private async Task SetLocationInfo(List<EbLocation> locations)
        {
            if (locations == null) return;

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
                EbLog.Write("Failed to set location :: "+ex.Message);
            }
        }
    }
}
