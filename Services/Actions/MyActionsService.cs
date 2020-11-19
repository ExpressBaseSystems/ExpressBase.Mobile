using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.Services
{
    public class MyActionsService : BaseService, IMyActionsService
    {
        public MyActionsService() : base(true) { }

        public async Task<MyActionsResponse> GetMyActionsAsync()
        {
            try
            {
                RestRequest request = new RestRequest(ApiConstants.GET_ACTIONS, Method.GET);

                // auth Headers for api
                request.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
                request.AddHeader(AppConst.RTOKEN, App.Settings.RToken);

                IRestResponse iresp = await HttpClient.ExecuteAsync(request);
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
                RestRequest request = new RestRequest(ApiConstants.GET_ACTION_INFO, Method.GET);

                // auth Headers for api
                request.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
                request.AddHeader(AppConst.RTOKEN, App.Settings.RToken);

                request.AddParameter("stageid", stageid);
                request.AddParameter("refid", refid);
                request.AddParameter("dataid", dataid);

                IRestResponse iresp = await HttpClient.ExecuteAsync(request);
                return JsonConvert.DeserializeObject<EbStageInfo>(iresp.Content);
            }
            catch (Exception ex)
            {
                EbLog.Error("Error in action info api");
                EbLog.Error(ex.Message);
            }
            return new EbStageInfo();
        }

        public async Task<ParticularActionResponse> GetParticularActionAsync(int id)
        {
            try
            {
                RestRequest request = new RestRequest(ApiConstants.GET_ACTIONS + $"/{id}", Method.GET);

                // auth Headers for api
                request.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
                request.AddHeader(AppConst.RTOKEN, App.Settings.RToken);

                IRestResponse iresp = await HttpClient.ExecuteAsync(request);
                return JsonConvert.DeserializeObject<ParticularActionResponse>(iresp.Content);
            }
            catch (Exception ex)
            {
                EbLog.Info("Error in get_actions api");
                EbLog.Error(ex.Message);
            }
            return null;
        }
    }
}
