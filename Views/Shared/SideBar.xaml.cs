using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.ViewModels;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views.Shared
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SideBar : ContentPage
    {
        private readonly SideBarViewModel viewModel;

        public SideBar()
        {
            InitializeComponent();
            BindingContext = viewModel = new SideBarViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            viewModel.Initialize();
        }

        private async void About_Tapped(object sender, EventArgs e)
        {
            App.RootMaster.IsPresented = false;
            await App.Navigation.NavigateMasterAsync(new About());
        }

        private async void ChangeSolution_Tapped(object sender, EventArgs e)
        {
            App.RootMaster.IsPresented = false;
            await App.Navigation.NavigateMasterAsync(new MySolutions(true));
        }

        private async void ChangeApplication_Tapped(object sender, EventArgs e)
        {
            App.RootMaster.IsPresented = false;
            await App.Navigation.NavigateMasterAsync(new MyApplications(true));
        }

        private async void ChangeLocation_Tapped(object sender, EventArgs e)
        {
            App.RootMaster.IsPresented = false;
            await App.Navigation.NavigateMasterAsync(new MyLocations());
        }

        private void Logout_Tapped(object sender, EventArgs e)
        {
            App.RootMaster.IsPresented = false;
            if (App.Navigation.GetCurrentPage() is Home current)
                current.ShowLogoutConfirmBox();
        }

        private async void Setup_Tapped(object sender, EventArgs e)
        {
            App.RootMaster.IsPresented = false;
            await App.Navigation.NavigateMasterAsync(new SettingsView());
        }

        private async void MyActions_Tapped(object sender, EventArgs e)
        {
            if (!Utils.HasInternet)
            {
                Utils.Alert_NoInternet();
                return;
            }

            App.RootMaster.IsPresented = false;
            await App.Navigation.NavigateMasterAsync(new MyActions());
        }
    }
}