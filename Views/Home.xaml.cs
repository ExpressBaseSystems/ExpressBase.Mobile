using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Home : ContentPage
    {
        public int BackButtonCount { set; get; } = 0;

        public HomeViewModel ViewModel { set; get; }

        public Home()
        {
            InitializeComponent();
            BindingContext = ViewModel = new HomeViewModel();
            ObjectContainer.Content = ViewModel.View;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            int _current = Convert.ToInt32(Store.GetValue(AppConst.CURRENT_LOCATION));
            EbLocation loc = Settings.Locations.Find(item => item.LocId == _current);
            if (loc != null)
                CurrentLocation.Text = loc.ShortName;
            else
                CurrentLocation.Text = "Default";
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

        public void RefreshComplete(View view)
        {
            ObjectContainer.Content = view;
        }

        private async void RefreshView_Refreshing(object sender, System.EventArgs e)
        {
            IToast toast = DependencyService.Get<IToast>();
            try
            {
                if (!Settings.HasInternet)
                {
                    toast.Show("Not connected to Internet!");
                    RootRefreshView.IsRefreshing = false;
                    return;
                }
                else
                {
                    await Auth.AuthIfTokenExpiredAsync();//authenticate if token expired

                    RootRefreshView.IsRefreshing = true;
                    var Coll = await RestServices.Instance.GetEbObjects(Settings.AppId, Settings.LocationId, true);
                    if (Coll != null)
                    {
                        Store.SetJSON(AppConst.OBJ_COLLECTION, Coll.Pages);
                        var vm = (BindingContext as HomeViewModel);
                        vm.ObjectList = Coll.Pages;
                        vm.BuildView();
                        ObjectContainer.Content = vm.View;
                        vm.DeployFormTables();
                        await CommonServices.Instance.LoadLocalData(Coll.Data);//load pulled data to local
                    }
                    RootRefreshView.IsRefreshing = false;
                    toast.Show("Refreshed");
                }
            }
            catch (Exception ex)
            {
                toast.Show("Something went wrong. Please try again");
                Log.Write("ROOT MENU REFRESH-" + ex.Message);
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
                Device.BeginInvokeOnMainThread(() =>
                {
                    ViewModel.LoaderMessage = "Loading...";
                    ViewModel.IsBusy = true;
                });

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
    }
}