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
    public partial class MyApplications : ContentPage
    {
        public MyApplicationsViewModel ViewModel { set; get; }

        public MyApplications()
        {
            InitializeComponent();
            BindingContext = ViewModel = new MyApplicationsViewModel();
        }

        private void ApplicationsRefresh_Refreshing(object sender, System.EventArgs e)
        {
            try
            {
                if (!Settings.HasInternet)
                {
                    DependencyService.Get<IToast>().Show("Not connected to internet!");
                    return;
                }
                ApplicationsRefresh.IsRefreshing = true;
                Store.RemoveJSON(AppConst.APP_COLLECTION);
                ViewModel.PullApplications();
                ApplicationsRefresh.IsRefreshing = false;
            }
            catch (Exception ex)
            {
                ApplicationsRefresh.IsRefreshing = false;
                Log.Write(ex.Message);
            }
        }
    }
}