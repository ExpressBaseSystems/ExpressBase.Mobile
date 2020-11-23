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
    }
}
