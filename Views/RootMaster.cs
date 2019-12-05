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

            Sidebar.listView.ItemSelected += OnItemSelected;

            Detail.Navigation.PushAsync((Page)Activator.CreateInstance(pageType));
        }

        void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var item = e.SelectedItem as MasterPageItem;
            if (item.LinkType == "logout")
            {
                Store.Remove(AppConst.BTOKEN);
                Store.Remove(AppConst.RTOKEN);
                Store.Remove(AppConst.OBJ_COLLECTION);
                Store.Remove(AppConst.APP_COLLECTION);
            }
            else if (item.LinkType == "app_switch")
            {
                Store.Remove(AppConst.APPID);
                Store.Remove(AppConst.OBJ_COLLECTION);
            }
            else if (item.LinkType == "sln_switch")
            {
                Store.Remove(AppConst.SID);
                Store.Remove(AppConst.ROOT_URL);
                Store.Remove(AppConst.APPID);
                Store.Remove(AppConst.USERNAME);
                Store.Remove(AppConst.PASSWORD);
                Store.Remove(AppConst.BTOKEN);
                Store.Remove(AppConst.RTOKEN);
                Store.Remove(AppConst.OBJ_COLLECTION);
                Store.Remove(AppConst.APP_COLLECTION);
            }
            Application.Current.MainPage = new NavigationPage((Page)Activator.CreateInstance(item.TargetType));
        }
    }
}
