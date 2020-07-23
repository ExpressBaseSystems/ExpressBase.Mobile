using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Views;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Services
{
    public class NAVService
    {
        public static async Task NavigateToLogin()
        {
            Application.Current.MainPage = new NavigationPage()
            {
                BarBackgroundColor = App.Settings.Vendor.GetPrimaryColor(),
                BarTextColor = Color.White
            };

            if (App.Settings.LoginType == LoginType.SSO)
                await Application.Current.MainPage.Navigation.PushAsync(new Login());
            else
                await Application.Current.MainPage.Navigation.PushAsync(new LoginByOTP());
        }

        public static bool IsTokenExpired(string rtoken)
        {
            var jwtToken = new JwtSecurityToken(rtoken);

            if (DateTime.Compare(jwtToken.ValidTo, DateTime.Now) < 0)
                return true;
            else
                return false;
        }

        public static async Task NavigateIfTokenExpiredAsync()
        {
            if (IsTokenExpired(App.Settings.RToken))
            {
                await NavigateToLogin();
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
    }
}
