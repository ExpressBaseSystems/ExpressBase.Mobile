using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Services
{
    public interface IMenuServices
    {
        Task<List<MobilePagesWraper>> GetDataAsync(int appid, int locid);

        Task DeployFormTables(List<MobilePagesWraper> objlist);
    }

    public class MenuServices : IMenuServices
    {
        public async Task<List<MobilePagesWraper>> GetDataAsync(int appid, int locid)
        {
            List<MobilePagesWraper> _objlist = Store.GetJSON<List<MobilePagesWraper>>(AppConst.OBJ_COLLECTION);

            if (_objlist == null)
            {
                if (!Settings.HasInternet)
                {
                    DependencyService.Get<IToast>().Show("You are not connected to internet");
                    return new List<MobilePagesWraper>();
                }

                MobilePageCollection menuData = await GetFromApiAsync(Settings.AppId, Settings.LocationId);
                _objlist = menuData.Pages;
                await Store.SetJSONAsync(AppConst.OBJ_COLLECTION, _objlist);

                await CommonServices.Instance.LoadLocalData(menuData.Data);
            }
            return _objlist;
        }

        public async Task<MobilePageCollection> GetFromApiAsync(int appid, int locid)
        {
            try
            {
                var client = new RestClient(Settings.RootUrl);

                RestRequest request = new RestRequest("api/objects_by_app", Method.GET);
                request.AddHeader(AppConst.BTOKEN, Settings.BToken);
                request.AddHeader(AppConst.RTOKEN, Settings.RToken);

                request.AddParameter("appid", appid);
                request.AddParameter("locid", locid);
                request.AddParameter("pull_data", true);

                var response = await client.ExecuteAsync(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<MobilePageCollection>(response.Content);
                }
            }
            catch (Exception ex)
            {
                Log.Write("RestServices.GetEbObjects---" + ex.Message);
                return new MobilePageCollection();
            }
            return new MobilePageCollection();
        }

        public async Task DeployFormTables(List<MobilePagesWraper> objlist)
        {
            await Task.Run(() =>
            {
                foreach (MobilePagesWraper _p in objlist)
                {
                    EbMobilePage mpage = _p.ToPage();

                    if (mpage != null && mpage.Container is EbMobileForm)
                    {
                        if (mpage.NetworkMode != NetworkMode.Online)
                            (mpage.Container as EbMobileForm).CreateTableSchema();
                    }
                }
            });
        }
    }
}
