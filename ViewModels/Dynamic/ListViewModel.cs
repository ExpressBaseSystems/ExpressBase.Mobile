using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels.BaseModels;
using ExpressBase.Mobile.Views.Dynamic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels.Dynamic
{
    public class ListViewModel : DynamicBaseViewModel
    {
        private readonly IDataService dataService;

        public int DataCount { set; get; }

        public EbMobileVisualization Visualization { set; get; }

        public EbDataTable DataTable { set; get; }

        public List<EbMobileControl> FilterControls { set; get; }

        public List<SortColumn> SortColumns { set; get; }

        public Command ApplyFilterCommand => new Command(ApplyFilterClicked);

        public Action ViewAction { set; get; }

        public ListViewModel(EbMobilePage page) : base(page)
        {
            this.Visualization = (EbMobileVisualization)this.Page.Container;

            this.FilterControls = this.Visualization.FilterControls;
            this.SortColumns = this.Visualization.SortColumns.Select(x => new SortColumn { Name = x.ColumnName }).ToList();

            dataService = DataService.Instance;
        }

        public async Task SetData(int offset = 0)
        {
            try
            {
                if (this.Page.NetworkMode == NetworkMode.Online && !Utils.HasInternet)
                {
                    DependencyService.Get<IToast>().Show("You are not connected to internet.");
                    throw new Exception("no internet");
                }

                EbDataSet ds = await this.Visualization.GetData(this.Page.NetworkMode, offset);
                if (ds != null && ds.Tables.HasLength(2))
                {
                    DataTable = ds.Tables[1];
                    DataCount = Convert.ToInt32(ds.Tables[0].Rows[0]["count"]);
                }
                else
                    throw new Exception("no internet");
            }
            catch (Exception ex)
            {
                DataTable = new EbDataTable();
                Log.Write(ex.Message);
            }
        }

        public async Task<ContentPage> GetPageByContainer(CustomFrame frame, EbMobilePage page)
        {
            ContentPage renderer = null;
            try
            {
                await Task.Delay(10);

                switch (page.Container)
                {
                    case EbMobileForm f:

                        if (this.Visualization.FormMode == WebFormDVModes.New_Mode)
                            renderer = new FormRender(page, frame.DataRow);
                        else
                        {
                            int id = Convert.ToInt32(frame.DataRow["id"]);
                            if (id <= 0) throw new Exception("id has ivalid value" + id);
                            renderer = new FormRender(page, id);
                        }
                        break;
                    case EbMobileVisualization v:
                        renderer = new LinkedListRender(page, this.Visualization, frame);
                        break;
                    case EbMobileDashBoard d:
                        renderer = new DashBoardRender(page, frame.DataRow);
                        break;
                    default:
                        Log.Write("inavlid container type");
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
            return renderer;
        }

        public async Task Refresh(List<DbParameter> parameters)
        {
            try
            {
                DataTable.Rows.Clear();
                DataCount = 0;

                List<SortColumn> sort = this.SortColumns.FindAll(item => item.Selected);

                EbDataSet ds = await this.Visualization.GetData(this.Page.NetworkMode, 0, parameters, sort);

                if (ds != null && ds.Tables.HasLength(2))
                {
                    DataTable = ds.Tables[1];
                    DataCount = Convert.ToInt32(ds.Tables[0].Rows[0]["count"]);
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }

        public void BindMethod(Action method)
        {
            ViewAction = method;
        }

        private void ApplyFilterClicked()
        {
            if (ViewAction != null) ViewAction.Invoke();
        }
    }
}
