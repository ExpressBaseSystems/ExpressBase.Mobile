using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbMobileTableView : EbMobileDashBoardControl
    {
        public string DataSourceRefId { set; get; }

        public EbScript OfflineQuery { set; get; }

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
            WebView wview = new WebView
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
