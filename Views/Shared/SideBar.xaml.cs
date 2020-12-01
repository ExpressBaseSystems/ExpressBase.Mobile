using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using System;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views.Shared
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SideBar : ContentPage
    {
        public bool HasAppSwitcher => Utils.Applications.Count > 1;

        public bool HasSolutionSwitcher => App.Settings.Vendor.HasSolutionSwitcher;

        public bool HasLocationSwitcher => App.Settings.Vendor.HasLocationSwitcher && Utils.Locations.Count > 1;

        public bool HasMyActions => App.Settings.Vendor.HasActions;

        public SideBar()
        {
            InitializeComponent();
            BindingContext = this;

            User user = App.Settings.CurrentUser;
            UserName.Text = user.FullName;
            Email.Text = user.Email;
            this.SetDp();
        }

        private void SetDp()
        {
            try
            {
                INativeHelper helper = DependencyService.Get<INativeHelper>();

                byte[] bytes = helper.GetFile($"{App.Settings.AppDirectory}/{App.Settings.Sid}/user.png");
                if (bytes != null)
                    UserDp.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
            }
            catch (Exception ex)
            {
                EbLog.Error("SideBar.SetDp---" + ex.Message);
            }
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