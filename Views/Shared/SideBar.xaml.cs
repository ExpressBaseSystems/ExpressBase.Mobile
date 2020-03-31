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
        public SideBar()
        {
            InitializeComponent();

            var _user = Settings.UserObject;
            UserName.Text = _user.FullName;
            Email.Text = _user.Email;
            this.SetDp();
        }

        private void SetDp()
        {
            INativeHelper helper = DependencyService.Get<INativeHelper>();
            string sid = Settings.SolutionId;
            try
            {
                var bytes = helper.GetPhoto($"ExpressBase/{sid}/user.png");
                if (bytes != null)
                    UserDp.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
            }
            catch (Exception ex)
            {
                Log.Write("SideBar.SetDp---" + ex.Message);
            }
        }

        private void About_Tapped(object sender, EventArgs e)
        {
            App.RootMaster.IsPresented = false;
            App.RootMaster.Detail.Navigation.PushAsync(new About());
        }

        private void ChangeSolution_Tapped(object sender, EventArgs e)
        {
            try
            {
                Application.Current.MainPage = new NavigationPage(new SolutionSelect())
                {
                    BarBackgroundColor = Color.FromHex("0046bb"),
                    BarTextColor = Color.White
                };
                App.RootMaster.IsPresented = false;
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }

        private async void ChangeApplication_Tapped(object sender, EventArgs e)
        {
            try
            {
                App.RootMaster.IsPresented = false;
                await App.RootMaster.Detail.Navigation.PushAsync(new AppSelect());
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }

        private async void ChangeLocation_Tapped(object sender, EventArgs e)
        {
            try
            {
                App.RootMaster.IsPresented = false;
                await App.RootMaster.Detail.Navigation.PushAsync(new LocationsView());
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }

        private void Logout_Tapped(object sender, EventArgs e)
        {
            try
            {
                Store.Remove(AppConst.BTOKEN);
                Store.Remove(AppConst.RTOKEN);
                Store.Remove(AppConst.USER_ID);
                Store.Remove(AppConst.PASSWORD);
                Store.RemoveJSON(AppConst.USER_OBJECT);
                Store.RemoveJSON(AppConst.USER_LOCATIONS);
                Store.Remove(AppConst.CURRENT_LOCATION);

                Store.Remove(AppConst.APPID);
                Store.Remove(AppConst.APPNAME);
                Store.RemoveJSON(AppConst.OBJ_COLLECTION);
                Store.RemoveJSON(AppConst.APP_COLLECTION);
                Application.Current.MainPage = new NavigationPage(new Login())
                {
                    BarBackgroundColor = Color.FromHex("0046bb"),
                    BarTextColor = Color.White
                };
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }
    }
}