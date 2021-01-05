using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Services.DashBoard;
using ExpressBase.Mobile.ViewModels.BaseModels;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels.Dynamic
{
    public class DashBoardRenderViewModel : DynamicBaseViewModel
    {
        public EbMobileDashBoard DashBoard { set; get; }

        public Thickness Padding => DashBoard.Padding == null ? 5 : DashBoard.Padding.ConvertToXValue();

        readonly IDashBoardService dashService;

        private EbDataSet _data;

        public EbDataSet Data
        {
            get => _data;
            set
            {
                _data = value;
                NotifyPropertyChanged();
            }
        }

        public Command RefreshCommand => new Command(async () => await RefreshDataAsync());

        public DashBoardRenderViewModel()
        {
            dashService = new DashBoardService();
        }

        public DashBoardRenderViewModel(EbMobilePage page) : base(page)
        {
            dashService = new DashBoardService();

            DashBoard = this.Page.Container as EbMobileDashBoard;
        }

        public override async Task InitializeAsync()
        {
            try
            {
                if (this.IsOnline())
                {
                    Data = await dashService.GetDataAsync(DashBoard.DataSourceRefId);
                }
                else
                {
                    Data = await dashService.GetLocalDataAsync(DashBoard.OfflineQuery);
                }
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
        }

        public async Task RefreshDataAsync()
        {
            await InitializeAsync();
            Device.BeginInvokeOnMainThread(() => IsRefreshing = false);
        }
    }
}
