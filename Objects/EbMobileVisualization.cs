using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

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

        public List<EbMobileDataColumn> SearchColumns { set; get; }

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

        public bool HideContext { set; get; }

        #endregion

        public string GetQuery => HelperFunctions.B64ToString(this.OfflineQuery.Code);

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

        public SeparatorVisibility XFSeperator()
        {
            return ShowRowSeperator ? SeparatorVisibility.Default : SeparatorVisibility.None;
        }

        public bool IsSearchable()
        {
            return SearchColumns != null && SearchColumns.Any();
        }

        public async Task<EbDataSet> GetData(NetworkMode network, int offset, List<DbParameter> dbparam = null, List<SortColumn> sort = null, List<Param> search = null)
        {
            EbDataSet ds = null;

            if (network == NetworkMode.Online)
            {
                try
                {
                    dbparam ??= new List<DbParameter>();

                    List<Param> param = dbparam.ToParams();

                    int limit = PageLength == 0 ? 30 : PageLength;

                    ds = await this.GetLiveData(limit, offset, param, sort, search);
                }
                catch (Exception ex)
                {
                    EbLog.Error("Error at visualization getdata();");
                    EbLog.Error(ex.Message);
                }
            }
            else
            {
                ds = this.GetLocalData(dbparam, sort, offset);
            }
            return ds ?? new EbDataSet();
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

        public async Task<EbDataSet> GetLiveData(int limit, int offset, List<Param> param, List<SortColumn> sort, List<Param> search)
        {
            EbDataSet Data = null;
            try
            {
                if (App.Settings.CurrentLocation != null)
                {
                    param.Add(new Param
                    {
                        Name = "eb_loc_id",
                        Type = "11",
                        Value = App.Settings.CurrentLocation.LocId.ToString()
                    });
                }

                var vd = await DataService.Instance.GetDataAsync(this.DataSourceRefId, limit, offset, param, sort, search, false);
                Data = vd?.Data;
            }
            catch (Exception ex)
            {
                EbLog.Info($"Failed to get Live data for '{DataSourceRefId}'");
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
                EbLog.Info("Visualization get parameters from local error");
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

        public List<Param> GetSearchParams()
        {
            var search = new List<Param>();

            foreach (var dc in this.SearchColumns)
            {
                search.Add(new Param
                {
                    Name = dc.ColumnName,
                    Type = ((int)dc.Type).ToString(),
                });
            }
            return search;
        }

        #endregion
    }
}
