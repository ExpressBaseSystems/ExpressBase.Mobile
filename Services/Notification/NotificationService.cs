using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.Services
{
    public class NotificationService : INotificationService
    {
        private static NotificationService instance;

        public static NotificationService Instance => instance ??= new NotificationService();

        public async Task<string> GetAzureTokenAsync()
        {
            string token = string.Empty;

            RestClient client = new RestClient(App.Settings.RootUrl);

            RestRequest request = new RestRequest("api/notifications/register", Method.GET);
            request.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
            request.AddHeader(AppConst.RTOKEN, App.Settings.RToken);

            IRestResponse response = await client.ExecuteAsync<string>(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                token = response.Content.Trim('"');
            }
            return token;
        }

        public async Task<bool> CreateOrUpdateRegistration(string regid, DeviceRegistration device)
        {

            RestClient client = new RestClient(App.Settings.RootUrl);
            RestRequest request = new RestRequest("api/notifications/enable", Method.POST);

            request.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
            request.AddHeader(AppConst.RTOKEN, App.Settings.RToken);

            request.AddParameter("regid", regid);
            request.AddParameter("device", JsonConvert.SerializeObject(device));

            try
            {
                IRestResponse response = await client.ExecuteAsync(request);
                if (response.StatusCode == HttpStatusCode.OK)
                    return true;
            }
            catch (Exception ex)
            {
                EbLog.Error("Failed to update or create registration::" + ex.Message);
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

        private List<string> GetTags()
        {
            List<string> tags = new List<string> { "global:eb_pns_tag" };

            if (App.Settings.Sid != null)
            {
                tags.Add("solution:" + App.Settings.Sid);
            }

            if (App.Settings.CurrentUser != null)
            {
                tags.Add("authid:" + App.Settings.CurrentUser.AuthId);
            }

            return tags;
        }

        private async Task<string> GetNewRegistration()
        {
            string regid = await this.GetAzureTokenAsync();

            if (regid != null)
            {
                await Store.SetValueAsync(AppConst.AZURE_REGID, regid);
            }
            return regid;
        }

        private DeviceRegistration GetDevice()
        {
            string pns_token = Store.GetValue(AppConst.PNS_TOKEN);

            EbLog.Info("DEVICE TOKEN:" + pns_token);

            DeviceRegistration device = new DeviceRegistration()
            {
                Handle = pns_token,
                Tags = this.GetTags()
            };
            device.SetPlatform();

            return device;
        }

        public async Task UpdateNHRegisratation()
        {
            if (!Utils.HasInternet)
                return;

            try
            {
                DeviceRegistration device = this.GetDevice();

                if (device.Handle == null) return;

                string azure_regid = Store.GetValue(AppConst.AZURE_REGID);

                if (string.IsNullOrEmpty(azure_regid))
                {
                    azure_regid = await this.GetNewRegistration();

                    await this.CreateOrUpdateRegistration(azure_regid, device);
                }
                else
                {
                    bool status = await this.CreateOrUpdateRegistration(azure_regid, device);

                    if (!status)
                    {
                        azure_regid = await this.GetNewRegistration();
                        await this.CreateOrUpdateRegistration(azure_regid, device);
                    }
                }

                EbLog.Info("NH REG ID:" + azure_regid);
            }
            catch (Exception ex)
            {
                EbLog.Error("Failed to update NHub registration" + ex.Message);
            }
        }
    }
}
