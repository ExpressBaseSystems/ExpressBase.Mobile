using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Helpers
{
    public class LoaderHelper
    {
        public static void ShowLoader(Page Page,Type PageType)
        {
            if(Page is MasterDetailPage)
            {
                (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushModalAsync((Page)Activator.CreateInstance(PageType));
            }
            else
            {
                Page.Navigation.PushModalAsync((Page)Activator.CreateInstance(PageType));
            }
        }

        public static void HideLoader(Page Page)
        {
            if (Page is MasterDetailPage)
            {
                (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PopModalAsync();
            }
            else
            {
                Page.Navigation.PopModalAsync();
            }
        }
    }
}
