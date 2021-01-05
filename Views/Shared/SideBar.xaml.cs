using ExpressBase.Mobile.CustomControls;
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
            BindingContext = viewModel = SideBarViewModel.Instance;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            viewModel.Initialize();
        }

        private async void AboutTapped(object sender, EventArgs e)
        {
            App.RootMaster.IsPresented = false;
            await App.Navigation.NavigateMasterAsync(new About());
        }

        private async void ChangeSolutionTapped(object sender, EventArgs e)
        {
            App.RootMaster.IsPresented = false;
            await App.Navigation.NavigateMasterAsync(new MySolutions(true));
        }

        private async void ChangeApplicationTapped(object sender, EventArgs e)
        {
            App.RootMaster.IsPresented = false;
            await App.Navigation.NavigateMasterAsync(new MyApplications(true));
        }

        private async void ChangeLocationTapped(object sender, EventArgs e)
        {
            App.RootMaster.IsPresented = false;
            await App.Navigation.NavigateMasterAsync(new MyLocations());
        }

        private void LogoutTapped(object sender, EventArgs e)
        {
            App.RootMaster.IsPresented = false;
            EbCPLayout.ConfirmLogoutAction();
        }

        private async void SetupTapped(object sender, EventArgs e)
        {
            App.RootMaster.IsPresented = false;
            await App.Navigation.NavigateMasterAsync(new SettingsView());
        }

        private async void MyActionsTapped(object sender, EventArgs e)
        {
            if (!Utils.HasInternet)
            {
                Utils.Alert_NoInternet();
                return;
            }

            App.RootMaster.IsPresented = false;
            await App.Navigation.NavigateMasterAsync(new MyActions());
        }

        private async void LinksTapped(object sender, EventArgs e)
        {
            App.RootMaster.IsPresented = false;
            await App.Navigation.NavigateMasterAsync(new Home());
        }
    }
}