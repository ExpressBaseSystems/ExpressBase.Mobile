using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Views.Shared;
using System;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Views
{
    public class RootMaster : MasterDetailPage
    {
        public RootMaster(Type pageType)
        {
            try
            {
                Master = new SideBar(); 
                Detail = new NavigationPage
                {
                    BarBackgroundColor = App.Settings.Vendor.GetPrimaryColor(),
                    BarTextColor = Color.White
                };

                Detail.Navigation.PushAsync((Page)Activator.CreateInstance(pageType));
            }
            catch(Exception ex)
            {
                EbLog.Write(ex.Message);
            }
        }

        protected override bool OnBackButtonPressed()
        {
            return base.OnBackButtonPressed();
        }
    }
}
