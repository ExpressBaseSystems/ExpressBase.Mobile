using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressBase.Mobile.Data
{
    public class StaticQueries
    {
        public static string TABLE_EXIST
        {
            get
            {
                return "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = '{0}'";
            }
        }

        public static string COL_SCHEMA
        {
            get
            {
                return "SELECT * FROM {0} LIMIT 1";
            }
        }


        public static string CREATE_TABLE
        {
            get
            {
                return "CREATE TABLE IF NOT EXISTS {0} ({1});";
            }
        }

        public static string ALTER_TABLE
        {
            get
            {
                return "ALTER TABLE {0} ADD COLUMN {1};";
            }
        }
    }
}
