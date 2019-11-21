using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Views;
using System;
using System.IdentityModel.Tokens.Jwt;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Data;
using System.IO;

namespace ExpressBase.Mobile
{
    public partial class App : Xamarin.Forms.Application
    {

        static EbDataBase _database;

        public static EbDataBase DataDB
        {
            get
            {
                if (_database == null)
                {
                    string sid = string.Format("{0}.db3", Store.GetValue(AppConst.SID));
                    _database = new EbDataBase(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), sid));
                }
                return _database;
            }
        }

        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage();

            string sid = Store.GetValue(AppConst.SID);
            if (sid == null)
            {
                MainPage.Navigation.PushAsync(new SolutionSelect());
            }
            else
            {
                string rtoken = Store.GetValue(AppConst.RTOKEN);
                if (rtoken != null)
                {
                    if (Auth.IsTokenExpired(rtoken))
                    {
                        string username = Store.GetValue(AppConst.USERNAME);
                        string password = Store.GetValue(AppConst.PASSWORD);
                        ApiAuthResponse authresponse = Auth.TryAuthenticate(username, password);
                        if (authresponse.IsValid)
                        {
                            MainPage = new RootMaster(typeof(ObjectsRenderer));
                        }
                        else
                        {
                            MainPage.Navigation.PushAsync(new Login(username));
                        }
                    }
                    else
                    {
                        string apid = Store.GetValue(AppConst.APPID);

                        if (apid == null)
                            MainPage.Navigation.PushAsync(new AppSelect());
                        else
                            MainPage = new RootMaster(typeof(ObjectsRenderer));
                    }
                }
                else
                    MainPage.Navigation.PushAsync(new Login());
            }
        }

        protected override void OnStart()
        {
            
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
