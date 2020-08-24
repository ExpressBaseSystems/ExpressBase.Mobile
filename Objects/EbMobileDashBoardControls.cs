using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbMobileDashBoardControls : EbMobilePageBase
    {
        public virtual View XView { set; get; }

        public virtual void InitXControl() { }

        public virtual void InitXControl(EbDataRow DataRow) { }
    }

    public class EbMobileTableView : EbMobileDashBoardControls
    {
        public string DataSourceRefId { set; get; }

        public EbScript OfflineQuery { set; get; }

        //mob prop
        private EbDataTable Data { set; get; }

        private EbDataRow LinkedDataRow { set; get; }

        public override View XView { set; get; }

        public override void InitXControl(EbDataRow DataRow)
        {
            LinkedDataRow = DataRow;
            SetData();
            InitXView();
        }

        private void InitXView()
        {
            var wview = new WebView
            {
                MinimumHeightRequest = 100,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Source = new UrlWebViewSource
                {
                    Url = DependencyService.Get<INativeHelper>().GetAssetsURl() + "WebView/DataTable.html"
                }
            };
            wview.Navigated += Wview_Navigated;
            this.XView = wview;
        }

        private void Wview_Navigated(object sender, WebNavigatedEventArgs e)
        {
            string dt_string = JsonConvert.SerializeObject(this.Data);
            (sender as WebView).Eval($"drawTable({dt_string})");
        }

        private void SetData()
        {
            try
            {
                string sql = HelperFunctions.WrapSelectQueryUnPaged(HelperFunctions.B64ToString(this.OfflineQuery.Code));
                List<DbParameter> _DbParams = new List<DbParameter>();
                List<string> _Params = HelperFunctions.GetSqlParams(sql);
                if (_Params.Count > 0)
                    this.GetParameterValues(_DbParams, _Params);
                Data = App.DataDB.DoQuery(sql, _DbParams.ToArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void GetParameterValues(List<DbParameter> _DbParams, List<string> _Params)
        {
            try
            {
                foreach (string _p in _Params)
                {
                    _DbParams.Add(new DbParameter
                    {
                        ParameterName = _p,
                        Value = this.LinkedDataRow[_p],
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
