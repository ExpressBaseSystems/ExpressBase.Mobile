using ExpressBase.Mobile.Services;
using Xamarin.Forms;
using ExpressBase.Mobile.Data;
using System.Threading.Tasks;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services.Navigation;

namespace ExpressBase.Mobile
{
    public partial class App : Application
    {
        private static IDataBase dataDb;

        public static IDataBase DataDB => dataDb ??= DependencyService.Get<IDataBase>();

        public static MasterDetailPage RootMaster { set; get; }

        public static SettingsServices Settings { set; get; }

        public static INavigationService Navigation { set; get; }

        public static double ScreenX;

        static App()
        {
            Settings = new SettingsServices();
            Navigation = new NavigationService();
        }

        public App(EbNFData payload = null)
        {
            InitializeComponent();

            InitializeNavigation(payload);
        }

        private Task InitializeNavigation(EbNFData payload)
        {
            return Navigation.InitializeAppAsync(payload);
        }

        protected override async void OnStart()
        {
            if (Settings.Vendor.AllowNotifications && Settings.CurrentUser != null)
            {
                await NotificationService.Instance.UpdateNHRegistration();
            }
        }
    }
}
