using ExpressBase.Mobile.Structures;
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
            this.Columns.Add(new SQLiteColumSchema { ColumnName = "id", ColumnType = "INTEGER PRIMARY KEY AUTOINCREMENT" });
            this.Columns.Add(new SQLiteColumSchema { ColumnName = "eb_created_at_device", ColumnType = "DATETIME" });
            this.Columns.Add(new SQLiteColumSchema { ColumnName = "eb_del", ColumnType = "INT DEFAULT 0 NOT NULL" });
            this.Columns.Add(new SQLiteColumSchema { ColumnName = "eb_void", ColumnType = "INT DEFAULT 0 NOT NULL" });
            this.Columns.Add(new SQLiteColumSchema { ColumnName = "eb_loc_id", ColumnType = "INT" });
            this.Columns.Add(new SQLiteColumSchema { ColumnName = "eb_device_id", ColumnType = "TEXT" });
            this.Columns.Add(new SQLiteColumSchema { ColumnName = "eb_appversion", ColumnType = "TEXT" });
            this.Columns.Add(new SQLiteColumSchema { ColumnName = "eb_synced", ColumnType = "INT DEFAULT 0 NOT NULL" });//only in mobile
            this.Columns.Add(new SQLiteColumSchema { ColumnName = "eb_syncrecord_id", ColumnType = "INT" });
        }
    }

    public class SQLiteColumSchema
    {
        public string ColumnName { set; get; }

        public string ColumnType { set; get; }
    }
}
