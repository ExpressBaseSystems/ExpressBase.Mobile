using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.Services
{
    public class MenuServices : BaseService, IMenuServices
    {
        public async Task<List<MobilePagesWraper>> GetDataAsync()
        {
            List<MobilePagesWraper> objectList = App.Settings.MobilePages ?? new List<MobilePagesWraper>();

            if (App.Settings.CurrentUser.IsAdmin)
            {
                EbLog.Info($"logged in as [admin], service returning [{objectList.Count}] objects");
                return objectList;
            }
            else
            {
                EbMobileSettings settings = App.Settings.CurrentApplication.AppSettings;

                if (settings != null && settings.HasMenuPreloadApi)
                {
                    if (Utils.HasInternet)
                    {
                        EbLog.Info("Network connection is live and [preload api] connected");
                        objectList = await GetFromMenuPreload(settings.MenuApi);
                    }
                    else
                    {
                        Utils.Alert_NoInternet();
                    }
                }
            }
            //filter all pages with respect to current location
            List<MobilePagesWraper> filter = App.Settings.CurrentUser.FilterByLocation(objectList);

            return filter;
        }

        //HomeViewModel.cs 
        //public async Task<List<MobilePagesWraper>> UpdateDataAsync()
        //{
        //    List<MobilePagesWraper> collection = null;
        //    try
        //    {
        //        EbMobileSolutionData data = await App.Settings.GetSolutionDataAsync(false, 6000, async status =>
        //        {
        //            collection = await this.GetDataAsync();

        //            if (status == ResponseStatus.TimedOut)
        //            {
        //                Utils.Alert_SlowNetwork();
        //                EbLog.Info("[solutiondata api] raised timeout in UpdateDataAsync");
        //            }
        //            else
        //            {
        //                Utils.Alert_NetworkError();
        //                EbLog.Info("[solutiondata api] raised network error in UpdateDataAsync");
        //            }
        //        });

        //        if (data != null)
        //        {
        //            App.Settings.MobilePages = App.Settings.CurrentApplication.MobilePages;
        //            App.Settings.WebObjects = App.Settings.CurrentApplication.WebObjects;
        //            collection = await this.GetDataAsync();
        //            Utils.Toast("Refreshed");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        EbLog.Error("menu update failed :: " + ex.Message);
        //    }
        //    return collection ?? new List<MobilePagesWraper>();
        //}

        public async Task<List<MobilePagesWraper>> GetFromMenuPreload(EbApiMeta apimeta)
        {
            RestClient client = new RestClient(App.Settings.RootUrl);
            RestRequest request = new RestRequest($"api/{apimeta.Name}/{apimeta.Version}", Method.GET);

            request.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
            request.AddHeader(AppConst.RTOKEN, App.Settings.RToken);

            MenuPreloadResponse resp = null;
            try
            {
                IRestResponse response = await client.ExecuteAsync(request);
                if (response.IsSuccessful)
                {
                    resp = JsonConvert.DeserializeObject<MenuPreloadResponse>(response.Content);
                }
            }
            catch (Exception ex)
            {
                EbLog.Error("Error on menu preload api request :: " + ex.Message);
            }

            List<MobilePagesWraper> pages = new List<MobilePagesWraper>();

            if (resp != null && resp.Result != null)
            {
                List<MobilePagesWraper> all = App.Settings.MobilePages ?? new List<MobilePagesWraper>();

                foreach (string objName in resp.Result)
                {
                    MobilePagesWraper wraper = all.Find(item => item.Name == objName);

                    if (wraper != null)
                    {
                        pages.Add(wraper);
                    }
                }
            }
            return pages;
        }
    }
}
