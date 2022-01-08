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

        public static string SQLiteType(EbDbTypes EbDbType)
        {
            if (EbDbType == EbDbTypes.String)
                return "TEXT";
            else if (EbDbType == EbDbTypes.Int16 || EbDbType == EbDbTypes.Int32)
                return "INT";
            else if (EbDbType == EbDbTypes.Decimal || EbDbType == EbDbTypes.Double)
                return "REAL";
            else if (EbDbType == EbDbTypes.Date || EbDbType == EbDbTypes.DateTime)
                return "DATETIME";
            else if (EbDbType == EbDbTypes.Boolean || EbDbType == EbDbTypes.BooleanOriginal)
                return "INT";
            else
                return "TEXT";
        }

        public static object SQLiteTypeValue(EbDbTypes EbDbType, object Value)
        {
            if (EbDbType == EbDbTypes.String)
                return Value?.ToString();
            else if (EbDbType == EbDbTypes.Int16 || EbDbType == EbDbTypes.Int32)
            {
                int.TryParse(Value?.ToString(), out int i);
                return i;
            }
            else if (EbDbType == EbDbTypes.Decimal || EbDbType == EbDbTypes.Double)
            {
                double.TryParse(Value?.ToString(), out double i);
                return i;
            }
            else if (EbDbType == EbDbTypes.Boolean || EbDbType == EbDbTypes.BooleanOriginal)
            {
                bool.TryParse(Value?.ToString(), out bool i);
                return i ? 1 : 0;
            }
            else
                return Value;
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
