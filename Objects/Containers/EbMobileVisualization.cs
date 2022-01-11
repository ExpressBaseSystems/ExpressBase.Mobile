using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbMobileVisualization : EbMobileContainer
    {
        public MobileVisualizationType Type { set; get; }

        public List<EbMobileStaticParameter> StaticParameters { set; get; }

        public List<EbMobileStaticListItem> Items { set; get; }

        public string DataSourceRefId { set; get; }

        public string SourceFormRefId { set; get; }

        public string LinkRefId { get; set; }

        public int PageLength { set; get; } = 30;

        public string MessageOnEmpty { set; get; }

        public WebFormDVModes FormMode { set; get; }

        public bool RenderAsPopup { set; get; }

        public EbMobileDataColToControlMap FormId { set; get; }

        public EbScript OfflineQuery { set; get; }

        public EbMobileTableLayout DataLayout { set; get; }

        public List<Param> DataSourceParams { get; set; }

        public List<EbMobileControl> FilterControls { set; get; }

        public List<EbMobileDataColumn> SortColumns { set; get; }

        public List<EbMobileDataColumn> SearchColumns { set; get; }

        public List<EbMobileDataColToControlMap> LinkFormParameters { get; set; }

        public List<EbCTCMapper> ContextToControlMap { set; get; }

        public bool ShowLinkIcon { set; get; }

        public EbScript LinkExpr { get; set; }

        public string LinkExprFailMsg { get; set; }

        public bool ShowNewButton { set; get; }

        public string NewButtonText { set; get; }

        public bool UseLinkSettings { set; get; }

        public string FabLinkRefId { get; set; }

        public List<EbCTCMapper> ContextToFabControlMap { set; get; }

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

        public string GetQuery => HelperFunctions.B64ToString(this.OfflineQuery.Code);

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
            List<Param> search = new List<Param>();

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
    }
}
