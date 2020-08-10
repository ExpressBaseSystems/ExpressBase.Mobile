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
            get { return datarows; }
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

        public bool IsFilterVisible => SortColumns.Any() || FilterControls.Any();

        public Command AddCommand => new Command(async () => await AddButtonClicked());

        public Command EditCommand => new Command(async () => await EditButtonClicked());

        public Command RefreshListCommand => new Command(async () => await RefreshDataAsync());

        public Command ItemTappedCommand => new Command(async (o) => await ListItemTapped(o));

        public Command ApplyFilterCommand => new Command(async (o) => await ApplyFilterClicked(o));

        private readonly DynamicFrame sender;

        private List<DbParameter> contextParams;

        private List<DbParameter> filterParams;

        public LinkedListViewModel(EbMobilePage page, EbMobileVisualization context, DynamicFrame sender) : base(page)
        {
            this.Visualization = (EbMobileVisualization)page.Container;
            this.ListItemIndex = new IntRef();
            this.Context = context;
            this.sender = sender;

            this.SortColumns = this.Visualization.SortColumns.Select(x => new SortColumn { Name = x.ColumnName }).ToList();
            this.FilterControls = this.Visualization.FilterControls;
        }

        public override async Task InitializeAsync()
        {
            this.SetContextParams();
            await SetDataAsync();
        }

        private void SetContextParams()
        {
            EbDataRow dataRow = sender.DataRow;
            contextParams = new List<DbParameter>();

            try
            {
                if (this.NetworkType == NetworkMode.Online)
                {
                    foreach (Param param in this.Visualization.DataSourceParams)
                    {
                        object data = dataRow[param.Name];

                        if (data != null)
                        {
                            contextParams.Add(new DbParameter
                            {
                                ParameterName = param.Name,
                                DbType = Convert.ToInt32(param.Type),
                                Value = data
                            });
                        }
                    }
                }
                else
                {
                    string sql = HelperFunctions.B64ToString(this.Visualization.OfflineQuery.Code);
                    List<string> _parameters = HelperFunctions.GetSqlParams(sql);

                    foreach (string param in _parameters)
                    {
                        object data = dataRow[param];
                        if (data != null)
                        {
                            contextParams.Add(new DbParameter
                            {
                                ParameterName = param,
                                Value = data
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                EbLog.Write(ex.Message);
            }
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
                EbLog.Write("LinkedList_SetData---" + ex.Message);
            }
        }

        private async Task ListItemTapped(object item)
        {
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
                    ContentPage renderer = this.GetPageByContainer(dyFrame, page);

                    if (renderer != null)
                        await (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(renderer);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task AddButtonClicked()
        {
            EbMobilePage page = HelperFunctions.GetPage(Visualization.LinkRefId);

            if (page != null && page.Container is EbMobileForm)
            {
                FormRender Renderer = new FormRender(page, Visualization, sender.DataRow, 0);
                await (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(Renderer);
            }
        }

        private async Task EditButtonClicked()
        {
            EbMobilePage _page = HelperFunctions.GetPage(Context.SourceFormRefId);

            if (_page != null)
            {
                int id = Convert.ToInt32(sender.DataRow["id"]);
                if (id != 0)
                {
                    FormRender Renderer = new FormRender(_page, id);
                    await (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(Renderer);
                }
            }
        }

        private ContentPage GetPageByContainer(DynamicFrame senderInner, EbMobilePage page)
        {
            ContentPage renderer = null;
            try
            {
                switch (page.Container)
                {
                    case EbMobileForm f:
                        if (this.Visualization.FormMode == WebFormDVModes.New_Mode)
                            renderer = new FormRender(page, Visualization, senderInner.DataRow);
                        else
                        {
                            int id = Convert.ToInt32(senderInner.DataRow["id"]);
                            if (id <= 0) throw new Exception("id has ivalid value" + id);
                            renderer = new FormRender(page, id);
                        }
                        break;
                    case EbMobileVisualization v:
                        renderer = new LinkedListRender(page, this.Visualization, senderInner);
                        break;
                    case EbMobileDashBoard d:
                        renderer = new DashBoardRender(page, senderInner.DataRow);
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

                var temp = filterParams == null ? contextParams : contextParams.Union(filterParams).OrderBy(x => x.ParameterName).ToList();

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
                EbLog.Write(ex.Message);
            }
        }

        private async Task ApplyFilterClicked(object filters)
        {
            filterParams = (List<DbParameter>)filters;
            await RefreshDataAsync();
        }
    }
}
