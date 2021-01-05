using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
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
            return wrapper = GetFrame();
        }

        public override void SetBindingValue(EbDataSet dataSet)
        {
            if (!string.IsNullOrEmpty(BindingTable))
            {
                int tableIndex = Convert.ToInt32(BindingTable.Substring(BindingTable.Length - 1));

                if (dataSet.TryGetTable(tableIndex, out EbDataTable dt))
                {
                    EbXDataGrid xGrid = new EbXDataGrid { DataSource = dt };

                    wrapper.Content = xGrid;
                }
            }
        }
    }
}
