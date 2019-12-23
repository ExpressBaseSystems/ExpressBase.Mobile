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
            Detail = new NavigationPage();

            Detail.Navigation.PushAsync((Page)Activator.CreateInstance(pageType));
        }
    }
}
