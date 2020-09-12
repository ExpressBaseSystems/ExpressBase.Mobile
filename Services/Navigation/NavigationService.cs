using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Views;
using ExpressBase.Mobile.Views.Base;
using ExpressBase.Mobile.Views.Dynamic;
using ExpressBase.Mobile.Views.Shared;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Services
{
    public class NavigationService
    {
        //login with new stack
        public static async Task LoginWithNS()
        {
            App.RootMaster = null;
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
            try
            {
                var stack = Application.Current.MainPage.Navigation.NavigationStack;

                if (stack.Any())
                {
                    Page last = Application.Current.MainPage.Navigation.NavigationStack.LastOrDefault();

                    await Application.Current.MainPage.Navigation.PushAsync(page);
                    if (last != null)
                        Application.Current.MainPage.Navigation.RemovePage(last);
                }
            }
            catch (Exception ex)
            {
                EbLog.Info("failed to replace mainpage top page");
                EbLog.Error(ex.Message);
            }
        }

        public static async Task ReplaceRootTopAsync(Page page)
        {
            try
            {
                var stack = App.RootMaster.Detail.Navigation.NavigationStack;

                if (stack.Any())
                {
                    Page last = App.RootMaster.Detail.Navigation.NavigationStack.LastOrDefault();

                    await App.RootMaster.Detail.Navigation.PushAsync(page);
                    if (last != null)
                        App.RootMaster.Detail.Navigation.RemovePage(last);
                }
            }
            catch (Exception ex)
            {
                EbLog.Info("failed to replace rootmaster top page");
                EbLog.Error(ex.Message);
            }
        }

        public static async Task LoginAction()
        {
            await App.RootMaster.Detail.Navigation.PushModalAsync(new LoginAction());
        }

        public static void UpdateViewStack()
        {
            try
            {
                IReadOnlyList<Page> stack = App.RootMaster.Detail.Navigation.NavigationStack;

                foreach (var page in stack)
                {
                    if (page is IRefreshable iref && iref.CanRefresh())
                    {
                        iref.UpdateRenderStatus();
                    }
                }
            }
            catch (Exception ex)
            {
                EbLog.Error("Failed to auto refresh listview");
                EbLog.Error(ex.Message);
            }
        }

        public static void RefreshCurrentPage()
        {
            Page current = App.RootMaster.Detail.Navigation.NavigationStack.Last();

            if (current != null && current is IRefreshable iref)
            {
                iref.RefreshPage();
            }
        }

        public static async Task NavigateButtonLinkPage(EbMobileButton button, EbDataRow row, EbMobilePage page)
        {
            ContentPage renderer = null;
            var container = page.Container;

            if (container is EbMobileForm)
            {
                if (button.FormMode == WebFormDVModes.New_Mode)
                    renderer = new FormRender(page, button.LinkFormParameters, row);
                else
                {
                    try
                    {
                        var map = button.FormId;
                        if (map == null)
                        {
                            EbLog.Info("form id should be set");
                            throw new Exception("Form rendering exited! due to null value for 'FormId'");
                        }
                        else
                        {
                            int id = Convert.ToInt32(row[map.ColumnName]);
                            if (id <= 0)
                            {
                                EbLog.Info("id has ivalid value" + id);
                                throw new Exception("Form rendering exited! due to invalid id");
                            }
                            renderer = new FormRender(page, id);
                        }
                    }
                    catch (Exception ex)
                    {
                        EbLog.Error(ex.Message);
                    }
                }
            }
            else if (container is EbMobileVisualization)
            {
                renderer = new ListRender(page, row);
            }

            if (renderer != null)
                await App.RootMaster.Detail.Navigation.PushAsync(renderer);
        }
    }
}
