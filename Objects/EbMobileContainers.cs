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
        public NetworkMode NetworkType { set; get; }
    }

    public class EbMobileVisualization : EbMobileContainer
    {
        public string DataSourceRefId { set; get; }

        public List<Param> DataSourceParams { get; set; }

        public string SourceFormRefId { set; get; }

        public EbScript OfflineQuery { set; get; }

        public EbMobileTableLayout DataLayout { set; get; }

        public string LinkRefId { get; set; }

        public List<EbMobileDataColumn> Filters { set; get; }

        public WebFormDVModes FormMode { set; get; }

        public int PageLength { set; get; } = 30;

        //mobile property
        public string GetQuery { get { return HelperFunctions.B64ToString(this.OfflineQuery.Code); } }

        public EbDataSet GetData(NetworkMode networkType, int offset = 0, List<DbParameter> parameters = null)
        {
            EbDataSet ds = null;
            try
            {
                if (networkType == NetworkMode.Online)
                {
                    if (parameters == null)
                        ds = this.GetLiveData(offset);
                    else
                        ds = this.GetLiveData(parameters, offset);
                }
                else if (networkType == NetworkMode.Offline)
                {
                    var sqlParams = HelperFunctions.GetSqlParams(this.GetQuery);
                    if (sqlParams.Count > 0)
                    {
                        List<DbParameter> dbParams = new List<DbParameter>();
                        foreach (string s in sqlParams)
                            dbParams.Add(new DbParameter { ParameterName = s });

                        ds = this.GetLocalData(dbParams, offset);
                    }
                    else
                        ds = this.GetLocalData(offset);
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
            return ds;
        }

        public EbDataSet GetLocalData(int offset = 0)
        {
            EbDataSet Data = new EbDataSet();
            try
            {
                string sql = HelperFunctions.WrapSelectQuery(GetQuery);

                DbParameter[] dbParameters = {
                    new DbParameter{ ParameterName = "@limit", Value = PageLength, DbType = (int)EbDbTypes.Int32 },
                    new DbParameter{ ParameterName = "@offset", Value = offset, DbType = (int)EbDbTypes.Int32 },
                };

                Data = App.DataDB.DoQueries(sql, dbParameters);
            }
            catch (Exception ex)
            {
                Log.Write("EbMobileVisualization.GetData---" + ex.Message);
            }
            return Data;
        }

        public EbDataSet GetLocalData(List<DbParameter> dbParameters, int offset = 0)
        {
            EbDataSet Data = new EbDataSet();
            try
            {
                var userParam = dbParameters.Find(item => item.ParameterName == "current_userid");

                if (userParam != null)
                {
                    userParam.Value = Settings.UserId;
                    userParam.DbType = (int)EbDbTypes.Int32;
                }

                string sql = HelperFunctions.WrapSelectQuery(GetQuery, dbParameters);

                dbParameters.Add(new DbParameter { ParameterName = "@limit", Value = PageLength, DbType = (int)EbDbTypes.Int32 });
                dbParameters.Add(new DbParameter { ParameterName = "@offset", Value = offset, DbType = (int)EbDbTypes.Int32 });

                Data = App.DataDB.DoQueries(sql, dbParameters.ToArray());
            }
            catch (Exception ex)
            {
                Log.Write("EbMobileVisualization.GetData with params---" + ex.Message);
            }
            return Data;
        }

        public EbDataSet GetLiveData(int offset = 0)
        {
            EbDataSet Data = null;
            try
            {
                Auth.AuthIfTokenExpired();
                VisualizationLiveData vd = RestServices.Instance.PullReaderData(this.DataSourceRefId, null, this.PageLength, offset);
                Data = vd.Data;
            }
            catch (Exception ex)
            {
                Log.Write("EbMobileVisualization.GetLiveData---" + ex.Message);
            }
            return Data;
        }

        public EbDataSet GetLiveData(List<DbParameter> dbParameters, int offset = 0)
        {
            EbDataSet Data = null;
            try
            {
                Auth.AuthIfTokenExpired();
                VisualizationLiveData vd = RestServices.Instance.PullReaderData(this.DataSourceRefId, dbParameters.ToParams(), this.PageLength, offset);
                Data = vd.Data;
            }
            catch (Exception ex)
            {
                Log.Write("EbMobileVisualization.GetLiveData with params---" + ex.Message);
            }
            return Data;
        }

        public EbMobileVisualization()
        {
            OfflineQuery = new EbScript();
            Filters = new List<EbMobileDataColumn>();
            DataSourceParams = new List<Param>();
        }
    }

    public class EbMobileDashBoard : EbMobileContainer
    {
        public List<EbMobileDashBoardControls> ChildControls { get; set; }

        public EbMobileDashBoard()
        {
            ChildControls = new List<EbMobileDashBoardControls>();
        }
    }

    public class EbMobilePdf : EbMobileContainer
    {
        public string Template { set; get; }

        public EbScript OfflineQuery { set; get; }

        public EbMobilePdf()
        {
            OfflineQuery = new EbScript();
        }
    }
}
