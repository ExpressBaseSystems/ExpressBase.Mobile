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

        public static string GET_ALL_TABLE
        {
            get
            {
                return "SELECT name FROM sqlite_master WHERE type = 'table' ORDER BY 1";
            }
        }

        public static string STARFROM_TABLE
        {
            get
            {
                return "SELECT {0} FROM {1} WHERE eb_synced = 0";
            }
        }

        public static string STARFROM_TABLE_WDEP
        {
            get
            {
                return "SELECT {0} FROM {1} WHERE eb_synced = 0 AND {2} = {3}";
            }
        }


        public static string DELETE_FROM_TABLE
        {
            get
            {
                return "DELETE * FROM {0}";
            }
        }

        public static string FLAG_LOCALROW_SYNCED
        {
            get
            {
                return "UPDATE {0} SET eb_synced = 1,eb_syncrecord_id = @cloudrowid WHERE id = @rowid";
            }
        }

        public static string CURRVAL
        {
            get
            {
                return "SELECT id from {0} order by id DESC limit 1;";
            }
        }
    }
}
