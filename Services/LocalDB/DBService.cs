using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using System;
using System.Collections.Generic;

namespace ExpressBase.Mobile.Services.LocalDB
{
    public class DBService : IDBService
    {
        private static DBService instance;

        public static DBService Current => instance ??= new DBService();

        private DBService() { }

        public void ImportData(EbDataSet dataSet)
        {
            EbLog.Info("Importing Data to local DB...");

            if (dataSet?.Tables.Count > 0)
            {
                EbLog.Info($"Importing {dataSet?.Tables.Count} Tables");

                foreach (EbDataTable dt in dataSet.Tables)
                {
                    EbLog.Info($"Importing Tables {dt.TableName} with {dt.Rows.Count} records");

                    List<SQLiteColumSchema> ColSchema = new List<SQLiteColumSchema>();

                    foreach (EbDataColumn col in dt.Columns)
                    {
                        ColSchema.Add(new SQLiteColumSchema
                        {
                            ColumnName = col.ColumnName,
                            ColumnType = SQLiteTableSchema.SQLiteType(col.Type)
                        });
                    }

                    DropTable(dt.TableName);

                    EbLog.Info($"{dt.TableName} droped.");

                    CreateTable(dt.TableName, ColSchema);

                    EbLog.Info($"{dt.TableName} created.");

                    App.DataDB.DoNonQueryBatch(dt);

                    EbLog.Info($"Importing Tables {dt.TableName} complete.");
                }
            }
        }

        private void DropTable(string tableName)
        {
            try
            {
                App.DataDB.DoNonQuery($"DROP TABLE IF EXISTS {tableName};");
            }
            catch (Exception ex)
            {
                EbLog.Error($"Failed to drop Table {tableName} : " + ex.Message);
            }
        }

        private void CreateTable(string tableName, List<SQLiteColumSchema> columns)
        {
            try
            {
                List<string> name_type = new List<string>();

                foreach (SQLiteColumSchema column in columns)
                {
                    name_type.Add(string.Format("{0} {1}", column.ColumnName, column.ColumnType));
                }
                string create_query = string.Format(StaticQueries.CREATE_TABLE, tableName, string.Join(",", name_type.ToArray()));

                int status = App.DataDB.DoNonQuery(create_query);
            }
            catch (Exception e)
            {
                EbLog.Error($"Failed to create Table {tableName}" + e.Message);
            }
        }

        public void CreateTables(SQLiteTableSchemaList SQLSchemaList)
        {
            try
            {
                foreach (SQLiteTableSchema SQLSchema in SQLSchemaList)
                {
                    var tableExist = App.DataDB.DoScalar(string.Format(StaticQueries.TABLE_EXIST, SQLSchema.TableName));

                    if (Convert.ToInt32(tableExist) > 0)
                    {
                        EbDataTable dt = App.DataDB.DoQuery(string.Format(StaticQueries.COL_SCHEMA, SQLSchema.TableName));

                        List<SQLiteColumSchema> unCreated = this.GetNewControls(SQLSchema.Columns, dt.Columns);

                        if (unCreated.Count > 0)
                        {
                            AlterTable(SQLSchema.TableName, unCreated);
                        }
                    }
                    else
                    {
                        CreateTable(SQLSchema.TableName, SQLSchema.Columns);
                    }
                }
            }
            catch (Exception e)
            {
                EbLog.Error($"Failed to create tables" + e.Message);
            }
        }

        private void AlterTable(string tableName, List<SQLiteColumSchema> columns)
        {
            try
            {
                List<string> name_type = new List<string>();

                foreach (SQLiteColumSchema schema in columns)
                {
                    name_type.Add(string.Format("{0} {1}", schema.ColumnName, schema.ColumnType));
                }
                string alter_query = string.Empty;
                foreach (string str in name_type)
                    alter_query += string.Format(StaticQueries.ALTER_TABLE, tableName, str);

                int status = App.DataDB.DoNonQuery(alter_query);
            }
            catch (Exception e)
            {
                EbLog.Error($"Failed to Alter table {tableName}" + e.Message);
            }
        }

        private List<SQLiteColumSchema> GetNewControls(List<SQLiteColumSchema> columns, ColumnColletion newColumns)
        {
            List<SQLiteColumSchema> unCreated = new List<SQLiteColumSchema>();

            foreach (SQLiteColumSchema cols in columns)
            {
                EbDataColumn data_col = newColumns.Find(x => x.ColumnName == cols.ColumnName);

                if (data_col == null)
                {
                    unCreated.Add(cols);
                }
            }
            return unCreated;
        }
    }
}
