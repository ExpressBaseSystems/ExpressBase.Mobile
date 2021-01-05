using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Views.Base;
using ExpressBase.Mobile.Views.Dynamic;
using ExpressBase.Mobile.Views.Shared;
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

        public async void InitNavigation()
        {
            EbMobileSettings settings = App.Settings.CurrentApplication?.AppSettings;

            IMasterPage master;

            if (settings != null && !string.IsNullOrEmpty(settings.DashBoardRefId))
            {
                EbMobilePage page = EbPageHelper.GetPage(settings.DashBoardRefId);

                if (page != null && page.Container is EbMobileDashBoard)
                {
                    master = new DashBoardRender(page);
                }
                else
                {
                    EbLog.Info("Default application dashboard not found, check object permissions");
                    master = new Home();
                }
            }
            else
            {
                master = new Home();
            }

            master.UpdateMasterLayout();

            await Detail.Navigation.PushAsync((Page)master);
        }

        protected override bool OnBackButtonPressed()
        {
            return base.OnBackButtonPressed();
        }
    }
}
