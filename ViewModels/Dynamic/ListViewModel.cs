using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
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
        public int Offset { set; get; }

        public int DataCount { set; get; }

        private RowColletion datarows;

        public RowColletion DataRows
        {
            get { return datarows; }
            set
            {
                datarows = value;
                NotifyPropertyChanged();
            }
        }

        private List<DbParameter> parameters;

        public List<SortColumn> SortColumns { set; get; }

        public EbMobileVisualization Visualization { set; get; }

        public IntRef ListItemIndex { set; get; }

        public List<EbMobileControl> FilterControls { set; get; }

        public bool IsFilterVisible => SortColumns.Any() || FilterControls.Any();

        public Command ItemTappedCommand => new Command(async (o) => await ListItemTapped(o));

        public Command AddCommand => new Command(async () => await AddButtonClicked());

        public Command ApplyFilterCommand => new Command(async (o) => await ApplyFilterClicked(o));

        public Command RefreshListCommand => new Command(async () => await RefreshDataAsync());

        public ListViewModel(EbMobilePage page) : base(page)
        {
            this.Visualization = (EbMobileVisualization)this.Page.Container;
            this.ListItemIndex = new IntRef();

            this.SortColumns = this.Visualization.SortColumns.Select(x => new SortColumn { Name = x.ColumnName }).ToList();
            this.FilterControls = this.Visualization.FilterControls;
        }

        public override async Task InitializeAsync()
        {
            await this.SetDataAsync();
        }

        public async Task SetDataAsync()
        {
            try
            {
                if (this.Page.NetworkMode == NetworkMode.Online && !Utils.HasInternet)
                {
                    Utils.Alert_NoInternet();
                    throw new Exception("no internet");
                }

                EbDataSet ds = await this.Visualization.GetData(this.Page.NetworkMode, this.Offset);
                if (ds != null && ds.Tables.HasLength(2))
                {
                    DataRows = ds.Tables[1].Rows;
                    DataCount = Convert.ToInt32(ds.Tables[0].Rows[0]["count"]);
                }
                else
                    throw new Exception("no internet");
            }
            catch (Exception ex)
            {
                EbLog.Write(ex.Message);
            }
        }

        private async Task AddButtonClicked()
        {
            EbMobilePage page = HelperFunctions.GetPage(Visualization.LinkRefId);

            if (page != null && page.Container is EbMobileForm)
            {
                FormRender Renderer = new FormRender(page);
                await (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(Renderer);
            }
        }

        private async Task ListItemTapped(object item)
        {
            IToast toast = DependencyService.Get<IToast>();
            try
            {
                DynamicFrame customFrame = (DynamicFrame)item;
                EbMobilePage page = HelperFunctions.GetPage(this.Visualization.LinkRefId);
                if (this.NetworkType != page.NetworkMode)
                {
                    toast.Show("Link page Mode is different.");
                    return;
                }
                else
                {
                    ContentPage renderer = this.GetPageByContainer(customFrame.DataRow, page);

                    if (renderer != null)
                        await (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(renderer);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public ContentPage GetPageByContainer(EbDataRow row, EbMobilePage page)
        {
            ContentPage renderer = null;
            try
            {
                switch (page.Container)
                {
                    case EbMobileForm f:

                        if (this.Visualization.FormMode == WebFormDVModes.New_Mode)
                            renderer = new FormRender(page, Visualization, row);
                        else
                        {
                            int id = Convert.ToInt32(row["id"]);
                            if (id <= 0) throw new Exception("id has ivalid value" + id);
                            renderer = new FormRender(page, id);
                        }
                        break;
                    case EbMobileVisualization v:
                        renderer = new LinkedListRender(page, this.Visualization, row);
                        break;
                    case EbMobileDashBoard d:
                        renderer = new DashBoardRender(page, row);
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

        public async Task RefreshDataAsync()
        {
            try
            {
                DataCount = 0;

                List<SortColumn> sort = this.SortColumns.FindAll(item => item.Selected);

                EbDataSet ds = await this.Visualization.GetData(this.NetworkType, Offset, parameters, sort);

                if (ds != null && ds.Tables.HasLength(2))
                {
                    DataRows = ds.Tables[1].Rows;
                    DataCount = Convert.ToInt32(ds.Tables[0].Rows[0]["count"]);
                }
            }
            catch (Exception ex)
            {
                EbLog.Write(ex.Message);
            }

            IsRefreshing = false;
            Page current = App.RootMaster.Detail.Navigation.NavigationStack.Last();
            (current as IRefreshable).Refreshed();
        }

        private async Task ApplyFilterClicked(object filters)
        {
            parameters = (List<DbParameter>)filters;
            await RefreshDataAsync();
        }
    }
}
