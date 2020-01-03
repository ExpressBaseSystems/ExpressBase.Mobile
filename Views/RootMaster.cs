using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Views.Shared;
using System;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Views
{
    class RootMaster : MasterDetailPage
    {
        public RootMaster(Type pageType)
        {
            try
            {
                Master = new SideBar(); 
                Detail = new NavigationPage
                {
                    BarBackgroundColor = Color.FromHex("315eff"),
                    BarTextColor = Color.White
                };

                Detail.Navigation.PushAsync((Page)Activator.CreateInstance(pageType));
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
