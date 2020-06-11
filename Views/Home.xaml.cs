using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
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

        private int BackButtonCount;

        private readonly HomeViewModel ViewModel;

        public Home()
        {
            InitializeComponent();
            IconedLoader.IsVisible = true;
            BindingContext = ViewModel = new HomeViewModel();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                CurrentLocation.Text = App.Settings.CurrentLocation?.ShortName;
                CurrentSolution.Text = App.Settings.Sid;

                if (!isRendered)
                {
                    await ViewModel.InitializeAsync();
                    isRendered = true;
                }
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
            BackButtonCount++;

            if (BackButtonCount == 2)
            {
                BackButtonCount = 0;
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
                    toast.Show("Not connected to Internet!");
                    return;
                }
                else
                {
                    await IdentityService.AuthIfTokenExpiredAsync();

                    RootRefreshView.IsRefreshing = true;
                    IconedLoader.IsVisible = true;
                    Store.RemoveJSON(AppConst.OBJ_COLLECTION);

                    await ViewModel.Refresh();
                    MenuView.Notify("ItemSource");

                    RootRefreshView.IsRefreshing = false;
                    IconedLoader.IsVisible = false;
                    toast.Show("Refreshed");
                }
            }
            catch (Exception ex)
            {
                toast.Show("Something went wrong. Please try again");
                EbLog.Write(ex.Message);
                IconedLoader.IsVisible = false;
            }
        }

        private void PushButton_Tapped(object sender, EventArgs e)
        {
            PushConfirmBox.Show();
        }

        public void ConfirmLogout()
        {
            LogoutDialog.Show();
        }
    }
}