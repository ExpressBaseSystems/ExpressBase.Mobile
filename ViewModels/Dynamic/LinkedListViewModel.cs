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
    public class LinkedListViewModel : DynamicBaseViewModel
    {
        private RowColletion datarows;

        public RowColletion DataRows
        {
            get => datarows;
            set
            {
                datarows = value;
                NotifyPropertyChanged();
            }
        }

        public int Offset { set; get; }

        public int DataCount { set; get; }

        public EbMobileVisualization Visualization { set; get; }

        public IntRef ListItemIndex { set; get; }

        public EbMobileVisualization Context { set; get; }

        public List<SortColumn> SortColumns { set; get; }

        public List<EbMobileControl> FilterControls { set; get; }

        public SeparatorVisibility ShowRowSeperator => Visualization.ShowRowSeperator ? SeparatorVisibility.Default : SeparatorVisibility.None;

        public bool IsFilterVisible => SortColumns.Any() || FilterControls.Any();

        #region commands

        public Command AddCommand => new Command(async () => await AddButtonClicked());

        public Command EditCommand => new Command(async () => await EditButtonClicked());

        public Command RefreshListCommand => new Command(async () => await RefreshDataAsync());

        public Command ItemTappedCommand => new Command(async (o) => await ListItemTapped(o));

        public Command ApplyFilterCommand => new Command(async (o) => await ApplyFilterClicked(o));

        #endregion

        private readonly EbDataRow sourceRecord;

        private List<DbParameter> contextParams;

        private List<DbParameter> filterParams;

        public LinkedListViewModel(EbMobilePage page, EbMobileVisualization context, EbDataRow row) : base(page)
        {
            this.Visualization = (EbMobileVisualization)page.Container;
            this.ListItemIndex = new IntRef();
            this.Context = context;
            this.sourceRecord = row;

            this.SortColumns = this.Visualization.SortColumns.Select(x => new SortColumn { Name = x.ColumnName }).ToList();
            this.FilterControls = this.Visualization.FilterControls;
        }

        public override async Task InitializeAsync()
        {
            contextParams = this.Visualization.GetContextParams(sourceRecord, this.NetworkType);
            await SetDataAsync();
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

                EbDataSet ds = await this.Visualization.GetData(this.Page.NetworkMode, this.Offset, this.contextParams);

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
                EbLog.Error("LinkedList_SetData---" + ex.Message);
            }
        }

        private bool isTapped = false;

        private async Task ListItemTapped(object item)
        {
            if (isTapped) return;

            DynamicFrame dyFrame = (DynamicFrame)item;

            try
            {
                EbMobilePage page = HelperFunctions.GetPage(this.Visualization.LinkRefId);
                if (this.NetworkType != page.NetworkMode)
                {
                    DependencyService.Get<IToast>().Show("Link page Mode is different.");
                    return;
                }
                else
                {
                    isTapped = true;

                    ContentPage renderer = this.GetPageByContainer(dyFrame.DataRow, page);

                    if (renderer != null)
                        await App.RootMaster.Detail.Navigation.PushAsync(renderer);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            isTapped = false;
        }

        private async Task AddButtonClicked()
        {
            EbMobilePage page = HelperFunctions.GetPage(Visualization.LinkRefId);

            if (page != null && page.Container is EbMobileForm)
            {
                FormRender Renderer = new FormRender(page, Visualization, sourceRecord);
                await App.RootMaster.Detail.Navigation.PushAsync(Renderer);
            }
        }

        private async Task EditButtonClicked()
        {
            EbMobilePage _page = HelperFunctions.GetPage(Context.SourceFormRefId);

            if (_page != null)
            {
                int id = Convert.ToInt32(sourceRecord["id"]);
                if (id != 0)
                {
                    FormRender Renderer = new FormRender(_page, id);
                    await App.RootMaster.Detail.Navigation.PushAsync(Renderer);
                }
            }
        }

        private ContentPage GetPageByContainer(EbDataRow row, EbMobilePage page)
        {
            ContentPage renderer = null;
            try
            {
                switch (page.Container)
                {
                    case EbMobileForm f:

                        if (this.Visualization.FormMode == WebFormDVModes.New_Mode)
                            renderer = new FormRender(page, Visualization.LinkFormParameters, row);
                        else
                        {
                            var map = Visualization.FormId;
                            if (map == null)
                            {
                                EbLog.Info("form id should be set");
                                throw new Exception("Form rendering exited! due to null value for 'FormId'");
                            }
                            else
                            {
                                int id = Convert.ToInt32(row[map.ColumnName]);
                                if (id <= 0)
                                {
                                    EbLog.Info("id has ivalid value" + id);
                                    throw new Exception("Form rendering exited! due to invalid id");
                                }
                                renderer = new FormRender(page, id);
                            }
                        }
                        break;
                    case EbMobileVisualization v:
                        renderer = new LinkedListRender(page, this.Visualization, row);
                        break;
                    case EbMobileDashBoard d:
                        renderer = new DashBoardRender(page, row);
                        break;
                    default:
                        EbLog.Error("inavlid container type");
                        break;
                }
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
            return renderer;
        }

        public async Task RefreshDataAsync()
        {
            try
            {
                DataCount = 0;

                List<SortColumn> sort = this.SortColumns.FindAll(item => item.Selected);

                var temp = filterParams == null ? contextParams : contextParams.Union(filterParams).ToList();

                temp?.OrderBy(x => x.ParameterName);

                EbDataSet ds = await this.Visualization.GetData(this.NetworkType, Offset, temp, sort);

                if (ds != null && ds.Tables.HasLength(2))
                {
                    DataRows = ds.Tables[1].Rows;
                    DataCount = Convert.ToInt32(ds.Tables[0].Rows[0]["count"]);
                }

                IsRefreshing = false;
                Page current = App.RootMaster.Detail.Navigation.NavigationStack.Last();
                (current as IRefreshable).Refreshed();
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
        }

        private async Task ApplyFilterClicked(object filters)
        {
            filterParams = (List<DbParameter>)filters;
            await RefreshDataAsync();
        }
    }
}
