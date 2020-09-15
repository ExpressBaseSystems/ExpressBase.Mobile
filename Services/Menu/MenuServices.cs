using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Services
{
    public class MenuServices : IMenuServices
    {
        private static MenuServices instance;

        public static MenuServices Instance => instance ??= new MenuServices();

        public async Task<List<MobilePagesWraper>> GetDataAsync()
        {
            List<MobilePagesWraper> objectList = App.Settings.MobilePages ?? new List<MobilePagesWraper>();

            if (App.Settings.CurrentUser.IsAdmin)
            {
                return objectList;
            }
            else
            {
                EbMobileSettings settings = App.Settings.CurrentApplication.AppSettings;

                if (settings != null && settings.HasMenuPreloadApi)
                {
                    if (Utils.HasInternet)
                        objectList = await GetFromMenuPreload(settings.MenuApi);
                    else
                    {
                        Utils.Alert_NoInternet();
                    }
                }
            }
            //filter all pages with respect to current location
            var filter = App.Settings.CurrentUser.FilterByLocation(objectList);

            return filter;
        }

        public async Task<List<MobilePagesWraper>> UpdateDataAsync()
        {
            List<MobilePagesWraper> collection = null;
            try
            {
                EbMobileSolutionData data = await App.Settings.GetSolutionDataAsync(false, 6000, async status =>
                {
                    collection = await this.GetDataAsync();

                    if (status == ResponseStatus.TimedOut)
                    {
                        Utils.Alert_SlowNetwork();
                        EbLog.Info("solution data api raised timeout in UpdateDataAsync");
                    }
                    else
                    {
                        Utils.Alert_NetworkError();
                        EbLog.Info("solution data api raised network error in UpdateDataAsync");
                    }
                });

                if (data != null)
                {
                    App.Settings.MobilePages = App.Settings.CurrentApplication.MobilePages;
                    collection = await this.GetDataAsync();
                    Utils.Toast("Refreshed");
                }
            }
            catch (Exception ex)
            {
                EbLog.Error("menu update failed :: " + ex.Message);
            }
            return collection ?? new List<MobilePagesWraper>();
        }

        public async Task<List<MobilePagesWraper>> GetFromMenuPreload(EbApiMeta apimeta)
        {
            RestClient client = new RestClient(App.Settings.RootUrl);
            RestRequest request = new RestRequest($"api/{apimeta.Name}/{apimeta.Version}", Method.GET);

            request.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
            request.AddHeader(AppConst.RTOKEN, App.Settings.RToken);

            MenuPreloadResponse resp = null;
            try
            {
                IRestResponse response = await client.ExecuteAsync(request);
                if (response.IsSuccessful)
                {
                    resp = JsonConvert.DeserializeObject<MenuPreloadResponse>(response.Content);
                }
            }
            catch (Exception ex)
            {
                EbLog.Error("Error on menu preload api request :: " + ex.Message);
            }

            List<MobilePagesWraper> pages = new List<MobilePagesWraper>();

            if (resp != null && resp.Result != null)
            {
                List<MobilePagesWraper> all = App.Settings.MobilePages ?? new List<MobilePagesWraper>();

                foreach (string objName in resp.Result)
                {
                    MobilePagesWraper wraper = all.Find(item => item.Name == objName);

                    if (wraper != null)
                    {
                        pages.Add(wraper);
                    }
                }
            }
            return pages;
        }

        public async Task DeployFormTables(List<MobilePagesWraper> objlist)
        {
            if (objlist == null)
                return;

            await Task.Run(() =>
            {
                foreach (MobilePagesWraper wraper in objlist)
                {
                    EbMobilePage mpage = wraper.GetPage();

                    if (mpage != null && mpage.Container is EbMobileForm form)
                    {
                        if (mpage.NetworkMode != NetworkMode.Online)
                        {
                            form.CreateTableSchema();
                        }
                    }
                }
            });
        }

        public async Task<SyncResponse> Sync()
        {
            SyncResponse response = new SyncResponse();
            try
            {
                List<EbMobileForm> FormCollection = EbPageFinder.GetOfflineForms();

                WebformData webdata = new WebformData();

                var depT = new List<string>();

                foreach (EbMobileForm Form in FormCollection)
                {
                    if (depT.Contains(Form.TableName)) continue;

                    EbMobileForm DependencyForm = Form.ResolveDependency();

                    if (DependencyForm != null)
                        depT.Add(DependencyForm.TableName);

                    EbDataTable SourceData = Form.GetLocalData();

                    for (int i = 0; i < SourceData.Rows.Count; i++)
                    {
                        PushResponse resp = await SendRecord(webdata, Form, SourceData, SourceData.Rows[i], i);

                        if (resp.RowAffected <= 0)
                            continue;

                        Form.FlagLocalRow(resp, resp.LocalRowId);
                        if (DependencyForm != null)
                            await PushDependencyData(webdata, Form, DependencyForm, resp.RowId, resp.LocalRowId);
                    }
                }
                response.Status = true;
                response.Message = "Sync complted";
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Sync failed";
                EbLog.Error(ex.Message);
            }
            return response;
        }

        private async Task<PushResponse> SendRecord(WebformData webdata, EbMobileForm Form, EbDataTable Dt, EbDataRow DataRow, int RowIndex)
        {
            PushResponse response = null;
            try
            {
                ClearWebFormData(webdata);
                SingleTable SingleTable = new SingleTable();

                int localid = Convert.ToInt32(DataRow["id"]);
                SingleRow row = this.GetRow(Form, Dt, DataRow, RowIndex);
                SingleTable.Add(row);
                webdata.MultipleTables.Add(Form.TableName, SingleTable);

                await Form.UploadFiles(localid, webdata);

                this.GetLinesEnabledData(localid, Form, webdata);

                response = await FormDataServices.Instance.SendFormDataAsync(webdata, 0, Form.WebFormRefId, row.LocId);
                response.LocalRowId = localid;
            }
            catch (Exception ex)
            {
                EbLog.Error("SyncServices.PushRow---" + ex.Message);
            }
            return response;
        }

        private void GetLinesEnabledData(int localid, EbMobileForm form, WebformData formData)
        {
            try
            {
                Dictionary<string, EbMobileControl> controls = form.ChildControls.ToControlDictionary();

                foreach (var pair in controls)
                {
                    if (pair.Value is ILinesEnabled)
                    {
                        ILinesEnabled Ilines = (pair.Value as ILinesEnabled);

                        SingleTable st = new SingleTable();
                        formData.MultipleTables.Add(Ilines.TableName, st);

                        EbDataTable data = Ilines.GetLocalData(form.TableName, localid);

                        foreach (var dr in data.Rows)
                        {
                            SingleRow row = new SingleRow { LocId = Convert.ToInt32(dr["eb_loc_id"]) };
                            row.Columns.AddRange(Ilines.GetColumnValues(data.Columns, dr));
                            st.Add(row);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
        }

        private void ClearWebFormData(WebformData wd)
        {
            wd.MultipleTables.Clear();
            wd.ExtendedTables.Clear();
        }

        private SingleRow GetRow(EbMobileForm Form, EbDataTable Dt, EbDataRow DataRow, int RowIndex)
        {
            SingleRow row = new SingleRow
            {
                RowId = 0,
                IsUpdate = false,
                LocId = Convert.ToInt32(DataRow["eb_loc_id"]),
            };
            row.Columns.AddRange(Form.GetColumnValues(Dt, RowIndex));
            return row;
        }

        private async Task PushDependencyData(WebformData webdata, EbMobileForm SourceForm, EbMobileForm DependencyForm, int LiveId, int LocalId)
        {
            try
            {
                string RefColumn = $"{SourceForm.TableName}_id";

                string query = string.Format(StaticQueries.STARFROM_TABLE_WDEP,
                    DependencyForm.GetQuery(),
                    DependencyForm.TableName,
                    RefColumn,
                    LocalId);

                EbDataTable dt = App.DataDB.DoQuery(query);

                if (dt.Rows.Any())
                {
                    SingleTable SingleTable = new SingleTable();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        FillLiveId(dt, dt.Rows[i], LiveId, RefColumn);

                        PushResponse resp = await SendRecord(webdata, DependencyForm, dt, dt.Rows[i], i);
                        if (resp.RowAffected <= 0)
                            continue;
                        DependencyForm.FlagLocalRow(resp, resp.LocalRowId);
                    }
                }
            }
            catch (Exception ex)
            {
                EbLog.Error("SyncServices.PushDependencyData---" + ex.Message);
            }
        }

        private void FillLiveId(EbDataTable dt, EbDataRow dr, int liveId, string columName)
        {
            EbDataColumn col = dt.Columns[columName];
            if (col != null)
            {
                dr[col.ColumnIndex] = liveId;
            }
        }
    }
}
