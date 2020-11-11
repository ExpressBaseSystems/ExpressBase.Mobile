using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Views;
using ExpressBase.Mobile.Views.Base;
using ExpressBase.Mobile.Views.Login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Services.Navigation
{
    public class NavigationService : INavigationService
    {
        public static NavigationService Instance { get; set; }

        private Application CurrentApplication => Application.Current;

        public NavigationService()
        {
            Instance = this;
        }

        public async Task InitializeAppAsync(EbNFData payload)
        {
            App.Settings.InitializeConfig();

            await InitializeNavigation();

            if (payload != null)
            {
                await InitRecievedIntentAction(payload);
            }
        }

        public async Task NavigateByRenderer(Page page)
        {
            if (App.RootMaster == null)
                await NavigateAsync(page);
            else
                await NavigateMasterAsync(page);
        }

        public async Task NavigateModalByRenderer(Page page)
        {
            if (App.RootMaster == null)
                await NavigateModalAsync(page);
            else
                await NavigateMasterModalAsync(page);
        }

        public async Task PopByRenderer(bool animation)
        {
            if (App.RootMaster == null)
                await PopAsync(animation);
            else
                await PopMasterAsync(animation);
        }

        public async Task PopModalByRenderer(bool animation)
        {
            if (App.RootMaster == null)
                await PopModalAsync(animation);
            else
                await PopMasterModalAsync(animation);
        }

        public async Task NavigateAsync(Page page)
        {
            await CurrentApplication.MainPage.Navigation.PushAsync(page);
        }

        public async Task NavigateModalAsync(Page page)
        {
            await CurrentApplication.MainPage.Navigation.PushModalAsync(page);
        }

        public async Task NavigateMasterAsync(Page page)
        {
            await App.RootMaster.Detail.Navigation.PushAsync(page);
        }

        public async Task NavigateMasterModalAsync(Page page)
        {
            await App.RootMaster.Detail.Navigation.PushModalAsync(page);
        }

        public async Task PopAsync(bool animate)
        {
            await CurrentApplication.MainPage.Navigation.PopAsync(animate);
        }

        public async Task PopModalAsync(bool animate)
        {
            await CurrentApplication.MainPage.Navigation.PopModalAsync(animate);
        }

        public async Task PopMasterAsync(bool animate)
        {
            await App.RootMaster.Detail.Navigation.PopAsync(animate);
        }

        public async Task PopMasterModalAsync(bool animate)
        {
            await App.RootMaster.Detail.Navigation.PopModalAsync(animate);
        }

        public async Task PopToRootAsync(bool animation)
        {
            await App.RootMaster.Detail.Navigation.PopToRootAsync(animation);
        }

        private async Task InitializeNavigation()
        {
            CurrentApplication.MainPage = new NavigationPage();

            await App.Settings.InitializeSettings();

            if (App.Settings.Sid == null)
            {
                if (Utils.Solutions.Any())
                    await NavigateAsync(new MySolutions());
                else
                    await NavigateAsync(new NewSolution());
            }
            else
            {
                if (App.Settings.RToken == null || IdentityService.IsTokenExpired())
                {
                    await NavigateToLogin();
                    return;
                }
                else
                {
                    if (App.Settings.AppId <= 0)
                        await NavigateAsync(new MyApplications());
                    else
                    {
                        App.RootMaster = new RootMaster(typeof(Home));
                        CurrentApplication.MainPage = App.RootMaster;
                    }
                }
            }
        }

        public async Task InitRecievedIntentAction(EbNFData payload)
        {
            if (payload.Link == null && App.RootMaster == null)
            {
                EbLog.Info("Intentaction aborted, link and rootmaster null");
                return;
            }

            EbNFLink link = payload.Link;
            try
            {
                if (link.LinkType == EbNFLinkTypes.Action)
                    await ResolveAction(link);
                else if (link.LinkType == EbNFLinkTypes.Page)
                    await ResolveRedirection(link);
            }
            catch (Exception ex)
            {
                EbLog.Info("Unknown error at indent action");
                EbLog.Error(ex.Message);
            }
        }

        public async Task NavigateToLogin(bool new_navigation = false)
        {
            if (new_navigation)
            {
                App.RootMaster = null;
                Application.Current.MainPage = new NavigationPage();
            }

            if (App.Settings.LoginType == LoginType.SSO)
                await CurrentApplication.MainPage.Navigation.PushAsync(new LoginByOTP());
            else
                await CurrentApplication.MainPage.Navigation.PushAsync(new LoginByPassword());
        }

        public void UpdateViewStack()
        {
            try
            {
                IReadOnlyList<Page> stack = App.RootMaster.Detail.Navigation.NavigationStack;

                foreach (Page page in stack)
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

        public void RefreshCurrentPage()
        {
            Page current = App.RootMaster.Detail.Navigation.NavigationStack.Last();

            if (current != null && current is IRefreshable iref)
            {
                iref.RefreshPage();
            }
        }

        private async Task ResolveAction(EbNFLink link)
        {
            if (link.ActionId != 0)
            {
                await App.Navigation.NavigateMasterAsync(new DoAction(link.ActionId));
                EbLog.Info("Navigated to action submit : actionid =" + link.ActionId);
            }
            else
                EbLog.Info("ActionId Empty, Intent action failed");
        }

        private async Task ResolveRedirection(EbNFLink link)
        {
            if (string.IsNullOrEmpty(link.LinkRefId))
            {
                EbLog.Info("Intentaction link type is page but linkrefid null");
                return;
            }
            EbMobilePage page = EbPageFinder.GetPage(link.LinkRefId);

            if (page != null)
            {
                EbLog.Info("Intentaction page rendering :" + page.DisplayName);

                ContentPage renderer = EbPageFinder.GetPageByContainer(page);
                await App.Navigation.NavigateMasterAsync(renderer);
            }
            else
                EbLog.Info("Intentaction page not found for linkrefid:" + link.LinkRefId);
        }

        public Page GetCurrentPage()
        {
            Page navigator = App.RootMaster == null ? CurrentApplication.MainPage : App.RootMaster.Detail;

            if (navigator is NavigationPage navp)
            {
                return navp.CurrentPage;
            }
            return null;
        }
    }
}
