using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Views.Dynamic;
using ExpressBase.Mobile.Views.Shared;
using System.Collections.Generic;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Views
{
    public class RootMaster : MasterDetailPage
    {
        public RootMaster()
        {
            Master = new SideBar();
            Detail = new NavigationPage
            {
                BarBackgroundColor = App.Settings.Vendor.GetPrimaryColor(),
                BarTextColor = Color.White
            };

            InitNavigation();
        }

        public void InitNavigation()
        {
            EbMobileSettings settings = App.Settings.CurrentApplication?.AppSettings;

            if (settings != null && !string.IsNullOrEmpty(settings.DashBoardRefId))
            {
                EbMobilePage page = EbPageHelper.GetPage(settings.DashBoardRefId);

                if (page != null && page.Container is EbMobileDashBoard)
                {
                    DashBoardRender dashboard = new DashBoardRender(page)
                    {
                        Title = "Home",
                        IconImageSource = new FontImageSource
                        {
                            Size = 12,
                            FontFamily = (OnPlatform<string>)HelperFunctions.GetResourceValue("FontAwesome"),
                            Glyph = "\uf015"
                        }
                    };
                    dashboard.HideToolBar();

                    Home links = new Home()
                    {
                        Title = "Links",
                        IconImageSource = new FontImageSource
                        {
                            Size = 12,
                            FontFamily = (OnPlatform<string>)HelperFunctions.GetResourceValue("FontAwesome"),
                            Glyph = "\uf00a"
                        }
                    };

                    List<Page> pages = new List<Page> { dashboard, links };

                    Detail.Navigation.PushAsync(new TabbedHome(pages));
                }
                else
                {
                    EbLog.Info("Default application dashboard not found, check object permissions");
                    Detail.Navigation.PushAsync(new Home());
                }
            }
            else
            {
                Detail.Navigation.PushAsync(new Home());
            }
        }

        protected override bool OnBackButtonPressed()
        {
            return base.OnBackButtonPressed();
        }
    }
}
