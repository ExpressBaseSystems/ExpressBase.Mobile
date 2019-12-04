using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Linq;

namespace ExpressBase.Mobile.Services
{
    public class CommonServices
    {
        public static void PushWebFormData(string Form, string RefId, int Locid, int RowId)
        {
            string uri = Settings.RootUrl + "api/webform_save";
            //WebFormSaveResponse Response = null;
            try
            {
                RestClient client = new RestClient(uri);
                RestRequest request = new RestRequest(Method.POST);

                request.AddHeader(AppConst.BTOKEN, Store.GetValue(AppConst.BTOKEN));
                request.AddHeader(AppConst.RTOKEN, Store.GetValue(AppConst.RTOKEN));

                request.AddParameter("webform_data", Form);
                request.AddParameter("refid", RefId);
                request.AddParameter("locid", Locid);
                request.AddParameter("rowid", RowId);

                var resp = client.Execute(request);
                //Response =  JsonConvert.DeserializeObject<WebFormSaveResponse>(resp.Content);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                // Response = new WebFormSaveResponse();
            }
            //return Response;
        }

        public void CreateLocalTable4Form(SQLiteTableSchema SQLSchema)
        {
            try
            {
                var tableExist = App.DataDB.DoScalar(string.Format(StaticQueries.TABLE_EXIST, SQLSchema.TableName));

                if (Convert.ToInt32(tableExist) > 0)//table exist
                {
                    EbDataTable dt = App.DataDB.DoQuery(string.Format(StaticQueries.COL_SCHEMA, SQLSchema.TableName));

                    List<SQLiteColumSchema> Uncreated = this.GetNewControls(SQLSchema.Columns, dt.Columns);

                    if (Uncreated.Count > 0)
                    {
                        this.AlterTable(SQLSchema.TableName, Uncreated);
                    }
                }
                else //table not exist
                {
                    this.CreateTable(SQLSchema.TableName, SQLSchema.Columns);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void CreateTable(string TableName, List<SQLiteColumSchema> Columns)
        {
            try
            {
                List<string> name_type = new List<string>();

                foreach (SQLiteColumSchema column in Columns)
                {
                    name_type.Add(string.Format("{0} {1}", column.ColumnName, column.ColumnType));
                }
                string create_query = string.Format(StaticQueries.CREATE_TABLE, TableName, string.Join(",", name_type.ToArray()));

                int status = App.DataDB.DoNonQuery(create_query);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void AlterTable(string TableName, List<SQLiteColumSchema> Columns)
        {
            try
            {
                List<string> name_type = new List<string>();

                foreach (SQLiteColumSchema column in Columns)
                {
                    name_type.Add(string.Format("{0} {1}", column.ColumnName, column.ColumnType));
                }
                string alter_query = string.Format(StaticQueries.ALTER_TABLE, TableName, string.Join(",", name_type.ToArray()));

                int status = App.DataDB.DoNonQuery(alter_query);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private List<SQLiteColumSchema> GetNewControls(List<SQLiteColumSchema> Columns, ColumnColletion NewCols)
        {
            List<SQLiteColumSchema> UnCreated = new List<SQLiteColumSchema>();

            foreach (SQLiteColumSchema cols in Columns)
            {
                EbDataColumn data_col = NewCols.Find(x => x.ColumnName == cols.ColumnName);
                if (data_col == null)
                    UnCreated.Add(cols);
            }

            return UnCreated;
        }

        public SyncResponse SyncDevice()
        {
            try
            {
                MobilePageCollection Collection = new MobilePageCollection
                {
                    Pages = JsonConvert.DeserializeObject<List<MobilePagesWraper>>(Store.GetValue(AppConst.OBJ_COLLECTION))
                };

                List<EbMobilePage> FormCollection = Collection.GetForms();

                foreach (EbMobilePage page in FormCollection)
                {
                    EbDataTable dt = App.DataDB.DoQuery(string.Format(StaticQueries.STARFROM_TABLE, (page.Container as EbMobileForm).TableName));
                    if (dt.Rows.Count <= 0)
                        continue;
                    this.PushRecord(page, dt);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        private void PushRecord(EbMobilePage page, EbDataTable dt)
        {
            EbMobileForm Form = page.Container as EbMobileForm;
            WebformData FormData = new WebformData { MasterTable = Form.TableName };
            try
            {
                SingleTable master_table = new SingleTable();

                SingleRow _srow = new SingleRow { RowId = "0", IsUpdate = false };
                master_table.Add(_srow);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    _srow.Columns.Clear();
                    FormData.MultipleTables.Clear();

                    _srow.LocId = Convert.ToInt32(dt.Rows[i]["eb_loc_id"]);
                    _srow.Columns.AddRange(this.GetDataColums(dt, i, Form));
                    FormData.MultipleTables.Add(Form.TableName, master_table);

                    SyncResponse response = this.Push(JsonConvert.SerializeObject(FormData), 0, Form.WebFormRefId, _srow.LocId);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private SyncResponse Push(string webform, int rowid, string refid, int locid)
        {
            try
            {
                string url = Settings.RootUrl + "api/push_data";
                RestClient client = new RestClient(url);

                RestRequest request = new RestRequest(Method.POST);
                request.AddParameter("webform_data", webform);
                request.AddParameter("rowid", rowid);
                request.AddParameter("refid", refid);
                request.AddParameter("locid", locid);

                // auth Headers for api
                request.AddHeader(AppConst.BTOKEN, Store.GetValue(AppConst.BTOKEN));
                request.AddHeader(AppConst.RTOKEN, Store.GetValue(AppConst.RTOKEN));

                IRestResponse response = client.Execute(request);
                return JsonConvert.DeserializeObject<SyncResponse>(response.Content);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return null;
        }

        private List<SingleColumn> GetDataColums(EbDataTable dt, int rowindex, EbMobileForm form)
        {
            List<SingleColumn> SC = new List<SingleColumn>();

            for (int i = 0; i < dt.Rows[rowindex].Count; i++)
            {
                EbDataColumn column = dt.Columns.Find(o => o.ColumnIndex == i);

                if (!SQLiteTableSchema.LocalColumsOnly.Contains(column.ColumnName))
                {
                    DbTypedValue DTV = form.GetDbType(column.ColumnName, dt.Rows[rowindex][i]);
                    SC.Add(new SingleColumn
                    {
                        Name = column.ColumnName,
                        Type = (int)DTV.Type,
                        Value = DTV.Value
                    });
                }
            }
            return SC;
        }
    }
}
