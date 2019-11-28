using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace ExpressBase.Mobile.Data
{
    public interface IDataBase
    {
        int CreateDB(string sid);

        void DoQueries(string query, params DbParameter[] parameter);

        EbDataTable DoQuery(string query, params DbParameter[] parameter);

        int DoNonQuery(string query, params DbParameter[] parameter);
    }

    public class DbParameter
    {
        public int DbType { set; get; }

        public string ParameterName { set; get; }

        public object Value { set ; get; }
    }
}