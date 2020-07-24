using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Home : ContentPage
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
                CurrentLocation.Text = App.Settings.CurrentLocation?.LongName;
                CurrentSolution.Text = App.Settings.Sid.ToUpper();

                if (!isRendered)
                {
                    await viewModel.InitializeAsync();
                    isRendered = true;
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
            IToast toast = DependencyService.Get<IToast>();
            try
            {
                if (!Utils.HasInternet)
                {
                    Utils.Alert_NoInternet();
                    RootRefreshView.IsRefreshing = false;
                    return;
                }
                else
                {
                    if (NAVService.IsTokenExpired(App.Settings.RToken))
                    {
                        await NAVService.LoginWithNS();
                    }
                    else
                    {
                        IconedLoader.IsVisible = true;
                        App.Settings.MobilePages = null;

                        await viewModel.UpdateAsync();
                        MenuView.Notify("ItemSource");
                        ToggleStatus();

                        RootRefreshView.IsRefreshing = false;
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

        public async void RefreshView()
        {
            await viewModel.LocationSwitched();
            MenuView.Notify("ItemSource");
            ToggleStatus();
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
    }
}