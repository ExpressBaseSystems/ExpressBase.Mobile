using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using ExpressBase.Mobile.Helpers;
using System.Threading.Tasks;
using System.Linq;
using ExpressBase.Mobile.Enums;

namespace ExpressBase.Mobile.Services
{
    public class CommonServices
    {
        private static CommonServices instance;

        public static CommonServices Instance => instance ?? (instance = new CommonServices());

        public void CreateLocalTable(SQLiteTableSchemaList SQLSchemaList)
        {
            try
            {
                foreach (SQLiteTableSchema SQLSchema in SQLSchemaList)
                {
                    var tableExist = App.DataDB.DoScalar(string.Format(StaticQueries.TABLE_EXIST, SQLSchema.TableName));

                    if (Convert.ToInt32(tableExist) > 0)//table exist
                    {
                        EbDataTable dt = App.DataDB.DoQuery(string.Format(StaticQueries.COL_SCHEMA, SQLSchema.TableName));

                        List<SQLiteColumSchema> Uncreated = this.GetNewControls(SQLSchema.Columns, dt.Columns);

                        if (Uncreated.Count > 0)
                            this.AlterTable(SQLSchema.TableName, Uncreated);
                    }
                    else //table not exist
                        this.CreateTable(SQLSchema.TableName, SQLSchema.Columns);
                }
            }
            catch (Exception e)
            {
                EbLog.Write("CommonServices.CreateLocalTable---" + e.Message);
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
                EbLog.Write("CommonServices.CreateTable---" + e.Message);
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
                EbLog.Write("CommonServices.AlterTable---" + e.Message);
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

        public async Task<int> LoadLocalData(EbDataSet DS)
        {
            try
            {
                if (DS?.Tables.Count > 0)
                {
                    foreach (EbDataTable dt in DS.Tables)
                    {
                        List<SQLiteColumSchema> ColSchema = new List<SQLiteColumSchema>();

                        foreach (EbDataColumn col in dt.Columns)
                            ColSchema.Add(new SQLiteColumSchema { ColumnName = col.ColumnName, ColumnType = SQLiteTableSchema.SQLiteType(col.Type) });
                       
                        await DropTable(dt.TableName);//droping existing table

                        this.CreateTable(dt.TableName, ColSchema);

                        App.DataDB.DoNonQueryBatch(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                EbLog.Write("CommonServices.LoadLocalData---" + ex.Message);
            }
            return 0;
        }

        private async Task DropTable(string tableName)
        {
            try
            {
                await Task.Delay(1);
                App.DataDB.DoNonQuery($"DROP TABLE IF EXISTS {tableName};");
            }
            catch (Exception ex)
            {
                EbLog.Write("CommonServices.DropTable---" + ex.Message);
            }
        }
    }
}
