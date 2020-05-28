using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MyActions : ContentPage
    {
        public bool isRendered;

        public MyActionsViewModel ViewModel { set; get; }

        public MyActions()
        {
            InitializeComponent();
            BindingContext = ViewModel = new MyActionsViewModel();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            Loader.IsVisible = true;
            if (!isRendered)
            {
                await ViewModel.InitializeAsync();
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
                await ViewModel.RefreshMyActions();
                MyActionsRefresh.IsRefreshing = false;
            }
            catch (Exception ex)
            {
                MyActionsRefresh.IsRefreshing = false;
                Log.Write(ex.Message);
            }
        }
    }
}