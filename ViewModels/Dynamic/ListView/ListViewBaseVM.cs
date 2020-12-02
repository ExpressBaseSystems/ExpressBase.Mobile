using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels.BaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels.Dynamic
{
    public class ListViewBaseVM : DynamicBaseViewModel
    {
        public int Offset { set; get; }

        public int DataCount { set; get; }

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

        public EbMobileVisualization Visualization { set; get; }

        public IntRef ListItemIndex { set; get; }

        public List<SortColumn> SortColumns { set; get; }

        public List<EbMobileControl> FilterControls { set; get; }

        public SeparatorVisibility ShowRowSeperator => Visualization.XFSeperator();

        public bool IsFilterVisible => SortColumns.Any() || FilterControls.Any();

        public bool IsSearchVisible => Visualization.IsSearchable();

        protected EbDataRow ContextRecord { set; get; }

        protected List<DbParameter> ContextParams { set; get; }

        protected List<DbParameter> FilterParams { set; get; }

        protected List<Param> SearchColumns { set; get; }

        protected bool IsTapped { set; get; }

        public Command ItemTappedCommand => new Command<DynamicFrame>(async (o) => await NavigateToLink(o));

        public Command AddCommand => new Command(async () => await NavigateToFabLink());

        public Command ApplyFilterCommand => new Command<List<DbParameter>>(async (o) => await FilterData(o));

        public Command RefreshListCommand => new Command(async () => await RefreshDataAsync());

        public Command SearchCommand => new Command<string>(async (query) => await SearchData(query));

        public ListViewBaseVM(EbMobilePage page) : base(page)
        {
            this.Visualization = (EbMobileVisualization)page.Container;
            this.ListItemIndex = new IntRef();

            this.SortColumns = GetSortColumns();
            this.FilterControls = this.Visualization.FilterControls;
        }

        protected List<SortColumn> GetSortColumns()
        {
            return this.Visualization.SortColumns.Select(x => new SortColumn { Name = x.ColumnName }).ToList();
        }

        protected List<SortColumn> GetSortColumsActive()
        {
            return this.SortColumns.FindAll(item => item.Selected);
        }

        public async Task RefreshDataAsync(bool isSearch = false)
        {
            try
            {
                this.DataCount = 0;

                List<SortColumn> sort = this.GetSortColumsActive();

                List<DbParameter> filter = this.GetFilterParameters();

                List<Param> searchCols = isSearch ? SearchColumns : null;

                EbDataSet ds = await this.Visualization.GetData(this.NetworkType, Offset, filter, sort, searchCols);

                if (ds != null && ds.Tables.HasLength(2))
                {
                    DataRows = ds.Tables[1].Rows;
                    DataCount = Convert.ToInt32(ds.Tables[0].Rows[0]["count"]);
                }
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }

            IsRefreshing = false;
            App.Navigation.RefreshCurrentPage();
        }

        public async Task SearchData(string search)
        {
            if (search == null)
                return;

            Offset = 0;
            SearchColumns ??= this.Visualization.GetSearchParams();
            SearchColumns.ForEach(param => param.Value = search);

            try
            {
                await this.RefreshDataAsync(true);
            }
            catch (Exception ex)
            {
                EbLog.Info($"Error at search in '{PageName}'");
                EbLog.Error(ex.Message);
            }
        }

        public async Task SetDataAsync()
        {
            try
            {
                if (!Utils.IsNetworkReady(this.NetworkType))
                {
                    Utils.Alert_NoInternet();
                    throw new Exception("no internet");
                }

                EbDataSet ds = await this.Visualization.GetData(this.NetworkType, this.Offset, ContextParams);
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
                EbLog.Error(ex.Message);
            }
        }

        protected async Task NavigateToLink(DynamicFrame item)
        {
            if (IsTapped)
                return;

            IToast toast = DependencyService.Get<IToast>();
            try
            {
                EbMobilePage page = EbPageHelper.GetPage(this.Visualization.LinkRefId);

                if (this.NetworkType != page.NetworkMode)
                {
                    toast.Show("Link page Mode is different.");
                    return;
                }
                else
                {
                    IsTapped = true;

                    ContentPage renderer = EbPageHelper.ResolveByContext(this.Visualization, item.DataRow, page);

                    if (renderer != null)
                        await App.Navigation.NavigateMasterAsync(renderer);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            IsTapped = false;
        }

        protected async Task FilterData(List<DbParameter> filters)
        {
            FilterParams = filters;
            await RefreshDataAsync();
        }

        protected virtual Task NavigateToFabLink()
        {
            return Task.FromResult(false);
        }

        protected virtual List<DbParameter> GetFilterParameters()
        {
            return new List<DbParameter>();
        }
    }
}
