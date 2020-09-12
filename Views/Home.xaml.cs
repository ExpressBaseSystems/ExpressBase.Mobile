using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.ViewModels;
using ExpressBase.Mobile.Views.Base;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Home : EbContentPage
    {
        private int backButtonCount;

        private readonly HomeViewModel viewModel;

        public Home()
        {
            InitializeComponent();
            BindingContext = viewModel = new HomeViewModel();
            IconedLoader.IsVisible = true;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                CurrentLocation.Text = App.Settings.CurrentLocation?.LongName.ToLower();
                CurrentSolution.Text = App.Settings.Sid.ToUpper();

                if (!IsRendered)
                {
                    await viewModel.InitializeAsync();
                    IsRendered = true;
                }
                ToggleStatus();
                IconedLoader.IsVisible = false;
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
                IconedLoader.IsVisible = false;
            }
        }

        protected override bool OnBackButtonPressed()
        {
            backButtonCount++;

            if (backButtonCount == 2)
            {
                backButtonCount = 0;
                return false;
            }
            else
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    DependencyService.Get<IToast>().Show("Press again to EXIT!");
                });
                return true;
            }
        }

        private async void RefreshView_Refreshing(object sender, EventArgs e)
        {
            RootRefreshView.IsRefreshing = false;
            try
            {
                if (!Utils.HasInternet)
                {
                    Utils.Alert_NoInternet();
                    return;
                }
                else
                {
                    IconedLoader.IsVisible = true;
                    await viewModel.UpdateAsync();
                    this.ToggleStatus();

                    IconedLoader.IsVisible = false;
                }
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
                IconedLoader.IsVisible = false;
            }
        }

        private void ToggleStatus()
        {
            EmptyMessage.IsVisible = viewModel.IsEmpty();
        }

        public void ShowLogoutConfirmBox()
        {
            LogoutDialog.Show();
        }

        public void TriggerSync()
        {
            if (!Utils.HasInternet)
            {
                Utils.Alert_NoInternet();
                return;
            }
            PushConfirmBox.Show();
        }

        public override bool CanRefresh()
        {
            return viewModel.RefreshOnAppearing;
        }

        public override void UpdateRenderStatus()
        {
            IsRendered = false;
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            App.ScreenX = width;
        }
    }
}