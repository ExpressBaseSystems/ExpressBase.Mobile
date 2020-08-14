using ExpressBase.Mobile.Configuration;
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

        public bool HasLocationSwitcher => App.Settings.Vendor.HasLocationswitcher && Utils.Locations.Count > 1;

        public bool HasMyActions => App.Settings.Vendor.HasActions;

        public bool HasOfflineSupport => App.Settings.Vendor.HasOfflineFeature;

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

                byte[] bytes = helper.GetPhoto($"{App.Settings.AppDirectory}/{App.Settings.Sid}/user.png");
                if (bytes != null)
                    UserDp.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
            }
            catch (Exception ex)
            {
                EbLog.Write("SideBar.SetDp---" + ex.Message);
            }
        }

        private async void About_Tapped(object sender, EventArgs e)
        {
            App.RootMaster.IsPresented = false;
            await App.RootMaster.Detail.Navigation.PushAsync(new About());
        }

        private async void ChangeSolution_Tapped(object sender, EventArgs e)
        {
            App.RootMaster.IsPresented = false;
            await App.RootMaster.Detail.Navigation.PushAsync(new MySolutions(true));
        }

        private async void ChangeApplication_Tapped(object sender, EventArgs e)
        {
            App.RootMaster.IsPresented = false;
            await App.RootMaster.Detail.Navigation.PushAsync(new MyApplications(true));
        }

        private async void ChangeLocation_Tapped(object sender, EventArgs e)
        {
            App.RootMaster.IsPresented = false;
            await App.RootMaster.Detail.Navigation.PushAsync(new MyLocations());
        }

        private void Logout_Tapped(object sender, EventArgs e)
        {
            App.RootMaster.IsPresented = false;
            Page navigator = (Application.Current.MainPage as MasterDetailPage).Detail;
            Page current = (navigator as NavigationPage).CurrentPage;

            if (current is Home)
            {
                (current as Home).ShowLogoutConfirmBox();
            }
        }

        private async void Setup_Tapped(object sender, EventArgs e)
        {
            App.RootMaster.IsPresented = false;
            await App.RootMaster.Detail.Navigation.PushAsync(new SettingsView());
        }

        private async void MyActions_Tapped(object sender, EventArgs e)
        {
            if (!Utils.HasInternet)
            {
                Utils.Alert_NoInternet();
                return;
            }

            App.RootMaster.IsPresented = false;
            await (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(new MyActions());
        }

        private void SyncButton_Tapped(object sender, EventArgs e)
        {
            App.RootMaster.IsPresented = false;
            Page navigator = (Application.Current.MainPage as MasterDetailPage).Detail;
            Page current = (navigator as NavigationPage).CurrentPage;

            if (current is Home)
            {
                (current as Home).TriggerSync();
            }
        }
    }
}