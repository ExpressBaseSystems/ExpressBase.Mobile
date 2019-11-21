using SQLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace ExpressBase.Mobile.Data
{
    public class EbDataBase
    {
        readonly SQLiteConnection _database;

        public EbDataBase(string dbPath)
        {
            _database = new SQLiteConnection(dbPath);
        }

        public int DoNonQuery(string query,params object[] parameters)
        {
            try
            {
                return _database.Execute(query, parameters);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return 0;
        }

        public void DoQuery(string query)
        {

        }
    }
}
