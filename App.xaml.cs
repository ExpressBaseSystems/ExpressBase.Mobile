using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Views;
using Xamarin.Forms;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using System.Threading.Tasks;
using System.Linq;
using ExpressBase.Mobile.Models;
using System;
using ExpressBase.Mobile.Services.Notification;

namespace ExpressBase.Mobile
{
    public partial class App : Application
    {
        private static IDataBase _dataDb;

        public static IDataBase DataDB => _dataDb ??= DependencyService.Get<IDataBase>();

        public static MasterDetailPage RootMaster { set; get; }

        public static SettingsServices Settings { set; get; }

        public static double ScreenX;

        private EbNFData nfPayLoad;

        public App(EbNFData nfdata = null)
        {
            InitializeComponent();

            nfPayLoad = nfdata;
            Settings = new SettingsServices();
        }

        protected override async void OnStart()
        {
            Settings.InitializeConfig();

            await InitNavigation();

            if (nfPayLoad != null) await NewIntentAction(nfPayLoad);

            if (Settings.Vendor.AllowNotifications && Settings.CurrentUser != null)
            {
                await NotificationService.Instance.UpdateNHRegisratation();
            }
        }

        private async Task InitNavigation()
        {
            MainPage = new NavigationPage();

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

        public async Task NewIntentAction(EbNFData payload)
        {
            try
            {
                if (payload.Link == null && RootMaster == null)
                {
                    EbLog.Info("Intentaction aborted, link and rootmaster null");
                    return;
                }
                await NFIntentService.Resolve(payload);
                nfPayLoad = null;
            }
            catch (Exception ex)
            {
                EbLog.Info("Unknown error at indent action");
                EbLog.Error(ex.Message);
                EbLog.Error(ex.StackTrace);
            }
        }
    }
}
