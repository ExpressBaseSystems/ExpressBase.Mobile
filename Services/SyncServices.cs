using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExpressBase.Mobile.Services
{
    public class SyncServices
    {
        private static SyncServices instance;

        public static SyncServices Instance => instance ?? (instance = new SyncServices());

        public string Message { set; get; }

        private List<EbMobileForm> FormCollection;

        private WebformData WebFormData;

        public bool Sync()
        {
            try
            {
                FormCollection = GetForms();
                WebFormData = new WebformData();
                bool status = InitPush();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return true;
        }

        private List<EbMobileForm> GetForms()
        {
            List<EbMobileForm> ls = new List<EbMobileForm>();
            var pages = Settings.Objects;

            foreach (MobilePagesWraper _p in pages)
            {
                EbMobilePage mpage = _p.ToPage();
                if (mpage != null && mpage.Container.GetType() == typeof(EbMobileForm))
                {
                    ls.Add(mpage.Container as EbMobileForm);
                }
            }

            return ls;
        }

        private List<string> DependencyTables;

        private bool InitPush()
        {
            DependencyTables = new List<string>();

            foreach (EbMobileForm Form in this.FormCollection)
            {
                if (DependencyTables.Contains(Form.TableName))
                    continue;

                EbMobileForm DependencyForm = ResolveDependency(Form);

                if (DependencyForm != null)
                    DependencyTables.Add(DependencyForm.TableName);

                EbDataTable SourceData = Form.GetFormData();

                for (int i = 0; i < SourceData.Rows.Count; i++)
                {
                    PushResponse resp = PushRow(Form, SourceData, SourceData.Rows[i], i);

                    if (resp.RowAffected <= 0)
                        continue;

                    Form.FlagLocalRow(resp, resp.LocalRowId, Form.TableName);

                    if (DependencyForm != null)
                    {
                        PushDependencyData(Form, DependencyForm, resp.RowId, resp.LocalRowId);
                    }
                }
            }
            return true;
        }

        private PushResponse PushRow(EbMobileForm Form, EbDataTable Dt, EbDataRow DataRow, int RowIndex)
        {
            PushResponse response = null;
            try
            {
                ClearWebFormData();
                SingleTable SingleTable = new SingleTable();

                int localid = Convert.ToInt32(DataRow["id"]);
                SingleRow row = GetRow(Form, Dt, DataRow, RowIndex);
                SingleTable.Add(row);
                WebFormData.MultipleTables.Add(Form.TableName, SingleTable);

                Form.UploadFiles(localid, WebFormData);

                response = Api.Push(WebFormData, 0, Form.WebFormRefId, row.LocId);
                response.LocalRowId = localid;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return response;
        }

        private void PushDependencyData(EbMobileForm SourceForm, EbMobileForm DependencyForm, int LiveId, int LocalId)
        {
            try
            {
                string RefColumn = $"{SourceForm.TableName}_id";

                string query = string.Format(StaticQueries.STARFROM_TABLE_WDEP,
                    DependencyForm.SelectQuery,
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
                        DependencyForm.FlagLocalRow(resp, resp.LocalRowId, SourceForm.TableName);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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

        private EbMobileForm ResolveDependency(EbMobileForm SourceForm)
        {
            if (string.IsNullOrEmpty(SourceForm.AutoGenMVRefid))
                return null;

            var autogenvis = HelperFunctions.GetPage(SourceForm.AutoGenMVRefid);

            if (autogenvis == null)
                return null;

            string linkref = (autogenvis.Container as EbMobileVisualization).LinkRefId;

            if (string.IsNullOrEmpty(linkref))
                return null;

            var linkpage = HelperFunctions.GetPage(linkref);

            if (linkpage == null)
                return null;

            if (linkpage.Container is EbMobileVisualization)
            {
                if (string.IsNullOrEmpty((linkpage.Container as EbMobileVisualization).LinkRefId))
                    return null;

                var innerlink = HelperFunctions.GetPage((linkpage.Container as EbMobileVisualization).LinkRefId);

                if (innerlink.Container is EbMobileForm)
                {
                    return (innerlink.Container as EbMobileForm);
                }
            }

            return null;
        }
    }
}
