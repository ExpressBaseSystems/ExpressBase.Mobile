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
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels.Dynamic
{
    public class ListViewModel : DynamicBaseViewModel
    {
        private readonly IDataService dataService;

        private Action viewAction;

        public int DataCount { set; get; }

        public EbMobileVisualization Visualization { set; get; }

        public EbDataTable DataTable { set; get; }

        public List<EbMobileControl> FilterControls { set; get; }

        public List<SortColumn> SortColumns { set; get; }

        public Command ApplyFilterCommand => new Command(ApplyFilterClicked);

        public bool IsFilterVisible => SortColumns.Any() || FilterControls.Any();

        public ListViewModel(EbMobilePage page) : base(page)
        {
            this.Visualization = (EbMobileVisualization)this.Page.Container;

            this.SortColumns = this.Visualization.SortColumns.Select(x => new SortColumn { Name = x.ColumnName }).ToList();
            this.FilterControls = this.Visualization.FilterControls;

            dataService = DataService.Instance;
        }

        public override async Task InitializeAsync()
        {
            await this.SetData();
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
                EbLog.Write(ex.Message);
            }
        }

        public ContentPage GetPageByContainer(CustomFrame frame, EbMobilePage page)
        {
            ContentPage renderer = null;
            try
            {
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
                        EbLog.Write("inavlid container type");
                        break;
                }
            }
            catch (Exception ex)
            {
                EbLog.Write(ex.Message);
            }
            return renderer;
        }

        public async Task Refresh(List<DbParameter> parameters, int offset = 0)
        {
            try
            {
                DataTable.Rows.Clear();
                DataCount = 0;

                List<SortColumn> sort = this.SortColumns.FindAll(item => item.Selected);

                EbDataSet ds = await this.Visualization.GetData(this.Page.NetworkMode, offset, parameters, sort);

                if (ds != null && ds.Tables.HasLength(2))
                {
                    DataTable = ds.Tables[1];
                    DataCount = Convert.ToInt32(ds.Tables[0].Rows[0]["count"]);
                }
            }
            catch (Exception ex)
            {
                EbLog.Write(ex.Message);
            }
        }

        public void BindMethod(Action method)
        {
            viewAction = method;
        }

        private void ApplyFilterClicked()
        {
            viewAction?.Invoke();
        }
    }
}
