using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Home : ContentPage, IRefreshable
    {
        private bool isRendered;

        private int backButtonCount;

        private readonly HomeViewModel viewModel;

        public Home()
        {
            InitializeComponent();
            IconedLoader.IsVisible = true;
            BindingContext = viewModel = new HomeViewModel();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                CurrentLocation.Text = App.Settings.CurrentLocation?.LongName.ToLower();
                CurrentSolution.Text = App.Settings.Sid.ToUpper();

                if (!isRendered)
                {
                    await viewModel.InitializeAsync();
                    isRendered = !viewModel.RefreshOnAppearing;
                }

                ToggleStatus();
                IconedLoader.IsVisible = false;
            }
            catch (Exception ex)
            {
                EbLog.Write(ex.Message);
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

        private async void RefreshView_Refreshing(object sender, System.EventArgs e)
        {
            RootRefreshView.IsRefreshing = false;
            IToast toast = DependencyService.Get<IToast>();
            try
            {
                if (!Utils.HasInternet)
                {
                    Utils.Alert_NoInternet();
                    return;
                }
                else
                {
                    if (NavigationService.IsTokenExpired(App.Settings.RToken))
                        await NavigationService.LoginWithNS();
                    else
                    {
                        IconedLoader.IsVisible = true;
                        App.Settings.MobilePages = null;

                        await viewModel.UpdateAsync();
                        this.ToggleStatus();

                        IconedLoader.IsVisible = false;
                        toast.Show("Refreshed");
                    }
                }
            }
            catch (Exception ex)
            {
                toast.Show("Something went wrong. Please try again");
                EbLog.Write(ex.Message);
                IconedLoader.IsVisible = false;
            }
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

        private void ToggleStatus()
        {
            if (viewModel.IsEmpty())
                EmptyMessage.IsVisible = true;
            else
                EmptyMessage.IsVisible = false;
        }

        public void Refreshed() { }

        public void UpdateRenderStatus()
        {
            isRendered = false;
        }
    }
}