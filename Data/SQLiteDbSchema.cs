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
            this.Columns.Add(new SQLiteColumSchema { ColumnName = "eb_synced", ColumnType = "INT" });
            this.Columns.Add(new SQLiteColumSchema { ColumnName = "eb_synced_at", ColumnType = "INT" });
        }
    }

    public class SQLiteColumSchema
    {
        public string ColumnName { set; get; }

        public string ColumnType { set; get; }
    }
}
