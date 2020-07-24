﻿using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Views;
using Xamarin.Forms;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using System.Threading.Tasks;
using System.Linq;

namespace ExpressBase.Mobile
{
    public partial class App : Application
    {
        public static IDataBase DataDB { get; set; }

        public static MasterDetailPage RootMaster { set; get; }

        public static SettingsServices Settings { set; get; }

        public App()
        {
            InitializeComponent();

            DataDB = DependencyService.Get<IDataBase>();
            Settings = new SettingsServices();
        }

        protected override async void OnStart()
        {
            Settings.InitializeConfig();

            await InitNavigation();

            if (Settings.Vendor.AllowNotifications && Settings.CurrentUser != null)
            {
                await NotificationService.Instance.UpdateNHRegisratation();
            }
        }

        private async Task InitNavigation()
        {
            MainPage = new NavigationPage
            {
                BarBackgroundColor = Settings.Vendor.GetPrimaryColor(),
                BarTextColor = Color.White
            };

            await Settings.InitializeSettings();

            if (Settings.Sid == null)
            {
                if (Utils.Solutions.Any())
                    await MainPage.Navigation.PushAsync(new MySolutions());
                else
                    await MainPage.Navigation.PushAsync(new NewSolution());
            }
            else
            {
                if (Settings.RToken != null)
                {
                    if (NAVService.IsTokenExpired(Settings.RToken))
                        await NAVService.LoginWithCS();
                    else
                    {
                        if (Settings.AppId <= 0)
                            await MainPage.Navigation.PushAsync(new MyApplications());
                        else
                        {
                            RootMaster = new RootMaster(typeof(Home));
                            MainPage = RootMaster;
                        }
                    }
                }
                else
                    await NAVService.LoginWithCS();
            }
        }
    }
}
