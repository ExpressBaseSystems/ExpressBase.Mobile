using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Structures;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Services
{
    public class LocalDBServie : ILocalDBService
    {
        public async Task<SyncResponse> PushDataToCloud(Loader loader = null)
        {
            SyncResponse response = new SyncResponse() { Status = true };
            int totalRecords = 0, failedCount = 0;
            try
            {
                List<EbMobileForm> FormCollection = EbPageHelper.GetOfflineForms();
                WebformData webdata = new WebformData();
                var depT = new List<string>();
                int localid;

                foreach (EbMobileForm Form in FormCollection)
                {
                    if (depT.Contains(Form.TableName)) continue;

                    EbMobileForm DependencyForm = Form.ResolveDependency();

                    if (DependencyForm != null)
                        depT.Add(DependencyForm.TableName);

                    EbDataTable SourceData = Form.GetLocalData();
                    totalRecords += SourceData.Rows.Count;
                    string msg = $"Pushing {Form.DisplayName} {{0}} of {SourceData.Rows.Count}";

                    for (int i = 0; i < SourceData.Rows.Count; i++)
                    {
                        if (loader != null)
                            Device.BeginInvokeOnMainThread(() => { loader.Message = string.Format(msg, i + 1); });

                        localid = Convert.ToInt32(SourceData.Rows[i]["id"]);
                        EbLog.Info(string.Format(msg, i + 1) + "; Local Id: " + localid);

                        Form.UpdateRetryCount(SourceData.Rows[i]);

                        PushResponse resp = await SendRecord(webdata, Form, SourceData, SourceData.Rows[i], i);

                        if (resp.RowAffected <= 0)
                        {
                            response.Status = false;
                            failedCount++;
                            EbLog.Error("Push Data Failed: " + resp.Message + "; " + resp.MessageInt);
                        }
                        else
                        {
                            Form.FlagLocalRow(resp);
                            if (DependencyForm != null)//// error submission must be consider [flow pending...]
                                await PushDependencyData(webdata, Form, DependencyForm, resp.RowId, resp.LocalRowId);
                        }
                        await Task.Delay(500); // to avoid server overload
                    }
                }
                if (response.Status)
                {
                    response.Message = "Push completed";
                    DeleteUnwantedRecords();
                }
                else if (failedCount > 0)
                    response.Message = $"{failedCount} of {totalRecords} failed to push";

                EbLog.Info(response.Message);
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Push failed: " + ex.Message;
                EbLog.Error(ex.Message);
            }
            return response;
        }

        public async Task<string> PushData(Loader loader = null)
        {
            string msg = null;
            try
            {
                if (IdentityService.IsTokenExpired())
                {
                    msg = "Session expired (Login again to refresh the session)";
                }
                else
                {
                    if (loader != null) loader.IsVisible = true;
                    loader.Message = "Sync started...";

                    SyncResponse response = await PushDataToCloud(loader);
                    if (!response.Status)
                        msg = response.Message;
                }
            }
            catch (Exception ex)
            {
                msg = "Push failed: " + ex.Message;
                Utils.Toast("Failed to push: " + ex.Message);
                EbLog.Error("Failed to sync::" + ex.Message);
            }
            if (loader != null) loader.IsVisible = false;
            return msg;
        }

        private void DeleteUnwantedRecords()
        {
            try
            {
                List<EbMobileForm> FormCollection = EbPageHelper.GetOfflineForms();
                foreach (EbMobileForm Form in FormCollection)
                {
                    DateTime date = DateTime.UtcNow.AddDays(-7);
                    DbParameter[] parameter = new DbParameter[]
                    {
                        new DbParameter{ParameterName="@strdate", DbType= (int)EbDbTypes.String, Value = date.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}
                    };
                    string query = Form.GetDeleteQuery();
                    int rowsAffected = App.DataDB.DoNonQuery(query, parameter);
                    EbLog.Info(rowsAffected + " row(s) deleted from " + Form.TableName + " and its lines table");
                }
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
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
                if (FormService.Instance == null)
                    new FormService();
                response = await FormService.Instance.SendFormDataAsync(null, webdata, 0, Form.WebFormRefId, row.LocId);
                response.LocalRowId = localid;
            }
            catch (Exception ex)
            {
                EbLog.Error("SyncServices.PushRow---" + ex.Message);
            }
            return response;
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

        private async Task PushDependencyData(WebformData webdata, EbMobileForm sourceForm, EbMobileForm dependencyForm, int liveId, int localId)
        {
            try
            {
                string RefColumn = $"{sourceForm.TableName}_id";
                string query = string.Format(StaticQueries.STARFROM_TABLE_WDEP, dependencyForm.GetQuery(), dependencyForm.TableName, RefColumn, localId);

                EbDataTable dt = App.DataDB.DoQuery(query);

                if (dt.Rows.Any())
                {
                    SingleTable SingleTable = new SingleTable();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        FillLiveId(dt, dt.Rows[i], liveId, RefColumn);

                        PushResponse resp = await SendRecord(webdata, dependencyForm, dt, dt.Rows[i], i);
                        if (resp.RowAffected <= 0)
                            continue;
                        dependencyForm.FlagLocalRow(resp);
                    }
                }
            }
            catch (Exception ex)
            {
                EbLog.Error("SyncServices.PushDependencyData---" + ex.Message);
            }
        }

        private void GetLinesEnabledData(int localid, EbMobileForm form, WebformData formData)
        {
            try
            {
                Dictionary<string, EbMobileControl> controls = form.ChildControls.ToControlDictionary();

                foreach (var ctrl in controls.Values)
                {
                    if (ctrl is ILinesEnabled Ilines)
                    {
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
