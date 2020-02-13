using ExpressBase.Mobile.Structures;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace ExpressBase.Mobile.Data
{
    public interface IDataBase
    {
        int CreateDB(string sid);

        EbDataSet DoQueries(string query, params DbParameter[] parameter);

        EbDataTable DoQuery(string query, params DbParameter[] parameter);

        int DoNonQuery(string query, params DbParameter[] parameter);

        void DoNonQueryBatch(EbDataTable Table);

        object DoScalar(string query, params DbParameter[] parameter);
    }

    public class DbParameter
    {
        public int DbType { set; get; }

        public string ParameterName { set; get; }

        public object Value { set ; get; }
    }

    public class DbTypeConverter
    {
        public static EbDbTypes ConvertToDbType(Type _typ)
        {
            if (_typ == typeof(DateTime))
                return EbDbTypes.Date;
            else if (_typ == typeof(string))
                return EbDbTypes.String;
            else if (_typ == typeof(bool))
                return EbDbTypes.Boolean;
            else if (_typ == typeof(decimal) || _typ == typeof(Double) || _typ == typeof(Single))
                return EbDbTypes.Decimal;
            else if (_typ == typeof(int) || _typ == typeof(Int32) || _typ == typeof(Int16))
                return EbDbTypes.Int32;
            else if (_typ == typeof(Int16))
                return EbDbTypes.Int16;
            else if (_typ == typeof(Int64))
                return EbDbTypes.Int64;
            else if (_typ == typeof(Single))
                return EbDbTypes.Int32;
            else if (_typ == typeof(TimeSpan))
                return EbDbTypes.Time;
            else if (_typ == typeof(Byte[]))
                return EbDbTypes.Bytea;
            return EbDbTypes.String;
        }
    }
}