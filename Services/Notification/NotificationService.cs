using ExpressBase.Mobile.Configuration;
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
    public class NotificationService : BaseService, INotificationService
    {
        private static NotificationService instance;

        public static NotificationService Instance => instance ??= new NotificationService();

        public EbAppVendors AppVendor { set; get; }

        public NotificationService() : base(true)
        {
            AppVendor = EbBuildConfig.GetVendor();
        }

        public async Task<string> GetAzureTokenAsync()
        {
            string token = string.Empty;

            RestRequest request = new RestRequest("api/notifications/get_registration_id", Method.GET);
            request.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
            request.AddHeader(AppConst.RTOKEN, App.Settings.RToken);

            IRestResponse response = await HttpClient.ExecuteAsync<string>(request);
            if (response.IsSuccessful)
            {
                token = response.Content.Trim('"');
            }
            return token;
        }

        public async Task<EbNFRegisterResponse> CreateOrUpdateRegistration(string regid, DeviceRegistration device)
        {
            RestRequest request = new RestRequest("api/notifications/register", Method.POST);

            request.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
            request.AddHeader(AppConst.RTOKEN, App.Settings.RToken);

            request.AddParameter("regid", regid);
            request.AddParameter("device", JsonConvert.SerializeObject(device));

            try
            {
                IRestResponse response = await HttpClient.ExecuteAsync(request);
                if (response.IsSuccessful)
                {
                    return JsonConvert.DeserializeObject<EbNFRegisterResponse>(response.Content);
                }
            }
            catch (Exception ex)
            {
                EbLog.Error("Failed to update or create registration::" + ex.Message);
            }
            return null;
        }

        public async Task<bool> UnRegisterAsync(string regid)
        {
            RestRequest request = new RestRequest("api/notifications/delete_registration", Method.DELETE);
            request.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
            request.AddHeader(AppConst.RTOKEN, App.Settings.RToken);

            request.AddParameter("regid", regid);

            IRestResponse response = await HttpClient.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                return true;
            }
            return false;
        }

        private List<string> GetTags()
        {
            List<string> tags = new List<string> { $"global:eb_pns_tag_{AppVendor}" };

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
                Tags = this.GetTags(),
                Vendor = AppVendor
            };

            return device;
        }

        public async Task UpdateNHRegistration()
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
                    var resp = await this.CreateOrUpdateRegistration(azure_regid, device);

                    if (resp == null || !resp.Status)
                    {
                        azure_regid = await this.GetNewRegistration();

                        resp = await this.CreateOrUpdateRegistration(azure_regid, device);
                    }

                    if (resp != null) EbLog.Info(resp.Message);
                }

                EbLog.Info("NH REG ID:" + azure_regid);
            }
            catch (Exception ex)
            {
                EbLog.Error("Failed to update NHub registration" + ex.Message);
            }
        }

        public async Task UnRegisterCurrent()
        {
            string registration = Store.GetValue(AppConst.AZURE_REGID);

            if (!string.IsNullOrEmpty(registration))
            {
                try
                {
                    Store.Remove(AppConst.AZURE_REGID);

                    bool status = await UnRegisterAsync(registration);
                    EbLog.Info($"AZURE_REGID delete api status: '{status}'");
                }
                catch (Exception ex)
                {
                    EbLog.Info("Error at unregister AZURE_REGID api");
                    EbLog.Error(ex.Message);
                }
            }
            else
            {
                EbLog.Error("AZURE_REGID empty");
            }
        }
    }
}
