using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Views;
using Xamarin.Forms;
using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using System.Threading.Tasks;

namespace ExpressBase.Mobile
{
    public partial class App : Application
    {
        public static string DbPath { set; get; }

        public static IDataBase DataDB { get; set; }

        public static MasterDetailPage RootMaster { set; get; }

        public static SettingsServices Settings { set; get; }

        public App(string dbPath)
        {
            InitializeComponent();

            DbPath = dbPath;
            if (DataDB == null)
                DataDB = DependencyService.Get<IDataBase>();

            Settings = new SettingsServices();
        }

        public App()
        {
            InitializeComponent();

            DataDB = DependencyService.Get<IDataBase>();
            Settings = new SettingsServices();
        }

        private async Task InitNavigation()
        {
            MainPage = new NavigationPage
            {
                BarBackgroundColor = Color.FromHex("0046bb"),
                BarTextColor = Color.White
            };

            await Settings.Resolve();

            if (Settings.Sid == null)
                await MainPage.Navigation.PushAsync(new MySolutions());
            else
            {
                if (Settings.RToken != null)
                {
                    if (Utils.HasInternet && IdentityService.IsTokenExpired(Settings.RToken))
                    {
                        string username = await Store.GetValueAsync(AppConst.USERNAME);
                        string password = await Store.GetValueAsync(AppConst.PASSWORD);

                        ApiAuthResponse authresponse = await IdentityService.Instance.AuthenticateAsync(username, password);

                        if (authresponse.IsValid)
                        {
                            RootMaster = new RootMaster(typeof(Home));
                            MainPage = RootMaster;
                        }
                        else
                            await MainPage.Navigation.PushAsync(new Login());
                    }
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
                    await MainPage.Navigation.PushAsync(new Login());
            }
        }

        protected override async void OnStart()
        {
            await InitNavigation();
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
