using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbMobileTableView : EbMobileDashBoardControl
    {
        public string DataSourceRefId { set; get; }

        public EbScript OfflineQuery { set; get; }

        public string BindingTable { set; get; }

        private EbDataTable Data { set; get; }

        private EbDataRow LinkedDataRow { set; get; }

        private void SetData()
        {
            try
            {
                string sql = HelperFunctions.WrapSelectQueryUnPaged(HelperFunctions.B64ToString(this.OfflineQuery.Code));
                List<DbParameter> dbParams = new List<DbParameter>();
                List<string> parameters = HelperFunctions.GetSqlParams(sql);
                if (parameters.Count > 0)
                    this.GetParameterValues(dbParams, parameters);
                Data = App.DataDB.DoQuery(sql, dbParams.ToArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void GetParameterValues(List<DbParameter> dbParams, List<string> parameters)
        {
            try
            {
                foreach (string param in parameters)
                {
                    dbParams.Add(new DbParameter
                    {
                        ParameterName = param,
                        Value = this.LinkedDataRow[param],
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
