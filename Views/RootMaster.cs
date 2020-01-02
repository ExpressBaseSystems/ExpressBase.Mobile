using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Views.Shared;
using System;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Views
{
    class RootMaster : MasterDetailPage
    {
        SideBar Sidebar;

        public RootMaster(Type pageType)
        {
            Sidebar = new SideBar();
            Master = Sidebar;
            Detail = new NavigationPage
            {
                BarBackgroundColor = Color.FromHex("315eff"),
                BarTextColor = Color.White
            };

            Detail.Navigation.PushAsync((Page)Activator.CreateInstance(pageType));
        }
    }
}
