using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.Services
{
    public interface INotificationService
    {
        Task<string> GetAzureTokenAsync();

        Task<bool> CreateOrUpdateRegistration(string regId, DeviceRegistration device);

        Task<bool> UnRegisterAsync(string regId);
    }

    public class NotificationService : INotificationService
    {
        private static NotificationService instance;

        public static NotificationService Instance => instance ?? (instance = new NotificationService());

        public async Task<string> GetAzureTokenAsync()
        {
            string token = string.Empty;

            RestClient client = new RestClient(App.Settings.RootUrl);

            RestRequest request = new RestRequest("api/notifications/register", Method.GET);
            request.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
            request.AddHeader(AppConst.RTOKEN, App.Settings.RToken);

            IRestResponse response = await client.ExecuteAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                token = response.Content;
            }
            return token;
        }

        public async Task<bool> CreateOrUpdateRegistration(string regid, DeviceRegistration device)
        {
            RestClient client = new RestClient(App.Settings.RootUrl);

            RestRequest request = new RestRequest("api/notifications/enable", Method.PUT);
            request.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
            request.AddHeader(AppConst.RTOKEN, App.Settings.RToken);

            request.AddParameter("regid", regid);
            request.AddParameter("device", JsonConvert.SerializeObject(device));

            IRestResponse response = await client.ExecuteAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> UnRegisterAsync(string regid)
        {
            RestClient client = new RestClient(App.Settings.RootUrl);

            RestRequest request = new RestRequest("api/notifications/unregister", Method.DELETE);
            request.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
            request.AddHeader(AppConst.RTOKEN, App.Settings.RToken);

            request.AddParameter("regid", regid);

            IRestResponse response = await client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                return true;
            }
            return false;
        }
    }
}
