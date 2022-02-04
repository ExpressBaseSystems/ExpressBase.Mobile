using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Structures;
using ExpressBase.Mobile.ViewModels.BaseModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels.Dynamic
{
    public class ListViewBaseViewModel : DynamicBaseViewModel
    {
        public int Offset { set; get; }

        public int DataCount { set; get; }

        public EbCPLayout EbLayout { get; set; }

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

        protected IDataService dataService;

        public Command ItemTappedCommand => new Command<DynamicFrame>(async (o) => await NavigateToLink(o));

        public Command AddCommand => new Command(async () => await NavigateToFabLink());

        public Command ApplyFilterCommand => new Command<List<DbParameter>>(async (o) => await FilterData(o));

        public Command RefreshListCommand => new Command(async () => await RefreshDataAsync());

        public Command SearchCommand => new Command<string>(async (query) => await SearchData(query));

        public ListViewBaseViewModel(EbMobilePage page) : base(page)
        {
            this.Visualization = (EbMobileVisualization)page.Container;
            this.ListItemIndex = new IntRef();

            this.SortColumns = GetSortColumns();
            this.FilterControls = this.Visualization.FilterControls;

            dataService = new DataService();
        }

        private async Task<EbDataSet> GetData(List<DbParameter> dbParameters, List<SortColumn> sort, List<Param> search)
        {
            if (this.NetworkType == NetworkMode.Online)
            {
                List<Param> param = dbParameters.ToParams();

                return await this.GetLiveData(param, sort, search);
            }
            else
            {
                return this.GetLocalData(ContextParams, sort, search);
            }
        }

        private EbDataSet GetLocalData(List<DbParameter> dbParameters, List<SortColumn> sortOrder, List<Param> search)
        {
            try
            {
                dbParameters = dbParameters.ConvertAll(e => new DbParameter()
                {
                    ParameterName = e.ParameterName,
                    DbType = e.DbType,
                    Value = e.Value
                });

                DbParameter userParam = dbParameters.Find(item => item.ParameterName == EbKeywords.UserId);

                if (userParam != null)
                {
                    userParam.Value = App.Settings.UserId;
                    userParam.DbType = (int)EbDbTypes.Int32;
                }

                string sql = HelperFunctions.WrapSelectQuery(Visualization.GetQuery, dbParameters);

                int len = Visualization.PageLength == 0 ? 30 : Visualization.PageLength;

                dbParameters.Add(new DbParameter { ParameterName = "@limit", Value = len, DbType = (int)EbDbTypes.Int32 });
                dbParameters.Add(new DbParameter { ParameterName = "@offset", Value = Offset, DbType = (int)EbDbTypes.Int32 });

                return App.DataDB.DoQueries(sql, dbParameters.ToArray());
            }
            catch (Exception ex)
            {
                EbLog.Error("Failed to get local data");
                EbLog.Error(ex.Message);
            }
            return null;
        }

        private async Task<EbDataSet> GetLiveData(List<Param> param, List<SortColumn> sort, List<Param> search)
        {
            try
            {
                int limit = Visualization.PageLength == 0 ? 30 : Visualization.PageLength;

                if (App.Settings.CurrentLocation != null)
                {
                    param.Add(new Param
                    {
                        Name = EbKeywords.Location,
                        Type = "11",
                        Value = App.Settings.CurrentLocation.LocId.ToString()
                    });
                }

                MobileDataResponse dataResponse = await dataService.GetDataAsyncV2(Page.RefId, limit, Offset, param, sort, search);

                return dataResponse?.Data;
            }
            catch (Exception ex)
            {
                EbLog.Info($"Failed to get Live data for '{Page.RefId}'");
                EbLog.Error(ex.Message);
            }
            return null;
        }

        protected List<SortColumn> GetSortColumns()
        {
            return this.Visualization.SortColumns.Select(x => new SortColumn { Name = x.ColumnName }).ToList();
        }

        protected List<SortColumn> GetSortColumsActive()
        {
            return this.SortColumns.FindAll(item => item.Selected);
        }

        public async Task GetDataAsync()
        {
            try
            {
                if (!Utils.IsNetworkReady(this.NetworkType))
                {
                    Utils.Alert_NoInternet();
                    throw new Exception("no internet");
                }

                ContextParams ??= new List<DbParameter>();

                EbDataSet dataSet = await GetData(ContextParams, null, null);

                if (dataSet != null && dataSet.Tables.HasLength(2))
                {
                    DataRows = dataSet.Tables[1].Rows;
                    DataCount = Convert.ToInt32(dataSet.Tables[0].Rows[0]["count"]);
                }
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
        }

        public async Task RefreshDataAsync(bool isSearch = false)
        {
            try
            {
                this.DataCount = 0;

                List<SortColumn> sort = this.GetSortColumsActive();

                List<DbParameter> filter = this.GetFilterParameters();

                List<Param> searchCols = isSearch ? SearchColumns : null;

                EbDataSet ds = await GetData(filter, sort, searchCols);

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

        protected async Task NavigateToLink(DynamicFrame item)
        {
            if (IsTapped || EbPageHelper.IsShortTap())
                return;
            IsTapped = true;
            try
            {
                IsBusy = true;
                await Task.Delay(100);
                if (Visualization.LinkExpr != null && !Visualization.LinkExpr.IsEmpty())
                {
                    if (!EbListHelper.EvaluateLinkExpr(item.DataRow, Visualization.LinkExpr.GetCode()))
                    {
                        if (string.IsNullOrWhiteSpace(Visualization.LinkExprFailMsg))
                            Utils.Toast("Link Blocked");
                        else
                            Utils.Toast(Visualization.LinkExprFailMsg);
                        EbLog.Info("[LinkExpr] evaluation blocked link navigation");
                        IsBusy = false;
                        return;
                    }
                }

                if (this.Visualization.LinkRefId.Split(CharConstants.DASH)[2] == "3")
                {
                    await RenderReport(item.DataRow);
                }
                else
                {
                    EbMobilePage page = EbPageHelper.GetPage(this.Visualization.LinkRefId);

                    if (this.NetworkType != page.NetworkMode)
                    {
                        Utils.Toast("Link page Mode is different.");
                        IsBusy = false;
                        return;
                    }
                    else
                    {
                        ContentPage renderer = EbPageHelper.ResolveByContext(this.Visualization, item.DataRow, page);

                        if (renderer != null)
                            await App.Navigation.NavigateMasterAsync(renderer);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            IsBusy = false;
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

        private async Task RenderReport(EbDataRow Row)
        {
            if (Visualization.ContextToControlMap?.Count > 0)
            {
                if (NetworkType == NetworkMode.Online && !Utils.IsNetworkReady(this.NetworkType))
                {
                    Utils.Alert_NoInternet();
                    return;
                }
                List<Param> param = new List<Param>();
                foreach (EbCTCMapper map in Visualization.ContextToControlMap)
                {
                    param.Add(new Param
                    {
                        Name = map.ControlName,
                        Type = ((int)EbDbTypes.Int32).ToString(),
                        Value = Row[map.ColumnName]?.ToString()
                    });
                }

                PdfService PdfService = new PdfService();
                ReportRenderResponse r = null;
                try
                {
                    if (NetworkType == NetworkMode.Online)
                        r = await PdfService.GetPdfOnline(this.Visualization.LinkRefId, JsonConvert.SerializeObject(param));
                    else
                        r = PdfService.GetPdfOffline(this.Visualization.LinkRefId, JsonConvert.SerializeObject(param));

                    if (r?.ReportBytea != null)
                    {
                        INativeHelper helper = DependencyService.Get<INativeHelper>();
                        string root = App.Settings.AppDirectory;
                        string path = helper.NativeRoot + $"/{root}/{AppConst.SHARED_MEDIA}/{App.Settings.Sid.ToUpper()}/PDF{(DateTime.UtcNow.ToString("yyyyMMddHHmmss"))}.pdf";
                        File.WriteAllBytes(path, r.ReportBytea);

                        IAppHandler handler = DependencyService.Get<IAppHandler>();
                        string res = await handler.PrintPdfFile(path);
                        if (res != "success")
                        {
                            await Launcher.OpenAsync(new OpenFileRequest
                            {
                                File = new ReadOnlyFile(path)
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    EbLog.Error("ListBaseViewModel.RenderReport---" + ex.Message + " \n" + ex.StackTrace);
                }
            }
        }
    }
}
