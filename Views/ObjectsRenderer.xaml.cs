using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ObjectsRenderer : ContentPage
    {
        public ObjectsRenderer()
        {
            InitializeComponent();
            ObjectsRenderViewModel model = new ObjectsRenderViewModel();
            BindingContext = model;
            scrollView.Content = model.View;
        }

        protected override bool OnBackButtonPressed()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                bool status = await this.DisplayAlert("Alert!", "Do you really want to exit?", "Yes", "No");
                if (status)
                {
                    INativeHelper nativeHelper = null;
                    nativeHelper = DependencyService.Get<INativeHelper>();
                    nativeHelper.CloseApp();
                }
            });
            return true;
        }

        public void RefreshComplete(View view)
        {
            scrollView.Content = view; 
        }

        private void RefreshView_Refreshing(object sender, System.EventArgs e)
        {
            if (!Settings.HasInternet)
            {
                DependencyService.Get<IToast>().Show("Not connected to internet!");
                return;
            }

            RootRefreshView.IsRefreshing = true;
            var vm = (BindingContext as ObjectsRenderViewModel);
            vm.SetUpData();
            Store.Remove(AppConst.OBJ_COLLECTION);
            vm.BuildView();
            scrollView.Content = vm.View;
            RootRefreshView.IsRefreshing = false;
        }
    }
}