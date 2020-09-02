using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.ViewModels;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MyActions : ContentPage, IRefreshable
    {
        public bool isRendered;

        private readonly MyActionsViewModel viewModel;

        public MyActions()
        {
            InitializeComponent();
            BindingContext = viewModel = new MyActionsViewModel();
            Loader.IsVisible = true;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (!isRendered)
            {
                await viewModel.InitializeAsync();
                isRendered = true;
            }
            Loader.IsVisible = false;
        }

        private async void MyActionsRefresh_Refreshing(object sender, EventArgs e)
        {
            try
            {
                if (!Utils.HasInternet)
                {
                    DependencyService.Get<IToast>().Show("Not connected to internet!");
                    return;
                }
                MyActionsRefresh.IsRefreshing = true;
                await viewModel.InitializeAsync();
            }
            catch (Exception ex)
            {
                EbLog.Message("Failed to refresh myaction data");
                EbLog.Error(ex.Message);
            }
            MyActionsRefresh.IsRefreshing = false;
        }

        private void RefreshButton_Clicked(object sender, EventArgs e)
        {
            MyActionsRefresh_Refreshing(sender, e);
        }

        public void Refreshed()
        {
            //not implemented
        }

        public void UpdateRenderStatus()
        {
            isRendered = false;
        }

        public bool CanRefresh()
        {
            return true;
        }
    }
}