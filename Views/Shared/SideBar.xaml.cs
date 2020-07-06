using ExpressBase.Mobile.Constants;
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
        public bool HasAppSwitcher => App.Settings.Vendor.HasAppSwitcher;

        public bool HasSolutionSwitcher => App.Settings.Vendor.HasSolutionSwitcher;

        public bool HasLocationSwitcher => App.Settings.Vendor.HasLocationswitcher && Utils.Locations.Count > 1;

        public SideBar()
        {
            InitializeComponent();
            BindingContext = this;

            User _user = App.Settings.CurrentUser;
            UserName.Text = _user.FullName;
            Email.Text = _user.Email;
            this.SetDp();
        }

        private void SetDp()
        {
            try
            {
                INativeHelper helper = DependencyService.Get<INativeHelper>();
                string sid = App.Settings.Sid;

                var bytes = helper.GetPhoto($"ExpressBase/{sid}/user.png");
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
            try
            {
                App.RootMaster.IsPresented = false;
                Page navigator = (Application.Current.MainPage as MasterDetailPage).Detail;
                Home current = (navigator as NavigationPage).CurrentPage as Home;
                current.ConfirmLogout();
            }
            catch (Exception ex)
            {
                EbLog.Write(ex.Message);
            }
        }

        private void Setup_Tapped(object sender, EventArgs e)
        {

        }
    }
}