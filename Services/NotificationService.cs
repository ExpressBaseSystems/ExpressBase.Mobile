using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
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

        void UpdateNHRegisratation();
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

        private List<string> GetTags()
        {
            List<string> tags = new List<string> { "eb_pns_global" };

            if (App.Settings.Sid != null)
            {
                tags.Add(App.Settings.Sid);
            }

            if (App.Settings.CurrentUser != null)
            {
                tags.Add(App.Settings.CurrentUser.AuthId);
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

            EbLog.Write($"NH registration id:{pns_token}", LogTypes.MESSAGE);

            DeviceRegistration device = new DeviceRegistration()
            {
                Handle = pns_token,
                Tags = this.GetTags()
            };
            device.SetPlatform();

            return device;
        }

        public async void UpdateNHRegisratation()
        {
            try
            {
                DeviceRegistration device = this.GetDevice();

                if (!Utils.HasInternet || device.Handle == null) return;

                string azure_regid = Store.GetValue(AppConst.AZURE_REGID);

                if (string.IsNullOrEmpty(azure_regid))
                {
                    azure_regid = await this.GetNewRegistration();

                    if (azure_regid != null)
                    {
                        await Store.SetValueAsync(AppConst.AZURE_REGID, azure_regid);
                        await this.CreateOrUpdateRegistration(azure_regid, device);
                    }
                    EbLog.Write("NH REG ID:" + azure_regid, LogTypes.MESSAGE);
                }
                else
                {
                    EbLog.Write("NH REG ID:" + azure_regid, LogTypes.MESSAGE);

                    bool status = await this.CreateOrUpdateRegistration(azure_regid, device);

                    if (!status)
                    {
                        azure_regid = await this.GetNewRegistration();
                        if (azure_regid != null)
                        {
                            await Store.SetValueAsync(AppConst.AZURE_REGID, azure_regid);
                            await this.CreateOrUpdateRegistration(azure_regid, device);
                        }
                        EbLog.Write("NH REG ID:" + azure_regid + "after expire and new id", LogTypes.MESSAGE);
                    }
                }
            }
            catch (Exception ex)
            {
                EbLog.Write("Failed to update NHub registration" + ex.Message);
            }
        }
    }
}
