using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressBase.Mobile.Data
{
    public class SQLiteTableSchemaList : List<SQLiteTableSchema>
    {

    }

    public class SQLiteTableSchema
    {
        public string TableName { set; get; }

        public List<SQLiteColumSchema> Columns { set; get; }

        public SQLiteTableSchema()
        {
            Columns = new List<SQLiteColumSchema>();
        }

        public void AppendDefault()
        {
            this.Columns.Add(new SQLiteColumSchema { ColumnName = "eb_created_by", ColumnType = "INT" });
            this.Columns.Add(new SQLiteColumSchema { ColumnName = "eb_created_at", ColumnType = "DATETIME" });
            this.Columns.Add(new SQLiteColumSchema { ColumnName = "eb_lastmodified_by", ColumnType = "INT" });
            this.Columns.Add(new SQLiteColumSchema { ColumnName = "eb_lastmodified_at", ColumnType = "DATETIME" });
            this.Columns.Add(new SQLiteColumSchema { ColumnName = "eb_del", ColumnType = "INT" });
            this.Columns.Add(new SQLiteColumSchema { ColumnName = "eb_void", ColumnType = "INT" });
            this.Columns.Add(new SQLiteColumSchema { ColumnName = "eb_loc_id", ColumnType = "INT" });
            this.Columns.Add(new SQLiteColumSchema { ColumnName = "eb_device_id", ColumnType = "TEXT" });
            this.Columns.Add(new SQLiteColumSchema { ColumnName = "eb_appversion", ColumnType = "TEXT" });
            this.Columns.Add(new SQLiteColumSchema { ColumnName = "eb_synced", ColumnType = "INT" });//only in mobile
            this.Columns.Add(new SQLiteColumSchema { ColumnName = "eb_created_at_device", ColumnType = "DATETIME" });
            this.Columns.Add(new SQLiteColumSchema { ColumnName = "eb_syncrecord_id", ColumnType = "INT" });
        }

        public static List<string> LocalColumsOnly
        {
            get
            {
                return new List<string>
                {
                    "eb_created_by",
                    "eb_created_at",
                    "eb_lastmodified_by",
                    "eb_lastmodified_at",
                    "eb_del",
                    "eb_void",
                    "eb_loc_id",
                    "eb_synced",
                    "eb_syncrecord_id"
                };
            }
        }
    }

    public class SQLiteColumSchema
    {
        public string ColumnName { set; get; }

        public string ColumnType { set; get; }
    }
}
