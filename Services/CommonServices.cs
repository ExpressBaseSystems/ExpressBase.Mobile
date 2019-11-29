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

                if (Convert.ToInt32(tableExist) >= 0)//table exist
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
    }
}
