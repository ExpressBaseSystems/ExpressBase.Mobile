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

        public string Sid
        {
            get { return CurrentSolution?.SolutionName; }
        }

        public string RootUrl
        {
            get { return "https://" + CurrentSolution?.RootUrl; }
        }

        public string UserName
        {
            get { return CurrentUser?.Email; }
        }

        public string UserDisplayName
        {
            get { return CurrentUser?.FullName; }
        }

        public int UserId
        {
            get { return CurrentUser.UserId; }
        }

        public string RToken { set; get; }

        public string BToken { set; get; }

        public AppData CurrentApplication { set; get; }

        public int AppId
        {
            get
            {
                if (CurrentApplication != null)
                    return CurrentApplication.AppId;
                else
                    return 0;
            }
        }

        public EbLocation CurrentLocation { set; get; }

        public List<MobilePagesWraper> MobilePages { set; get; }

        public AppVendor Vendor { set; get; }

        public int CurrentLocId
        {
            get { return CurrentLocation.LocId; }
        }

        public async Task Resolve()
        {
            try
            {
                CurrentSolution = this.GetSolution();

                if (CurrentSolution != null)
                {
                    App.DataDB.SetDbPath(CurrentSolution.SolutionName);
                }

                RToken = await GetRToken();
                BToken = await GetBToken();

                CurrentApplication = GetCurrentApplication();

                if (CurrentApplication != null)
                {
                    MobilePages = CurrentApplication.MobilePages;
                }

                CurrentUser = GetUser();
                CurrentLocation = GetCurrentLocation();
            }
            catch (Exception ex)
            {
                EbLog.Write(ex.Message);
            }
        }

        public void InitializeConfig()
        {
            Config conf = Config.PopulateData<Config>("Config.json");
            Vendor = conf.Current;
            HelperFunctions.SetResourceValue("Primary_Color", Vendor.GetPrimaryColor());
        }

        public void Reset()
        {
            CurrentSolution = null;
            CurrentUser = null;
            RToken = null;
            BToken = null;
            CurrentApplication = null;
            CurrentLocation = null;
        }

        private SolutionInfo GetSolution()
        {
            return Store.GetJSON<SolutionInfo>(AppConst.SOLUTION_OBJ);
        }

        private AppData GetCurrentApplication()
        {
            return Store.GetJSON<AppData>(AppConst.CURRENT_APP);
        }

        private async Task<string> GetRToken()
        {
            return await Store.GetValueAsync(AppConst.RTOKEN);
        }

        private async Task<string> GetBToken()
        {
            return await Store.GetValueAsync(AppConst.BTOKEN);
        }

        private User GetUser()
        {
            return Store.GetJSON<User>(AppConst.USER_OBJECT);
        }

        private EbLocation GetCurrentLocation()
        {
            return Store.GetJSON<EbLocation>(AppConst.CURRENT_LOCOBJ);
        }

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

                if (this.CurrentApplication != null)
                {
                    this.CurrentApplication = solData.Applications.Find(item => item.AppId == this.CurrentApplication.AppId);
                    await Store.SetJSONAsync(AppConst.CURRENT_APP, this.CurrentApplication);
                }
            }
            return solData;
        }
    }
}
