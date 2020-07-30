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
    public enum RenderStyle
    {
        Flat = 1,
        Tile = 2
    }

    public class EbMobileVisualization : EbMobileContainer
    {
        public string DataSourceRefId { set; get; }

        public string SourceFormRefId { set; get; }

        public string LinkRefId { get; set; }

        public int PageLength { set; get; } = 30;

        public WebFormDVModes FormMode { set; get; }

        public EbScript OfflineQuery { set; get; }

        public EbMobileTableLayout DataLayout { set; get; }

        public List<Param> DataSourceParams { get; set; }

        public List<EbMobileControl> FilterControls { set; get; }

        public List<EbMobileDataColumn> SortColumns { set; get; }

        public List<EbMobileDataColToControlMap> LinkFormParameters { get; set; }

        public List<EbCTCMapper> ContextToControlMap { set; get; }

        public bool ShowNewButton { set; get; }

        public EbThickness Margin { set; get; }

        public EbThickness Padding { set; get; }

        public int RowSpacing { set; get; }

        public int ColumnSpacing { set; get; }

        public RenderStyle Style { set; get; } = RenderStyle.Flat;

        public int BorderRadius { set; get; }

        public string BorderColor { set; get; }

        public string BackgroundColor { set; get; }

        public bool BoxShadow { set; get; }

        public string ShadowColor { set; get; }

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

        public bool HasLink()
        {
            return !string.IsNullOrEmpty(LinkRefId);
        }

        public async Task<EbDataSet> GetData(NetworkMode networkType, int offset, List<DbParameter> parameters = null, List<SortColumn> sortOrder = null)
        {
            try
            {
                if (networkType == NetworkMode.Online)
                    return await this.GetLiveData(parameters, sortOrder, offset);
                else if (networkType == NetworkMode.Offline)
                    return this.GetLocalData(parameters, sortOrder, offset);
                else
                    return null;
            }
            catch (Exception ex)
            {
                EbLog.Write(ex.Message);
            }
            return null;
        }

        public EbDataSet GetLocalData(List<DbParameter> dbParameters, List<SortColumn> sortOrder, int offset)
        {
            EbDataSet Data = null;
            try
            {
                if (dbParameters == null)
                    dbParameters = new List<DbParameter>();

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
                EbLog.Write("EbMobileVisualization.GetData with params---" + ex.Message);
            }
            return Data;
        }

        public async Task<EbDataSet> GetLiveData(List<DbParameter> dbParameters, List<SortColumn> sortOrder, int offset)
        {
            EbDataSet Data = null;
            try
            {
                int len = this.PageLength == 0 ? 30 : this.PageLength;
                VisualizationLiveData vd = await DataService.Instance.GetDataAsync(this.DataSourceRefId, dbParameters.ToParams(), sortOrder, len, offset);
                Data = vd.Data;
            }
            catch (Exception ex)
            {
                EbLog.Write("EbMobileVisualization.GetLiveData with params---" + ex.Message);
            }
            return Data;
        }
    }
}
