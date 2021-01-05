using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpressBase.Mobile.Services
{
    public class BaseService
    {
        protected RestClient HttpClient { set; get; }

        public BaseService() { }

        public BaseService(bool httpClientInjection)
        {
            if (httpClientInjection)
            {
                HttpClient = new RestClient(App.Settings.RootUrl);
            }
        }

        public BaseService(string url)
        {
            HttpClient = new RestClient(url);
        }

        public RestClient GetRestClient(string url)
        {
            return new RestClient(url);
        }

        protected RestRequest GetRequest(string url, Method method)
        {
            RestRequest restRequest = new RestRequest(url, method);

            restRequest.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
            restRequest.AddHeader(AppConst.RTOKEN, App.Settings.RToken);

            return restRequest;
        }

        protected void LogHttpResponse(IRestResponse resp)
        {
            EbLog.Error(resp.StatusCode.ToString());
            EbLog.Error(resp.ErrorMessage);
            EbLog.Error(resp.Content ?? "http response content empty");
        }
    }
}
