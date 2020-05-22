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

        public int BackButtonCount { set; get; } = 0;

        public HomeViewModel ViewModel { set; get; }

        public Home()
        {
            InitializeComponent();
            BindingContext = ViewModel = new HomeViewModel();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                EbLocation loc = Settings.CurrentLocObject;
                if (loc != null)
                    CurrentLocation.Text = loc.ShortName;
                else
                    CurrentLocation.Text = "Default";

                if (!isRendered)
                {
                    await ViewModel.InitializeAsync();
                    isRendered = true;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
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
                if (!Settings.HasInternet)
                {
                    toast.Show("Not connected to Internet!");
                    return;
                }
                else
                {
                    await IdentityService.AuthIfTokenExpiredAsync();

                    RootRefreshView.IsRefreshing = true;
                    Store.RemoveJSON(AppConst.OBJ_COLLECTION);

                    await ViewModel.Refresh();
                    MenuView.Notify("ItemSource");

                    RootRefreshView.IsRefreshing = false;
                    toast.Show("Refreshed");
                }
            }
            catch (Exception ex)
            {
                toast.Show("Something went wrong. Please try again");
                Log.Write(ex.Message);
            }
        }

        private async void MyActions_Tapped(object sender, EventArgs e)
        {
            try
            {
                if (!Settings.HasInternet)
                {
                    DependencyService.Get<IToast>().Show("You are not connected to internet.");
                    return;
                }
                Device.BeginInvokeOnMainThread(() => { ViewModel.IsBusy = true; });

                await Auth.AuthIfTokenExpiredAsync();//authenticate if token expired

                MyActionsResponse actionResp = await RestServices.Instance.GetMyActionsAsync();

                await (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(new MyActions(actionResp));
                Device.BeginInvokeOnMainThread(() => ViewModel.IsBusy = false);
            }
            catch (Exception ex)
            {
                Device.BeginInvokeOnMainThread(() => ViewModel.IsBusy = false);
                Log.Write(ex.Message);
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