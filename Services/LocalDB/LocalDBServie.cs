﻿using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.Services
{
    public class LocalDBServie : ILocalDBService
    {
        public async Task<SyncResponse> PushDataToCloud()
        {
            SyncResponse response = new SyncResponse();
            try
            {
                List<EbMobileForm> FormCollection = EbPageHelper.GetOfflineForms();

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
                        dependencyForm.FlagLocalRow(resp, resp.LocalRowId);
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
