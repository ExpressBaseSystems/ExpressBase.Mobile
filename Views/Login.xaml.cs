using ExpressBase.Mobile.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Constants;

namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Login : ContentPage
    {
        public Login()
        {
            InitializeComponent();

            NavigationPage.SetHasNavigationBar(this, true);
            NavigationPage.SetHasBackButton(this, true);

            string url = Settings.RootUrl + "images/logo/" + Store.GetValue(AppConst.SID) + ".jpg";
            this.Logo.Source = ImageSource.FromUri(new Uri(url));
        }

        public Login(string username)
        {
            InitializeComponent();
            this.UserName.Text = username;
        }

        void OnLoginClick(object sender, EventArgs e)
        {
            string username = this.UserName.Text.Trim();
            string password = this.PassWord.Text.Trim();

            if (string.IsNullOrEmpty(username) && string.IsNullOrEmpty(password))
                return;

            ApiAuthResponse response = Auth.TryAuthenticate(username, password);
            if (response.IsValid)
            {
                Auth.UpdateStore(response, username, password);
                Application.Current.MainPage.Navigation.PushAsync(new AppSelect());
            }
        }
    }
}