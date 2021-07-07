using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using System;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbMobileTableView : EbMobileDashBoardControl
    {
        public string DataSourceRefId { set; get; }

        public EbScript OfflineQuery { set; get; }

        public string BindingTable { set; get; }

        private EbXFrame wrapper;

        public override View Draw()
        {
            if (!string.IsNullOrEmpty(DataSourceRefId))
            {
                this.GetBindingData();
            }
            return wrapper = GetFrame();
        }

        public override void SetBindingValue(EbDataSet dataSet)
        {
            if (string.IsNullOrEmpty(DataSourceRefId) && !string.IsNullOrEmpty(BindingTable))
            {
                int tableIndex = Convert.ToInt32(BindingTable.Substring(BindingTable.Length - 1));

                if (dataSet.TryGetTable(tableIndex, out EbDataTable dt))
                {
                    InitView(dt);
                }
            }
        }

        public async void GetBindingData()
        {
            try
            {
                MobileDataResponse response = await DataService.Instance.GetDataAsync(this.DataSourceRefId, 0, 0, null, null, null, false);

                if (response.Data != null && response.Data.Tables.HasLength(2))
                {
                    InitView(response.Data.Tables[1]);
                }
            }
            catch (Exception ex)
            {
                EbLog.Error("Error at [AutoFill] : " + ex.Message);
            }
        }

        private void InitView(EbDataTable dt)
        {
            EbXDataGrid xGrid = new EbXDataGrid { DataSource = dt};

            wrapper.Content = new StackLayout
            {
                Children = { xGrid }
            };

            //ScrollView scrollView = new ScrollView
            //{
            //    Orientation = ScrollOrientation.Neither,
            //    Content = xGrid
            //};
            //wrapper.Content = scrollView;
        }
    }
}
