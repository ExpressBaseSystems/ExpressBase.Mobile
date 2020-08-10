using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Views;
using ExpressBase.Mobile.Views.Shared;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Services
{
    public class NAVService
    {
        //login with new stack
        public static async Task LoginWithNS()
        {
            Application.Current.MainPage = new NavigationPage()
            {
                BarBackgroundColor = App.Settings.Vendor.GetPrimaryColor(),
                BarTextColor = Color.White
            };

            if (App.Settings.LoginType == LoginType.SSO)
                await Application.Current.MainPage.Navigation.PushAsync(new LoginByOTP());
            else
                await Application.Current.MainPage.Navigation.PushAsync(new Login());
        }

        //login with current stack
        public static async Task LoginWithCS()
        {
            if (App.Settings.LoginType == LoginType.SSO)
                await Application.Current.MainPage.Navigation.PushAsync(new LoginByOTP());
            else
                await Application.Current.MainPage.Navigation.PushAsync(new Login());
        }

        public static bool IsTokenExpired(string rtoken)
        {
            JwtSecurityToken jwtToken = new JwtSecurityToken(rtoken);

            if (DateTime.Compare(jwtToken.ValidTo, DateTime.Now) < 0)
                return true;
            else
                return false;
        }

        public static async Task NavigateIfTokenExpiredAsync()
        {
            if (IsTokenExpired(App.Settings.RToken))
            {
                await LoginWithNS();
            }
        }

        public static async Task ReplaceTopAsync(Page page)
        {
            Page last = null;
            try
            {
                int length = Application.Current.MainPage.Navigation.NavigationStack.Count;
                last = Application.Current.MainPage.Navigation.NavigationStack[length - 1];
            }
            catch (Exception)
            {
                //raise any message if wanted
            }
            await Application.Current.MainPage.Navigation.PushAsync(page);
            if (last != null)
                Application.Current.MainPage.Navigation.RemovePage(last);
        }

        public static async Task LoginAction()
        {
            await App.RootMaster.Detail.Navigation.PushModalAsync(new LoginAction());
        }

        public static void UpdateRenderStatusLast()
        {
            try
            {
                IReadOnlyList<Page> stack = App.RootMaster.Detail.Navigation.NavigationStack;

                if (stack.Count <= 1) return;

                int currentIndex = stack.Count - 1;

                Page lastPage = stack[currentIndex - 1];

                if (lastPage is IRefreshable)
                {
                    (lastPage as IRefreshable).UpdateRenderStatus();
                }
            }
            catch(Exception ex)
            {
                EbLog.Write("Failed to auto refresh listview :" + ex.Message);
            }
        }
    }
}
