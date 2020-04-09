using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Structures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpressBase.Mobile.Services
{
    public class SyncServices
    {
        private static SyncServices instance;

        public static SyncServices Instance => instance ?? (instance = new SyncServices());

        public string Message { set; get; }

        private List<EbMobileForm> FormCollection;

        private WebformData WebFormData;

        public SyncResponse Sync()
        {
            SyncResponse response = new SyncResponse();
            try
            {
                FormCollection = this.GetForms();
                WebFormData = new WebformData();
                this.InitPush(response);
            }
            catch (Exception ex)
            {
                Log.Write("SyncServices.Sync---" + ex.Message);
            }
            return response;
        }

        private List<EbMobileForm> GetForms()
        {
            List<EbMobileForm> ls = new List<EbMobileForm>();
            var pages = Settings.Objects;
            foreach (MobilePagesWraper _p in pages)
            {
                EbMobilePage mpage = _p.ToPage();
                if (mpage != null && mpage.Container is EbMobileForm)
                {
                    if (string.IsNullOrEmpty((mpage.Container as EbMobileForm).WebFormRefId))
                        continue;
                    if (mpage.NetworkMode == NetworkMode.Offline || mpage.NetworkMode == NetworkMode.Mixed)
                    {
                        ls.Add(mpage.Container as EbMobileForm);
                    }
                }
            }
            return ls;
        }

        private void InitPush(SyncResponse response)
        {
            var depT = new List<string>();
            try
            {
                foreach (EbMobileForm Form in this.FormCollection)
                {
                    if (depT.Contains(Form.TableName)) continue;

                    EbMobileForm DependencyForm = Form.ResolveDependency();

                    if (DependencyForm != null) depT.Add(DependencyForm.TableName);

                    EbDataTable SourceData = Form.GetLocalData();

                    for (int i = 0; i < SourceData.Rows.Count; i++)
                    {
                        PushResponse resp = this.PushRow(Form, SourceData, SourceData.Rows[i], i);

                        if (resp.RowAffected <= 0) continue;

                        Form.FlagLocalRow(resp, resp.LocalRowId);

                        if (DependencyForm != null) PushDependencyData(Form, DependencyForm, resp.RowId, resp.LocalRowId);
                    }
                }
                response.Status = true;
                response.Message = "Sync complted";
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Sync failed";
                Log.Write("SyncServices.InitPush---" + ex.Message);
            }
        }

        private PushResponse PushRow(EbMobileForm Form, EbDataTable Dt, EbDataRow DataRow, int RowIndex)
        {
            PushResponse response = null;
            try
            {
                ClearWebFormData();
                SingleTable SingleTable = new SingleTable();

                int localid = Convert.ToInt32(DataRow["id"]);
                SingleRow row = this.GetRow(Form, Dt, DataRow, RowIndex);
                SingleTable.Add(row);
                WebFormData.MultipleTables.Add(Form.TableName, SingleTable);

                Form.UploadFiles(localid, WebFormData);

                this.GetLinesEnabledData(localid, Form, WebFormData);

                response = RestServices.Instance.Push(WebFormData, 0, Form.WebFormRefId, row.LocId);
                response.LocalRowId = localid;
            }
            catch (Exception ex)
            {
                Log.Write("SyncServices.PushRow---" + ex.Message);
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
                Log.Write(ex.Message);
            }
        }

        private void PushDependencyData(EbMobileForm SourceForm, EbMobileForm DependencyForm, int LiveId, int LocalId)
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

                        PushResponse resp = PushRow(DependencyForm, dt, dt.Rows[i], i);
                        if (resp.RowAffected <= 0)
                            continue;
                        DependencyForm.FlagLocalRow(resp, resp.LocalRowId);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write("SyncServices.PushDependencyData---" + ex.Message);
            }
        }

        private void FillLiveId(EbDataTable dt, EbDataRow dr, int liveId, string columName)
        {
            EbDataColumn col = dt.Columns[columName];

            if (col != null)
                dr[col.ColumnIndex] = liveId;
        }

        private void ClearWebFormData()
        {
            WebFormData.MultipleTables.Clear();
            WebFormData.ExtendedTables.Clear();
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
    }
}
