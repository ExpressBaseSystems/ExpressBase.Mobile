using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.ViewModels;
using ExpressBase.Mobile.Views.Base;
using System;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MyActions : EbContentPage
    {
        private readonly MyActionsViewModel viewModel;

        public MyActions()
        {
            InitializeComponent();
            BindingContext = viewModel = new MyActionsViewModel();
            EbLayout.ShowLoader();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (!IsRendered)
            {
                await viewModel.InitializeAsync();
                IsRendered = true;
            }
            EbLayout.HideLoader();
        }

        private async void MyActionsRefresh_Refreshing(object sender, EventArgs e)
        {
            try
            {
                if (!Utils.HasInternet)
                {
                    Utils.Alert_NoInternet();
                    return;
                }
                await viewModel.InitializeAsync();
                Utils.Toast("Refreshed");
            }
            catch (Exception ex)
            {
                EbLog.Info("Failed to refresh myaction data");
                EbLog.Error(ex.Message);
            }
            MyActionsRefresh.IsRefreshing = false;
        }

        private void RefreshButton_Clicked(object sender, EventArgs e)
        {
            MyActionsRefresh_Refreshing(sender, e);
        }

        public override void UpdateRenderStatus()
        {
            IsRendered = false;
        }

        public override bool CanRefresh()
        {
            return true;
        }
    }
}