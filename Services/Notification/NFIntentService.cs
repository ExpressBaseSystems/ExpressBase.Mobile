using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Views;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Services.Notification
{
    public class NFIntentService
    {
        public static async Task Resolve(EbNFData data)
        {
            if (data.Link == null && App.RootMaster == null)
            {
                EbLog.Info("Intentaction aborted, link and rootmaster null");
                return;
            }
            EbNFLink link = data.Link;
            try
            {
                if (link.LinkType == EbNFLinkTypes.Action)
                {
                    await ResolveAction(link);
                }
                else if (link.LinkType == EbNFLinkTypes.Page)
                {
                    await ResolveRedirection(link);
                }
            }
            catch (Exception ex)
            {
                EbLog.Info("Unknown error at indent action");
                EbLog.Error(ex.Message);
                EbLog.Error(ex.StackTrace);
            }
        }

        private static async Task ResolveAction(EbNFLink link)
        {
            if (link.ActionId != 0)
            {
                await App.RootMaster.Detail.Navigation.PushAsync(new DoAction(link.ActionId));
                EbLog.Info("Navigated to action submit : actionid =" + link.ActionId);
            }
            else
            {
                EbLog.Info("ActionId Empty, Intent action failed");
            }
        }

        private static async Task ResolveRedirection(EbNFLink link)
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
                await App.RootMaster.Detail.Navigation.PushAsync(renderer);
            }
            else
                EbLog.Info("Intentaction page not found for linkrefid:" + link.LinkRefId);
        }
    }
}
