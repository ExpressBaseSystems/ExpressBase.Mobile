using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Structures;
using System;
using System.Collections.Generic;

namespace ExpressBase.Mobile
{
    public class DbTypedValue
    {
        public EbDbTypes Type { set; get; }

        private object _value;

        public object Value
        {
            set { _value = value; }
            get
            {
                if (Type == EbDbTypes.DateTime)
                    return Convert.ToDateTime(_value).ToString("yyyy-MM-dd");
                else if (Type == EbDbTypes.Date)
                    return Convert.ToDateTime(_value).ToString("yyyy-MM-dd");
                else
                    return _value;
            }
        }

        public DbTypedValue() { }

        public DbTypedValue(string Name, object Value, EbDbTypes Type)
        {
            if (Name == "eb_created_at_device")
                this.Type = EbDbTypes.DateTime;
            else
            {
                this.Type = Type;
                this.Value = Value;
            }
        }
    }

    public class EbMobileContainer : EbMobilePageBase
    {

    }

    public class EbMobileVisualization : EbMobileContainer
    {
        public string DataSourceRefId { set; get; }

        public string SourceFormRefId { set; get; }

        public EbScript OfflineQuery { set; get; }

        public EbMobileTableLayout DataLayout { set; get; }

        public string LinkRefId { get; set; }

        public List<EbMobileDataColumn> Filters { set; get; }

        public WebFormDVModes FormMode { set; get; }

        //mobile property
        public string GetQuery { get { return HelperFunctions.B64ToString(this.OfflineQuery.Code); } }

        public EbDataTable GetData()
        {
            EbDataTable Data = new EbDataTable();
            try
            {
                string sql = HelperFunctions.WrapSelectQuery(GetQuery);
                Data = App.DataDB.DoQuery(sql);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return Data;
        }

        public EbDataTable GetData(List<DbParameter> dbParameters)
        {
            EbDataTable Data = new EbDataTable();
            try
            {
                var userParam = dbParameters.Find(item => item.ParameterName == "current_userid");

                if(userParam != null)
                {
                    userParam.Value = Settings.UserId;
                    userParam.DbType = (int)EbDbTypes.Int32;
                }

                string sql = HelperFunctions.WrapSelectQuery(GetQuery, dbParameters);
                Data = App.DataDB.DoQuery(sql, dbParameters.ToArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return Data;
        }
    }

    public class EbMobileDashBoard : EbMobileContainer
    {
        public List<EbMobileDashBoardControls> ChiledControls { set; get; }

        public List<EbMobileDashBoardControls> ChildControls { get { return ChiledControls; } set { } }
    }
}
