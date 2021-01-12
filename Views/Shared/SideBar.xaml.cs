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
            BindingContext = viewModel = new SideBarViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            viewModel.Initialize();
        }

        private async void AboutClicked(object sender, EventArgs e)
        {
            await App.Navigation.NavigateMasterAsync(new About());
        }

        private async void ChangeSolution(object sender, EventArgs e)
        {
            await App.Navigation.NavigateMasterAsync(new MySolutions(true));
        }

        private async void ChangeApplication(object sender, EventArgs e)
        {
            await App.Navigation.NavigateMasterAsync(new MyApplications(true));
        }

        private async void ChangeLocation(object sender, EventArgs e)
        {
            await App.Navigation.NavigateMasterAsync(new MyLocations());
        }

        private void LogoutClicked(object sender, EventArgs e)
        {
            EbCPLayout.ConfirmLogoutAction();
        }

        private async void SetupClicked(object sender, EventArgs e)
        {
            await App.Navigation.NavigateMasterAsync(new SettingsView());
        }

        private async void MyActionsClicked(object sender, EventArgs e)
        {
            if (!Utils.HasInternet)
            {
                Utils.Alert_NoInternet();
                return;
            }

            await App.Navigation.NavigateMasterAsync(new MyActions());
        }

        private async void LinksClicked(object sender, EventArgs e)
        {
            await App.Navigation.NavigateMasterAsync(new Home());
        }
    }
}