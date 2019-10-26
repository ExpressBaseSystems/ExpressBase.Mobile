using ExpressBase.Mobile.Common.Structures;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Views.Shared;
using System;
using System.Collections.Generic;
using System.Text;
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
                Store.Remove(Constants.BTOKEN);
                Store.Remove(Constants.RTOKEN);
            }
            else if (item.LinkType == "app_switch")
            {
                Store.Remove(Constants.APPID);
            }
            else if (item.LinkType == "sln_switch")
            {
                Store.Remove(Constants.SID);
                Store.Remove(Constants.APPID);
                Store.Remove(Constants.USERNAME);
                Store.Remove(Constants.PASSWORD);
                Store.Remove(Constants.BTOKEN);
                Store.Remove(Constants.RTOKEN);
            }
            Application.Current.MainPage = new NavigationPage((Page)Activator.CreateInstance(item.TargetType));
        }
    }
}
