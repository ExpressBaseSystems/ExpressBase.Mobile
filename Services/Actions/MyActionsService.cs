using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.Services
{
    public class MyActionsService : IMyActionsService
    {
        public async Task<MyActionsResponse> GetMyActionsAsync()
        {
            try
            {
                RestClient client = new RestClient(App.Settings.RootUrl);

                RestRequest request = new RestRequest(ApiConstants.GET_ACTIONS, Method.GET);

                // auth Headers for api
                request.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
                request.AddHeader(AppConst.RTOKEN, App.Settings.RToken);

                IRestResponse iresp = await client.ExecuteAsync(request);
                return JsonConvert.DeserializeObject<MyActionsResponse>(iresp.Content);
            }
            catch (Exception ex)
            {
                EbLog.Info("Error in get_actions api");
                EbLog.Error(ex.Message);
            }
            return new MyActionsResponse();
        }

        public async Task<EbStageInfo> GetMyActionInfoAsync(int stageid, string refid, int dataid)
        {
            try
            {
                RestClient client = new RestClient(App.Settings.RootUrl);

                RestRequest request = new RestRequest(ApiConstants.GET_ACTION_INFO, Method.GET);

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
                EbLog.Error("Error in action info api");
                EbLog.Error(ex.Message);
            }
            return new EbStageInfo();
        }
    }
}
