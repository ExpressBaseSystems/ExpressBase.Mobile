using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Structures;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpressBase.Mobile
{
    public class EbMobileVisualization : EbMobileContainer
    {
        #region Properties

        public string DataSourceRefId { set; get; }

        public string SourceFormRefId { set; get; }

        public string LinkRefId { get; set; }

        public int PageLength { set; get; } = 30;

        public WebFormDVModes FormMode { set; get; }

        public EbMobileDataColToControlMap FormId { set; get; }

        public EbScript OfflineQuery { set; get; }

        public EbMobileTableLayout DataLayout { set; get; }

        public List<Param> DataSourceParams { get; set; }

        public List<EbMobileControl> FilterControls { set; get; }

        public List<EbMobileDataColumn> SortColumns { set; get; }

        public List<EbMobileDataColToControlMap> LinkFormParameters { get; set; }

        public List<EbCTCMapper> ContextToControlMap { set; get; }

        public bool ShowNewButton { set; get; }

        public bool ShowLinkIcon { set; get; }

        public bool EnableAlternateRowColoring { set; get; }

        public bool ShowRowSeperator { set; get; }

        public EbThickness Margin { set; get; }

        public EbThickness Padding { set; get; }

        public int RowSpacing { set; get; }

        public int ColumnSpacing { set; get; }

        public int BorderRadius { set; get; }

        public string BorderColor { set; get; }

        public string BackgroundColor { set; get; }

        public bool BoxShadow { set; get; }

        #endregion

        //mobile property
        public string GetQuery { get { return HelperFunctions.B64ToString(this.OfflineQuery.Code); } }

        public EbMobileVisualization()
        {
            OfflineQuery = new EbScript();
            DataSourceParams = new List<Param>();
            FilterControls = new List<EbMobileControl>();
            SortColumns = new List<EbMobileDataColumn>();
            LinkFormParameters = new List<EbMobileDataColToControlMap>();
        }

        #region Methods

        public bool HasLink()
        {
            return !string.IsNullOrEmpty(LinkRefId);
        }

        public bool HasSourceFormLink()
        {
            return !string.IsNullOrEmpty(SourceFormRefId);
        }

        public async Task<EbDataSet> GetData(NetworkMode networkType, int offset, List<DbParameter> parameters = null, List<SortColumn> sortOrder = null)
        {
            if (networkType == NetworkMode.Online)
            {
                return await this.GetLiveData(parameters, sortOrder, offset);
            }
            else
            {
                return this.GetLocalData(parameters, sortOrder, offset);
            }
        }

        public EbDataSet GetLocalData(List<DbParameter> dbParameters, List<SortColumn> sortOrder, int offset)
        {
            EbDataSet Data = null;
            try
            {
                dbParameters ??= new List<DbParameter>();

                DbParameter userParam = dbParameters.Find(item => item.ParameterName == "current_userid");

                if (userParam != null)
                {
                    userParam.Value = App.Settings.UserId;
                    userParam.DbType = (int)EbDbTypes.Int32;
                }

                string sql = HelperFunctions.WrapSelectQuery(GetQuery, dbParameters);

                int len = this.PageLength == 0 ? 30 : this.PageLength;

                dbParameters.Add(new DbParameter { ParameterName = "@limit", Value = len, DbType = (int)EbDbTypes.Int32 });
                dbParameters.Add(new DbParameter { ParameterName = "@offset", Value = offset, DbType = (int)EbDbTypes.Int32 });

                Data = App.DataDB.DoQueries(sql, dbParameters.ToArray());
            }
            catch (Exception ex)
            {
                EbLog.Error("Failed to get local data");
                EbLog.Error(ex.Message);
            }
            return Data;
        }

        public async Task<EbDataSet> GetLiveData(List<DbParameter> dbParameters, List<SortColumn> sortOrder, int offset)
        {
            EbDataSet Data = null;

            int len = this.PageLength == 0 ? 30 : this.PageLength;

            dbParameters ??= new List<DbParameter>();

            if (App.Settings.CurrentLocation != null)
            {
                dbParameters.Add(new DbParameter
                {
                    ParameterName = "eb_loc_id",
                    DbType = 11,
                    Value = App.Settings.CurrentLocation.LocId
                });
            }

            List<Param> paramsArray = dbParameters.ToParams();

            try
            {
                VisualizationLiveData vd = await DataService.Instance.GetDataAsync(this.DataSourceRefId, paramsArray, sortOrder, len, offset);
                Data = vd?.Data;
            }
            catch (Exception ex)
            {
                EbLog.Message($"Failed to get Live data for '{DataSourceRefId}'");
                EbLog.Error(ex.Message);
            }

            return Data;
        }

        public List<DbParameter> GetContextParams(EbDataRow row, NetworkMode network)
        {
            return network == NetworkMode.Online ? this.GetParamLive(row) : this.GetParamLocal(row);
        }

        private List<DbParameter> GetParamLocal(EbDataRow row)
        {
            var parameters = new List<DbParameter>();
            try
            {
                string sql = HelperFunctions.B64ToString(this.OfflineQuery.Code);
                List<string> _parameters = HelperFunctions.GetSqlParams(sql);

                foreach (string param in _parameters)
                {
                    object data = row[param];

                    if (data != null)
                    {
                        parameters.Add(new DbParameter
                        {
                            ParameterName = param,
                            Value = data
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                EbLog.Message("Visualization get parameters from local error");
                EbLog.Error(ex.Message);
            }
            return parameters;
        }

        private List<DbParameter> GetParamLive(EbDataRow row)
        {
            var parameters = new List<DbParameter>();

            foreach (Param param in this.DataSourceParams)
            {
                object data = row[param.Name];

                if (data != null)
                {
                    parameters.Add(new DbParameter
                    {
                        ParameterName = param.Name,
                        DbType = Convert.ToInt32(param.Type),
                        Value = data
                    });
                }
            }
            return parameters;
        }

        #endregion
    }
}
