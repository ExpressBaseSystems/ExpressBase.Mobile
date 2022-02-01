using ExpressBase.Mobile.Configuration;
using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services.LocalDB;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Services
{
    public sealed class SettingsServices : BaseService, ISettingsService
    {
        private static SettingsServices _current;

        public static SettingsServices Current => _current ??= new SettingsServices();

        private SettingsServices() { }

        public bool IsFirstRun { set; get; }

        public SolutionInfo CurrentSolution { set; get; }

        public User CurrentUser { set; get; }

        public string RToken { set; get; }

        public string BToken { set; get; }

        public AppData CurrentApplication { set; get; }

        public EbLocation CurrentLocation { set; get; }

        public List<MobilePagesWraper> MobilePages { set; get; }

        public List<WebObjectsWraper> WebObjects { set; get; }

        public List<MobilePagesWraper> ExternalMobilePages { set; get; }

        public AppVendor Vendor { set; get; }

        public LastSyncInfo SyncInfo { get; set; }

        public string Sid => CurrentSolution?.SolutionName;

        public string ISid => CurrentSolution?.ISolutionId;

        public string SolutionName => CurrentSolution.GetSolutionDisplayName();

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

                if (CurrentSolution != null)
                {
                    App.DataDB.SetDbPath(CurrentSolution.SolutionName);
                }

                IsFirstRun = this.IsApplicationFirstRun();

                RToken = await this.GetRToken();
                BToken = await this.GetBToken();

                CurrentApplication = this.GetCurrentApplication();

                if (CurrentApplication != null)
                {
                    MobilePages = CurrentApplication.MobilePages;
                    WebObjects = CurrentApplication.WebObjects;
                }

                CurrentUser = this.GetUser();
                CurrentLocation = this.GetCurrentLocation();
                ExternalMobilePages = this.GetExternalPages();
                SyncInfo = this.GetSyncInfo();
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
        }

        public void InitializeConfig()
        {
            try
            {
                EbBuildConfig conf = EbSerializers.DeserializeJsonFile<EbBuildConfig>("Configuration.Config.json");
                Vendor = conf.Current;
                HelperFunctions.SetResourceValue("Primary_Color", Vendor.GetPrimaryColor());
                HelperFunctions.SetResourceValue("Primary_FontColor", Vendor.GetPrimaryFontColor());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void ResetSettings()
        {
            CurrentSolution = null; CurrentUser = null; RToken = null; BToken = null; CurrentApplication = null; CurrentLocation = null;
        }

        private bool IsApplicationFirstRun()
        {
            try
            {
                if (!Store.TryGetValue<bool>(AppConst.FIRST_RUN, out _) && CurrentSolution == null)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("application first run check error, " + ex.Message);
            }
            return false;
        }

        private SolutionInfo GetSolution() => Store.GetJSON<SolutionInfo>(AppConst.SOLUTION_OBJ);

        private AppData GetCurrentApplication() => Store.GetJSON<AppData>(AppConst.CURRENT_APP);

        private async Task<string> GetRToken() => await Store.GetValueAsync(AppConst.RTOKEN);

        private async Task<string> GetBToken() => await Store.GetValueAsync(AppConst.BTOKEN);

        private User GetUser() => Store.GetJSON<User>(AppConst.USER_OBJECT);

        private EbLocation GetCurrentLocation() => Store.GetJSON<EbLocation>(AppConst.CURRENT_LOCOBJ);

        private List<MobilePagesWraper> GetExternalPages() => Store.GetJSON<List<MobilePagesWraper>>(AppConst.EXTERNAL_PAGES);

        private LastSyncInfo GetSyncInfo()
        {
            LastSyncInfo syncInfo = Store.GetJSON<LastSyncInfo>(AppConst.LAST_SYNC_INFO);
            if (syncInfo == null || syncInfo.SolnId != Sid)
            {
                syncInfo = new LastSyncInfo() { SolnId = Sid };
                Store.SetJSONAsync(AppConst.LAST_SYNC_INFO, syncInfo);
            }
            return syncInfo;
        }

        //version 2
        public async Task<SyncResponse> GetSolutionDataAsyncV2(Loader loader)
        {
            EbLog.BackupLogFiles();
            SyncResponse resp = new SyncResponse();
            RestClient client = new RestClient(App.Settings.RootUrl)
            {
                Timeout = ApiConstants.TIMEOUT_IMPORT
            };
            RestRequest request = new RestRequest(ApiConstants.GET_SOLUTION_DATAv2, Method.POST);

            request.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
            request.AddHeader(AppConst.RTOKEN, App.Settings.RToken);

            Dictionary<string, object> metaDict = new Dictionary<string, object>();
            LastSyncInfo syncInfo = App.Settings.SyncInfo;
            if (syncInfo == null)
                syncInfo = new LastSyncInfo();
            syncInfo.PullSuccess = false;
            syncInfo.IsLoggedOut = false;

            if (syncInfo.LastSyncTs != DateTime.MinValue)
            {
                metaDict.Add("last_sync_ts", syncInfo.LastSyncTs);
            }
            request.AddParameter("metadata", JsonConvert.SerializeObject(metaDict));

            EbMobileSolutionData solutionData = null;
            try
            {
                IRestResponse response = await client.ExecuteAsync(request);
                if (response.IsSuccessful)
                {
                    Device.BeginInvokeOnMainThread(() => { loader.Message = "Processing pulled data..."; });
                    solutionData = JsonConvert.DeserializeObject<EbMobileSolutionData>(response.Content);

                    if (!(solutionData.last_sync_ts > DateTime.Now.Subtract(new TimeSpan(0, 2, 0)) &&
                        solutionData.last_sync_ts < DateTime.Now.Add(new TimeSpan(0, 2, 0))))
                    {
                        resp.Message = "Device date time is incorrect";
                    }
                    else
                    {
                        List<AppData> oldAppData = Store.GetJSON<List<AppData>>(AppConst.APP_COLLECTION);
                        MergeObjectsInSolutionData(solutionData, oldAppData);
                        await ImportSolutionData(solutionData, true);
                        Device.BeginInvokeOnMainThread(() => { loader.Message = "Importing latest document ids..."; });
                        if (await GetLatestAutoId(solutionData.Applications))
                        {
                            resp.Status = true;
                            syncInfo.LastSyncTs = solutionData.last_sync_ts;
                            syncInfo.LastOfflineSaveTs = solutionData.last_sync_ts;
                            syncInfo.PullSuccess = true;
                        }
                    }
                }
                else
                {
                    //callback?.Invoke(response.ResponseStatus);
                    EbLog.Info("[GetSolutionData] api failure, callback invoked");
                }
            }
            catch (Exception ex)
            {
                EbLog.Error("Error on [GetSolutionData] request" + ex.Message);
            }
            await Store.SetJSONAsync(AppConst.LAST_SYNC_INFO, syncInfo);
            return resp;
        }

        private void MergeObjectsInSolutionData(EbMobileSolutionData New, List<AppData> OldApps)
        {
            if (OldApps == null)
                return;
            foreach (AppData app in New.Applications)
            {
                foreach (MobilePagesWraper wraper in app.MobilePages)
                {
                    if (string.IsNullOrEmpty(wraper.Json))
                    {
                        foreach (AppData _app in OldApps)
                        {
                            MobilePagesWraper _w = _app.MobilePages.Find(e => e.RefId == wraper.RefId);
                            if (_w != null)
                            {
                                wraper.Json = _w.Json;
                                break;
                            }
                        }
                    }
                }
                foreach (WebObjectsWraper wraper in app.WebObjects)
                {
                    if (string.IsNullOrEmpty(wraper.Json))
                    {
                        foreach (AppData _app in OldApps)
                        {
                            WebObjectsWraper _w = _app.WebObjects.Find(e => e.RefId == wraper.RefId);
                            if (_w != null)
                            {
                                wraper.Json = _w.Json;
                                break;
                            }
                        }
                    }
                }
            }
        }

        public async Task<EbMobileSolutionData> GetSolutionDataAsync(Loader loader)
        {
            EbLog.BackupLogFiles();
            EbMobileSolutionData solutionData = null;
            bool flag = false;
            try
            {
                loader.Message = "Sync started...";

                LocalDBServie service = new LocalDBServie();
                SyncResponse response = await service.PushDataToCloud(loader);

                if (response.Status)
                    loader.Message = string.Empty;
                else
                    loader.Message = response.Message + " \n";

                loader.Message += "Fetching data from server...";

                RestClient client = new RestClient(App.Settings.RootUrl)
                {
                    Timeout = ApiConstants.TIMEOUT_IMPORT
                };
                RestRequest request = new RestRequest(ApiConstants.GET_SOLUTION_DATAv2, Method.POST);

                request.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
                request.AddHeader(AppConst.RTOKEN, App.Settings.RToken);
                Dictionary<string, object> metaDict = new Dictionary<string, object>();
                request.AddParameter("metadata", JsonConvert.SerializeObject(metaDict));

                IRestResponse resp = await client.ExecuteAsync(request);
                if (resp.IsSuccessful)
                {
                    loader.Message = "Processing pulled data...";
                    solutionData = JsonConvert.DeserializeObject<EbMobileSolutionData>(resp.Content);
                    await ImportSolutionData(solutionData, true);
                    loader.Message = "Importing latest document ids...";
                    if (await GetLatestAutoId(solutionData.Applications))
                    {
                        LastSyncInfo syncInfo = App.Settings.SyncInfo;
                        syncInfo.PullSuccess = true;
                        syncInfo.LastSyncTs = solutionData.last_sync_ts;
                        syncInfo.LastOfflineSaveTs = solutionData.last_sync_ts;
                        syncInfo.IsLoggedOut = false;
                        syncInfo.SolnId = App.Settings.Sid;
                        await Store.SetJSONAsync(AppConst.LAST_SYNC_INFO, syncInfo);
                        flag = true;
                    }
                    else
                        Utils.Toast("Sync failed");
                }
                else
                    Utils.Toast(response.Message ?? "Sync failed");

            }
            catch (Exception ex)
            {
                EbLog.Error("Error on [GetSolutionData] request" + ex.Message);
                Utils.Toast(ex.Message);
            }
            loader.IsVisible = false;
            return flag ? solutionData : null;
        }


        private async Task ImportSolutionData(EbMobileSolutionData solutionData, bool export)
        {
            if (solutionData == null) return;

            if (export)
            {
                EbDataSet importData = solutionData.GetOfflineData();

                DBService.Current.ImportData(importData);
            }

            await Store.SetJSONAsync(AppConst.APP_COLLECTION, solutionData.Applications);
            await SetLocationInfo(solutionData.Locations);
            await SetCurrentUser(solutionData.CurrentUser);
            await SetSolutionObject(solutionData.CurrentSolution);
            await SetImagesInPdf(solutionData.Images);

            if (solutionData.ProfilePages != null && solutionData.ProfilePages.Count > 0)
            {
                await Store.SetJSONAsync(AppConst.EXTERNAL_PAGES, solutionData.ProfilePages);
                ExternalMobilePages = solutionData.ProfilePages;
            }

            if (CurrentApplication != null)
            {
                CurrentApplication = solutionData.Applications.Find(item => item.AppId == CurrentApplication.AppId);
                await Store.SetJSONAsync(AppConst.CURRENT_APP, CurrentApplication);
            }
        }

        private async Task<bool> GetLatestAutoId(List<AppData> Applications)
        {
            List<EbMobileAutoIdData> autoIdData = new List<EbMobileAutoIdData>();
            try
            {
                foreach (AppData app in Applications)
                {
                    foreach (MobilePagesWraper mobPageWrap in app.MobilePages)
                    {
                        EbMobilePage page = mobPageWrap.GetPage();
                        if (page.Container is EbMobileForm form)
                        {
                            EbMobileAutoId autoId = (EbMobileAutoId)form.ChildControls.Find(e => e is EbMobileAutoId);
                            if (autoId != null && !string.IsNullOrWhiteSpace(form.TableName))
                            {
                                string query = autoId.PrefixExpr?.GetCode();
                                if (!string.IsNullOrWhiteSpace(query) && page.NetworkMode == NetworkMode.Offline)
                                {
                                    EbDataTable dt = App.DataDB.DoQuery(query);
                                    if (dt.Rows.Count > 0)
                                    {
                                        autoIdData.Add(new EbMobileAutoIdData()
                                        {
                                            Table = form.TableName,
                                            Column = autoId.Name,
                                            Prefix = dt.Rows[0][0]?.ToString()
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
                if (autoIdData.Count > 0)
                {
                    RestRequest request = new RestRequest(ApiConstants.PULL_LATEST_AUTOID, Method.POST);
                    RestClient client = new RestClient(App.Settings.RootUrl)
                    {
                        Timeout = 10000
                    };
                    request.AddParameter("data", JsonConvert.SerializeObject(autoIdData));
                    request.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
                    request.AddHeader(AppConst.RTOKEN, App.Settings.RToken);
                    IRestResponse response = await client.ExecuteAsync(request);
                    if (response.IsSuccessful)
                    {
                        EbDataSet ds = new EbDataSet();
                        EbMobileAutoIdDataResponse resp = JsonConvert.DeserializeObject<EbMobileAutoIdDataResponse>(response.Content);
                        ds.Tables.Add(resp.OfflineData);
                        DBService.Current.ImportData(ds);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                EbLog.Error("Error in GetLatestAutoId: " + ex.Message);
            }
            return autoIdData.Count > 0 ? false : true;
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

        private async Task SetSolutionObject(Eb_Solution solution)
        {
            if (solution == null) return;

            try
            {
                this.CurrentSolution.SolutionObject = solution;

                List<SolutionInfo> allSolutions = Utils.Solutions;

                SolutionInfo current = allSolutions.Find(x => x.SolutionName == CurrentSolution.SolutionName && x.RootUrl == CurrentSolution.RootUrl);

                if (current != null)
                {
                    current.SolutionObject = solution;
                    await Store.SetJSONAsync(AppConst.MYSOLUTIONS, allSolutions);
                    await Store.SetJSONAsync(AppConst.SOLUTION_OBJ, current);
                }
            }
            catch (Exception ex)
            {
                EbLog.Error("failed to set solution object, " + ex.Message);
            }
        }

        private async Task SetCurrentUser(User currentUser)
        {
            if (currentUser == null) return;

            try
            {
                this.CurrentUser = currentUser;
                await Store.SetJSONAsync(AppConst.USER_OBJECT, currentUser);
            }
            catch (Exception ex)
            {
                EbLog.Error("failed to set user object, " + ex.Message);
            }
        }

        private async Task SetImagesInPdf(Dictionary<int, byte[]> imgs)
        {
            if (imgs == null || imgs.Count == 0)
                return;
            try
            {
                Dictionary<int, byte[]> storeImgs = Store.GetJSON<Dictionary<int, byte[]>>(AppConst.IMAGES_IN_PDF);
                if (storeImgs == null || storeImgs.Count == 0)
                    await Store.SetJSONAsync(AppConst.IMAGES_IN_PDF, imgs);
                else
                {
                    foreach (KeyValuePair<int, byte[]> item in imgs)
                    {
                        if (!storeImgs.ContainsKey(item.Key))
                            storeImgs.Add(item.Key, item.Value);
                    }
                    await Store.SetJSONAsync(AppConst.IMAGES_IN_PDF, storeImgs);
                }
            }
            catch (Exception ex)
            {
                EbLog.Error("failed to set images in pdf: " + ex.Message);
            }
        }
    }
}
