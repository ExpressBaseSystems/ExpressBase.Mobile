using ExpressBase.Mobile.Services;
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
        #region properties

        public static IDataBase DataDB { get; set; }

        public static MasterDetailPage RootMaster { set; get; }

        public static SettingsServices Settings { set; get; }

        #endregion

        public App()
        {
            InitializeComponent();

            //Creating offline DB sevice class using X-Forms dependency service
            DataDB = DependencyService.Get<IDataBase>();
            Settings = new SettingsServices();
        }

        #region App states

        protected override async void OnStart()
        {
            Settings.InitializeConfig();

            await InitNavigation();

            if (Settings.Vendor.AllowNotifications && Settings.CurrentUser != null)
            {
                //register for notification if token
                await NotificationService.Instance.UpdateNHRegisratation();
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
        }

        protected override void OnSleep()
        {
            base.OnSleep();
        }

        #endregion 

        /// <summary>
        /// Navigate to page on app start with respect to application data stored locally
        /// </summary>
        /// <returns>void</returns>
        private async Task InitNavigation()
        {
            MainPage = new NavigationPage
            {
                BarBackgroundColor = Settings.Vendor.GetPrimaryColor(),
                BarTextColor = Color.White
            };

            //Initializing Stored data
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
                    if (NavigationService.IsTokenExpired(Settings.RToken))
                        await NavigationService.LoginWithCS();
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
                    await NavigationService.LoginWithCS();
            }
        }
    }
}
