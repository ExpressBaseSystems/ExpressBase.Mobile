using ExpressBase.Mobile.Common.Structures;
using ExpressBase.Mobile.Models;
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

            Sidebar.listView.ItemSelected += OnItemSelected;

            Detail = new NavigationPage((Page)Activator.CreateInstance(pageType));
        }

        void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var item = e.SelectedItem as MasterPageItem;
            Application.Current.MainPage = new NavigationPage((Page)Activator.CreateInstance(item.TargetType));
        }
    }
}
