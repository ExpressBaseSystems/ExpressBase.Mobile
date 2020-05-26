using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.ViewModels;
using ExpressBase.Mobile.Views.Shared;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MyApplications : ContentPage
    {
        public bool isRendered;

        public MyApplicationsViewModel ViewModel { set; get; }

        public bool IsInternal { set; get; }

        public MyApplications(bool isInternal = false)
        {
            IsInternal = isInternal;

            InitializeComponent();
            Loader.IsVisible = true;
            BindingContext = ViewModel = new MyApplicationsViewModel();

            if (isInternal)
                ResetButton.IsVisible = false;
            else
                NavigationPage.SetHasBackButton(this, false);
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                CurrentLocation.Text = App.Settings.CurrentLocation?.ShortName;

                if (!isRendered)
                {
                    await ViewModel.InitializeAsync();
                    this.ToggleLocationSwitcher();
                    isRendered = true;
                }
                Loader.IsVisible = false;
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
                Loader.IsVisible = false;
            }
        }

        private async void ApplicationsRefresh_Refreshing(object sender, System.EventArgs e)
        {
            try
            {
                if (!Utils.HasInternet)
                {
                    DependencyService.Get<IToast>().Show("Not connected to internet!");
                    return;
                }
                ApplicationsRefresh.IsRefreshing = true;
                Store.RemoveJSON(AppConst.APP_COLLECTION);
                await ViewModel.Refresh();
                ApplicationsRefresh.IsRefreshing = false;

                ToggleLocationSwitcher();
            }
            catch (Exception ex)
            {
                ApplicationsRefresh.IsRefreshing = false;
                Log.Write(ex.Message);
            }
        }

        private void ResetButton_Clicked(object sender, EventArgs e)
        {
            ConfimReset.Show();
        }

        private async void LocSwitch_Clicked(object sender, EventArgs e)
        {
            var nav = new NavigationPage(new MyLocations(this))
            {
                BarBackgroundColor = Color.FromHex("0046bb"),
                BarTextColor = Color.White
            };

            if (IsInternal)
                await (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushModalAsync(nav);
            else
                await Application.Current.MainPage.Navigation.PushModalAsync(nav);
        }

        public void LocationPagePoped()
        {
            try
            {
                Store.RemoveJSON(AppConst.APP_COLLECTION);
                Task.Run(async () => await ViewModel.Refresh());
                ToggleLocationSwitcher();
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }

        private void ToggleLocationSwitcher()
        {
            if (ViewModel.Applications.Count <= 0)
                LocSwitchOverlay.IsVisible = true;
            else
                LocSwitchOverlay.IsVisible = false;
        }
    }
}