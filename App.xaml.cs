using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Views;
using System;
using System.IdentityModel.Tokens.Jwt;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

namespace ExpressBase.Mobile
{
    public partial class App : Xamarin.Forms.Application
    {
        public App()
        {
            //Xamarin.Forms.Application.Current.On<Xamarin.Forms.PlatformConfiguration.Android>().UseWindowSoftInputModeAdjust(WindowSoftInputModeAdjust.Resize);
            InitializeComponent();

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            string sid = Store.GetValue(Constants.SID);
            if (sid == null)
            {
                MainPage = new Boarding_Sid();
            }
            else
            {
                string rtoken = Store.GetValue(Constants.RTOKEN);
                if (rtoken != null)
                {
                    if (Auth.IsTokenExpired(rtoken))
                    {
                        string username = Store.GetValue(Constants.USERNAME);
                        string password = Store.GetValue(Constants.PASSWORD);
                        ApiAuthResponse authresponse = Auth.TryAuthenticate(username, password);
                        if (authresponse.IsValid)
                        {
                            MainPage = new RootMaster(typeof(ObjectsRenderer));
                        }
                        else
                        {
                            MainPage = new Login(username);
                        }
                    }
                    else
                    {
                        string apid = Store.GetValue(Constants.APPID);

                        if (apid == null)
                            MainPage = new Home();
                        else
                            MainPage = new RootMaster(typeof(ObjectsRenderer));
                    }
                }
                else
                    MainPage = new Login();
            }
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
