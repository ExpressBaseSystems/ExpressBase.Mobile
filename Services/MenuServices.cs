using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Services
{
    public interface IMenuServices
    {
        Task<SyncResponse> Sync();

        Task<MobilePageCollection> GetDataAsync();

        Task DeployFormTables(List<MobilePagesWraper> objlist);
    }

    public class MenuServices : IMenuServices
    {
        public async Task<MobilePageCollection> GetDataAsync()
        {
            MobilePageCollection collection;

            try
            {
                List<MobilePagesWraper> objlist = Store.GetJSON<List<MobilePagesWraper>>(AppConst.OBJ_COLLECTION);

                if (objlist == null)
                {
                    if (!Utils.HasInternet)
                    {
                        DependencyService.Get<IToast>().Show("You are not connected to internet");
                        throw new Exception();
                    }

                    collection = await GetFromApiAsync();
                    objlist = collection.Pages;

                    await Store.SetJSONAsync(AppConst.OBJ_COLLECTION, objlist);
                }
                else
                {
                    collection = new MobilePageCollection
                    {
                        Pages = objlist
                    };
                }
            }
            catch (Exception)
            {
                collection = new MobilePageCollection();
            }
            return collection;
        }

        public async Task<MobilePageCollection> GetFromApiAsync()
        {
            try
            {
                RestClient client = new RestClient(App.Settings.RootUrl);

                RestRequest request = new RestRequest("api/objects_by_app", Method.GET);

                request.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
                request.AddHeader(AppConst.RTOKEN, App.Settings.RToken);

                request.AddParameter("appid", App.Settings.AppId);
                request.AddParameter("locid", App.Settings.CurrentLocId);
                request.AddParameter("pull_data", true);

                var response = await client.ExecuteAsync(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<MobilePageCollection>(response.Content);
                }
            }
            catch (Exception ex)
            {
                EbLog.Write("RestServices.GetEbObjects---" + ex.Message);
                return new MobilePageCollection();
            }
            return new MobilePageCollection();
        }

        public async Task DeployFormTables(List<MobilePagesWraper> objlist)
        {
            await Task.Run(() =>
            {
                foreach (MobilePagesWraper _p in objlist)
                {
                    EbMobilePage mpage = _p.ToPage();

                    if (mpage != null && mpage.Container is EbMobileForm)
                    {
                        if (mpage.NetworkMode != NetworkMode.Online)
                            (mpage.Container as EbMobileForm).CreateTableSchema();
                    }
                }
            });
        }

        public async Task<SyncResponse> Sync()
        {
            SyncResponse response = new SyncResponse();
            try
            {
                List<EbMobileForm> FormCollection = HelperFunctions.GetOfflineForms();

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
                EbLog.Write(ex.Message);
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
                EbLog.Write("SyncServices.PushRow---" + ex.Message);
            }
            return response;
        }

        private void GetLinesEnabledData(int localid, EbMobileForm form, WebformData formData)
        {
            try
            {
                var controls = form.ChildControls.ToControlDictionary();

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
                EbLog.Write(ex.Message);
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
                EbLog.Write("SyncServices.PushDependencyData---" + ex.Message);
            }
        }

        private void FillLiveId(EbDataTable dt, EbDataRow dr, int liveId, string columName)
        {
            EbDataColumn col = dt.Columns[columName];

            if (col != null)
                dr[col.ColumnIndex] = liveId;
        }
    }
}
