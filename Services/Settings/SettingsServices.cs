﻿using ExpressBase.Mobile.Configuration;
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

        public bool SyncInProgress { set; get; }

        public SolutionInfo CurrentSolution { set; get; }

        public User CurrentUser { set; get; }

        public string RToken { set; get; }

        public string BToken { set; get; }

        public AppData CurrentApplication { set; get; }

        public EbLocation CurrentLocation { set; get; }

        public EbBTDevice SelectedBtDevice { set; get; }

        public string PrinterPreference { set; get; }

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

            if (syncInfo.LastSyncTs != DateTime.MinValue)
            {
                metaDict.Add(AppConst.last_sync_ts, syncInfo.LastSyncTs);
            }
            metaDict.Add(AppConst.draft_ids, GetErrorDraftIds());
            INativeHelper helper = DependencyService.Get<INativeHelper>();
            metaDict.Add(AppConst.app_version, helper.AppVersion);
            metaDict.Add(AppConst.device_id, helper.DeviceId);
            request.AddParameter(AppConst.metadata, JsonConvert.SerializeObject(metaDict));

            EbMobileSolutionData solutionData = null;
            try
            {
                IRestResponse response = await client.ExecuteAsync(request);
                if (response.IsSuccessful)
                {
                    Device.BeginInvokeOnMainThread(() => { loader.Message = "Processing pulled data..."; });

                    solutionData = JsonConvert.DeserializeObject<EbMobileSolutionData>(response.Content);

                    (bool incorrectDate, bool maintenanceMode, string msg) = await UpdateLastSyncInfo(solutionData, syncInfo, helper.AppVersion);
                    resp.Message = msg;

                    if (!maintenanceMode)
                    {
                        List<AppData> oldAppData = Store.GetJSON<List<AppData>>(AppConst.APP_COLLECTION);
                        MergeObjectsInSolutionData(solutionData, oldAppData);
                        await ImportSolutionData(solutionData);
                        Device.BeginInvokeOnMainThread(() => { loader.Message = "Importing latest document ids..."; });
                        EbLog.Info("Importing latest document ids...");
                        if (await GetLatestAutoId(solutionData.Applications))
                        {
                            if (!incorrectDate)
                            {
                                syncInfo.LastSyncTs = solutionData.last_sync_ts;
                                syncInfo.LastOfflineSaveTs = solutionData.last_sync_ts;
                            }
                            resp.Status = true;
                            syncInfo.PullSuccess = true;
                            await Store.SetJSONAsync(AppConst.LAST_SYNC_INFO, syncInfo);
                            EbLog.Info("[GetSolutionDataAsyncV2] api success");
                        }
                        else
                            resp.Message = "Failed to import latest doc ids";
                    }
                    else
                    {
                        syncInfo.PullSuccess = true;
                        await Store.SetJSONAsync(AppConst.LAST_SYNC_INFO, syncInfo);
                    }
                }
                else
                {
                    //callback?.Invoke(response.ResponseStatus);
                    resp.Message = "Pull failed. Try after sometime.";
                    EbLog.Info("[GetSolutionData] api failure, callback invoked");
                }
            }
            catch (Exception ex)
            {
                resp.Message = "Exception: " + ex.Message;
                EbLog.Error("Error on [GetSolutionData] request" + ex.Message);
            }
            return resp;
        }

        private async Task<(bool, bool, string)> UpdateLastSyncInfo(EbMobileSolutionData solutionData, LastSyncInfo syncInfo, string appVersion)
        {
            string msg = null;
            bool leaveLastSyncTsCheck = false, incorrectDate = false, maintenanceMode = false;

            if (solutionData.MetaData != null)
            {
                if (solutionData.MetaData.TryGetValue(AppConst.maintenance_msg, out object val) && val != null)
                {
                    msg = val.ToString();
                    maintenanceMode = true;
                }
                else if (solutionData.MetaData.TryGetValue(AppConst.session_expired, out object val2) && bool.TryParse(val2.ToString(), out bool b) && b)
                {
                    msg = AppConst.session_expired;
                    maintenanceMode = true;
                }
                else if (solutionData.MetaData.TryGetValue(AppConst.leave_ts_check, out object val3) && bool.TryParse(val3.ToString(), out bool st) && st)
                {
                    EbLog.Warning("Last sync ts check avoided");
                    leaveLastSyncTsCheck = true;
                }

                if (solutionData.MetaData.TryGetValue(AppConst.app_version, out object val4) && val4 != null)
                    syncInfo.LatestAppVersion = val4.ToString();
                else
                    syncInfo.LatestAppVersion = null;
            }
            else
                syncInfo.LatestAppVersion = null;

            if (!(solutionData.last_sync_ts > DateTime.Now.Subtract(new TimeSpan(0, 2, 0)) &&
                solutionData.last_sync_ts < DateTime.Now.Add(new TimeSpan(0, 3, 0))) && !leaveLastSyncTsCheck)
            {
                incorrectDate = true;
                EbLog.Warning("Device date time is incorrect. Server time: " + solutionData.last_sync_ts);
                if (msg == null)
                    msg = "Device date time is incorrect";
            }

            if (msg != null)
                EbLog.Warning(msg);

            syncInfo.PullSuccess = false;
            syncInfo.IsLoggedOut = false;
            syncInfo.SolnId = App.Settings.Sid;
            await Store.SetJSONAsync(AppConst.LAST_SYNC_INFO, syncInfo);

            return (incorrectDate, maintenanceMode, msg);
        }

        private string GetErrorDraftIds()
        {
            List<int> draft_ids = new List<int>();
            int i;
            try
            {
                var depT = new List<string>();
                List<EbMobileForm> FormCollection = EbPageHelper.GetOfflineForms();

                foreach (EbMobileForm Form in FormCollection)
                {
                    if (depT.Contains(Form.TableName)) continue;

                    EbDataTable dt = Form.GetDraftIds();

                    foreach (EbDataRow dr in dt.Rows)
                    {
                        i = Convert.ToInt32(dr[0]);
                        if (!draft_ids.Contains(i))
                            draft_ids.Add(i);
                    }
                }
            }
            catch (Exception ex)
            {
                EbLog.Error("GetErrorDraftIds: " + ex.Message);
            }

            return JsonConvert.SerializeObject(draft_ids);
        }

        private void UpdateErrorDraftIds(List<int> DraftIds)
        {
            try
            {
                List<string> depT = new List<string>();
                int i;
                List<EbMobileForm> FormCollection = EbPageHelper.GetOfflineForms();

                foreach (EbMobileForm Form in FormCollection)
                {
                    if (depT.Contains(Form.TableName)) continue;

                    EbDataTable dt = Form.GetDraftIds();

                    foreach (EbDataRow dr in dt.Rows)
                    {
                        i = Convert.ToInt32(dr[0]);
                        if (!DraftIds.Contains(i))
                        {
                            Form.UpdateDraftAsSynced(Convert.ToInt32(dr[1]), i);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                EbLog.Error("UpdateErrorDraftIds: " + ex.Message);
            }
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
            if (App.Settings.SyncInProgress)
            {
                EbLog.Info(App.Settings.Sid + ": Sync in progress...");
                return null;
            }
            App.Settings.SyncInProgress = true;
            EbLog.BackupLogFiles();
            EbMobileSolutionData solutionData = null;
            bool flag = false;
            try
            {
                loader.Message = "Sync started...";
                EbLog.Info("Sync started...");

                LocalDBServie service = new LocalDBServie();
                SyncResponse response = await service.PushDataToCloud(loader);

                if (response.Status)
                    loader.Message = string.Empty;
                else
                    loader.Message = response.Message + " \n";

                loader.Message += "Fetching data from server...";
                EbLog.Info("Fetching data from server...");

                RestClient client = new RestClient(App.Settings.RootUrl)
                {
                    Timeout = ApiConstants.TIMEOUT_IMPORT
                };
                RestRequest request = new RestRequest(ApiConstants.GET_SOLUTION_DATAv2, Method.POST);

                request.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
                request.AddHeader(AppConst.RTOKEN, App.Settings.RToken);
                Dictionary<string, object> metaDict = new Dictionary<string, object>();
                metaDict.Add(AppConst.draft_ids, GetErrorDraftIds());
                INativeHelper helper = DependencyService.Get<INativeHelper>();
                metaDict.Add(AppConst.app_version, helper.AppVersion);
                metaDict.Add(AppConst.device_id, helper.DeviceId);
                request.AddParameter(AppConst.metadata, JsonConvert.SerializeObject(metaDict));

                IRestResponse resp = await client.ExecuteAsync(request);
                if (resp.IsSuccessful)
                {
                    loader.Message = "Processing pulled data...";

                    LastSyncInfo syncInfo = App.Settings.SyncInfo;
                    if (syncInfo == null)
                        syncInfo = new LastSyncInfo();
                    solutionData = JsonConvert.DeserializeObject<EbMobileSolutionData>(resp.Content);

                    (bool incorrectDate, bool maintenanceMode, string msg) = await UpdateLastSyncInfo(solutionData, syncInfo, helper.AppVersion);

                    if (!maintenanceMode)
                    {
                        await ImportSolutionData(solutionData);
                        loader.Message = "Importing latest document ids...";
                        EbLog.Info("Importing latest document ids...");
                        if (await GetLatestAutoId(solutionData.Applications))
                        {
                            if (!incorrectDate)
                            {
                                syncInfo.LastSyncTs = solutionData.last_sync_ts;
                                syncInfo.LastOfflineSaveTs = solutionData.last_sync_ts;
                            }
                            else
                                Utils.Toast(msg);

                            syncInfo.PullSuccess = true;
                            await Store.SetJSONAsync(AppConst.LAST_SYNC_INFO, syncInfo);
                            flag = true;
                        }
                        else
                            Utils.Toast("Failed to import latest doc ids");
                    }
                    else
                    {
                        Utils.Toast(msg);
                        syncInfo.PullSuccess = true;
                        await Store.SetJSONAsync(AppConst.LAST_SYNC_INFO, syncInfo);
                    }
                }
                else
                {
                    string t = "Failed to get solution data :: " + resp.StatusDescription;
                    Utils.Toast(t);
                    EbLog.Warning(t);
                }
            }
            catch (Exception ex)
            {
                EbLog.Error("Error on [GetSolutionData] request" + ex.Message);
                Utils.Toast(ex.Message);
            }
            loader.IsVisible = false;
            App.Settings.SyncInProgress = false;
            return flag ? solutionData : null;
        }


        private async Task ImportSolutionData(EbMobileSolutionData solutionData)
        {
            if (solutionData == null) return;

            EbDataSet importData = solutionData.GetOfflineData();//Offline data cleared OnSerializing

            await Store.SetJSONAsync(AppConst.APP_COLLECTION, solutionData.Applications);
            await SetLocationInfo(solutionData.Locations);
            await SetCurrentUser(solutionData.CurrentUser);
            await SetSolutionObject(solutionData.CurrentSolution);
            await SetImagesInPdf(solutionData.Images);

            DBService.Current.ImportData(importData);
            UpdateErrorDraftIds(solutionData.DraftIds);

            if (solutionData.ProfilePages != null && solutionData.ProfilePages.Count > 0)
            {
                await Store.SetJSONAsync(AppConst.EXTERNAL_PAGES, solutionData.ProfilePages);
                ExternalMobilePages = solutionData.ProfilePages;
            }

            if (CurrentApplication != null)
            {
                CurrentApplication = solutionData.Applications.Find(item => item.AppId == CurrentApplication.AppId);
                MobilePages = CurrentApplication?.MobilePages;
                WebObjects = CurrentApplication?.WebObjects;
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
                        Timeout = ApiConstants.TIMEOUT_IMPORT
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
                        EbLog.Info("GetLatestAutoId success");
                        return true;
                    }
                    else
                        EbLog.Warning("GetLatestAutoId failed");
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
                EbLocation cur_loc = null;

                if (CurrentLocation != null)
                    cur_loc = locations.Find(item => item.LocId == CurrentLocation.LocId);

                if (cur_loc == null)
                    cur_loc = locations.Find(item => item.LocId == CurrentUser.Preference.DefaultLocation);

                if (cur_loc == null && locations.Count > 0)
                    cur_loc = locations[0];

                await Store.SetJSONAsync(AppConst.CURRENT_LOCOBJ, cur_loc);
                App.Settings.CurrentLocation = cur_loc;
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

                    App.DataDB.SetDbPath(current.SolutionName);
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
