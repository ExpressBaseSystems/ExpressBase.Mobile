using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Views;
using Xamarin.Forms;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using System.Threading.Tasks;
using System.Linq;
using ExpressBase.Mobile.Models;
using System;

namespace ExpressBase.Mobile
{
    public partial class App : Application
    {
        public static IDataBase DataDB { get; set; }

        public static MasterDetailPage RootMaster { set; get; }

        public static SettingsServices Settings { set; get; }

        public static double ScreenX;

        readonly EbNFData notification;

        public App(EbNFData nfdata = null)
        {
            InitializeComponent();

            notification = nfdata;

            DataDB = DependencyService.Get<IDataBase>();
            Settings = new SettingsServices();
        }

        protected override async void OnStart()
        {
            Settings.InitializeConfig();

            await InitNavigation();

            if (notification != null) await NewIntentAction(notification);

            if (Settings.Vendor.AllowNotifications && Settings.CurrentUser != null)
            {
                await NotificationService.Instance.UpdateNHRegisratation();
            }
        }

        private async Task InitNavigation()
        {
            MainPage = new NavigationPage();

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

        public async Task NewIntentAction(EbNFData data)
        {
            if (data.Link == null && RootMaster == null)
            {
                EbLog.Info("Intentaction aborted, link and rootmaster null");
                return;
            }

            EbNFLink link = data.Link;

            try
            {
                if (link.LinkType == EbNFLinkTypes.Action)
                {
                    await RootMaster.Detail.Navigation.PushAsync(new DoAction(link.ActionId));
                    EbLog.Info("Navigated to action submit : actionid =" + link.ActionId);
                }
                else if (link.LinkType == EbNFLinkTypes.Page)
                {
                    if (string.IsNullOrEmpty(link.LinkRefId))
                    {
                        EbLog.Info("Intentaction link type is page but linkrefid null");
                        return;
                    }

                    EbMobilePage page = EbPageFinder.GetPage(link.LinkRefId);

                    if (page != null)
                    {
                        EbLog.Info("Intentaction page rendering :" + page.DisplayName);

                        ContentPage renderer = EbPageFinder.GetPageByContainer(page);
                        await App.RootMaster.Detail.Navigation.PushAsync(renderer);
                    }
                    else
                        EbLog.Info("Intentaction page not found for linkrefid:" + link.LinkRefId);
                }
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
