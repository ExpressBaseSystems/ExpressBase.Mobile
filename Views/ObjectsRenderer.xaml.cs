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
    public partial class ObjectsRenderer : ContentPage
    {
        public int BackButtonCount { set; get; } = 0;

        public ObjectsRenderer()
        {
            InitializeComponent();
            ObjectsRenderViewModel model = new ObjectsRenderViewModel();
            BindingContext = model;
            scrollView.Content = model.View;
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
            scrollView.Content = view; 
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

                RootRefreshView.IsRefreshing = true;

                var Coll = await RestServices.Instance.GetEbObjects(Settings.AppId, Settings.LocationId, false);

                if(Coll != null)
                {
                    await Store.SetValueAsync(AppConst.OBJ_COLLECTION, JsonConvert.SerializeObject(Coll.Pages));
                    var vm = (BindingContext as ObjectsRenderViewModel);
                    vm.ObjectList = Coll.Pages;
                    vm.BuildView();
                    scrollView.Content = vm.View;
                }

                RootRefreshView.IsRefreshing = false;
                toast.Show("Refreshed");
            }
            catch(Exception ex)
            {
                toast.Show("Something went wrong. Please try again");
                Console.WriteLine(ex.Message);
            }
        }
    }
}