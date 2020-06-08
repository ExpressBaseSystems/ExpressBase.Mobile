using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.Services
{
    public interface IMyActionsService
    {
        Task<MyActionsResponse> GetMyActionsAsync();

        Task<EbStageInfo> GetMyActionInfoAsync(int stageid, string refid, int dataid);
    }

    public class MyActionsService : IMyActionsService
    {
        public async Task<MyActionsResponse> GetMyActionsAsync()
        {
            try
            {
                RestClient client = new RestClient(App.Settings.RootUrl);

                RestRequest request = new RestRequest("api/get_actions", Method.GET);

                // auth Headers for api
                request.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
                request.AddHeader(AppConst.RTOKEN, App.Settings.RToken);

                IRestResponse iresp = await client.ExecuteAsync(request);
                return JsonConvert.DeserializeObject<MyActionsResponse>(iresp.Content);
            }
            catch (Exception ex)
            {
                EbLog.Write("GetMyActionsAsync---" + ex.Message);
            }
            return new MyActionsResponse();
        }

        public async Task<EbStageInfo> GetMyActionInfoAsync(int stageid, string refid, int dataid)
        {
            try
            {
                RestClient client = new RestClient(App.Settings.RootUrl);

                RestRequest request = new RestRequest("api/get_action_info", Method.GET);

                // auth Headers for api
                request.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
                request.AddHeader(AppConst.RTOKEN, App.Settings.RToken);

                request.AddParameter("stageid", stageid);
                request.AddParameter("refid", refid);
                request.AddParameter("dataid", dataid);

                IRestResponse iresp = await client.ExecuteAsync(request);
                return JsonConvert.DeserializeObject<EbStageInfo>(iresp.Content);
            }
            catch (Exception ex)
            {
                EbLog.Write("RestService.GetMyActions---" + ex.Message);
            }
            return new EbStageInfo();
        }
    }
}
