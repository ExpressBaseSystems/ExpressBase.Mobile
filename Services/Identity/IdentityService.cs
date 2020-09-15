using ExpressBase.Mobile.Configuration;
using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Views;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Services
{
    public class IdentityService : IIdentityService
    {
        public RestClient Client { set; get; }

        public static IdentityService Instance => new IdentityService();

        public IdentityService()
        {
            Client = new RestClient(App.Settings.RootUrl);
        }

        public async Task<ApiAuthResponse> AuthenticateAsync(string username, string password)
        {
            ApiAuthResponse resp;
            try
            {
                RestRequest request = new RestRequest("api/auth", Method.GET);

                request.AddParameter("username", username.Trim());
                request.AddParameter("password", string.Concat(password, username).ToMD5());

                var response = await Client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                    resp = JsonConvert.DeserializeObject<ApiAuthResponse>(response.Content);
                else
                    resp = new ApiAuthResponse { IsValid = false };
            }
            catch (Exception ex)
            {
                EbLog.Error("Auth.TryAuthenticate---" + ex.Message);
                resp = new ApiAuthResponse { IsValid = false };
            }
            return resp;
        }

        public async Task<ApiAuthResponse> AuthenticateSSOAsync(string username, SignInOtpType type)
        {
            ApiAuthResponse resp;

            RestRequest request = new RestRequest("api/auth_sso", Method.POST);

            request.AddParameter("username", username);
            request.AddParameter("type", (int)type);

            try
            {
                var response = await Client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                    resp = JsonConvert.DeserializeObject<ApiAuthResponse>(response.Content);
                else
                    resp = new ApiAuthResponse { IsValid = false };
            }
            catch (Exception ex)
            {
                EbLog.Error("AuthenticateSSOAsync failed :: " + ex.Message);
                resp = new ApiAuthResponse { IsValid = false };
            }
            return resp;
        }

        public async Task<ApiAuthResponse> VerifyOTP(ApiAuthResponse autheresp, string otp)
        {
            RestRequest request = new RestRequest("api/verify_otp", Method.POST);

            if (!string.IsNullOrEmpty(autheresp.BToken))
            {
                request.AddHeader(AppConst.BTOKEN, autheresp.BToken);
                request.AddHeader(AppConst.RTOKEN, autheresp.RToken);
            }

            request.AddParameter("token", autheresp.TwoFAToken);
            request.AddParameter("authid", autheresp.UserAuthId);
            request.AddParameter("otp", otp);

            try
            {
                var response = await Client.ExecuteAsync(request);
                if (response.IsSuccessful)
                {
                    return JsonConvert.DeserializeObject<ApiAuthResponse>(response.Content);
                }
            }
            catch (Exception ex)
            {
                EbLog.Error("2FA verification failed :: " + ex.Message);
            }

            return null;
        }

        public async Task<ApiGenerateOTPResponse> GenerateOTP(ApiAuthResponse autheresp)
        {
            RestRequest request = new RestRequest("api/resend_otp", Method.POST);

            if (!string.IsNullOrEmpty(autheresp.BToken))
            {
                request.AddHeader(AppConst.BTOKEN, autheresp.BToken);
                request.AddHeader(AppConst.RTOKEN, autheresp.RToken);
            }

            request.AddParameter("token", autheresp.TwoFAToken);
            request.AddParameter("authid", autheresp.User.AuthId);

            try
            {
                var response = await Client.ExecuteAsync(request);
                if (response.IsSuccessful)
                {
                    return JsonConvert.DeserializeObject<ApiGenerateOTPResponse>(response.Content);
                }
            }
            catch (Exception ex)
            {
                EbLog.Error("2FA verification failed :: " + ex.Message);
            }

            return null;
        }

        public async Task UpdateAuthInfo(ApiAuthResponse resp, string username)
        {
            try
            {
                //primitive data
                await Store.SetValueAsync(AppConst.BTOKEN, resp.BToken);
                await Store.SetValueAsync(AppConst.RTOKEN, resp.RToken);

                App.Settings.RToken = resp.RToken;
                App.Settings.BToken = resp.BToken;
                App.Settings.CurrentUser = resp.User;

                await Store.SetJSONAsync(AppConst.USER_OBJECT, resp.User);

                if (resp.DisplayPicture != null)
                {
                    INativeHelper helper = DependencyService.Get<INativeHelper>();

                    string url = helper.NativeRoot + $"/{App.Settings.AppDirectory}/{ App.Settings.Sid.ToUpper()}/user.png";
                    File.WriteAllBytes(url, resp.DisplayPicture);
                }
            }
            catch (Exception ex)
            {
                EbLog.Error("UpdateAuthInfo---" + ex.Message);
            }
        }

        public async Task UpdateLastUser(string username, LoginType logintype = LoginType.SSO)
        {
            List<SolutionInfo> solutions = Utils.Solutions;
            SolutionInfo current = App.Settings.CurrentSolution;
            current.LastUser = username;
            current.LoginType = logintype;

            foreach (var sol in solutions)
            {
                if (sol.SolutionName == current.SolutionName && sol.RootUrl == current.RootUrl)
                {
                    sol.LastUser = username;
                    sol.LoginType = logintype;
                    break;
                }
            }
            await Store.SetJSONAsync(AppConst.SOLUTION_OBJ, current);
            await Store.SetJSONAsync(AppConst.MYSOLUTIONS, solutions);
        }

        public async Task Navigate(EbMobileSolutionData data)
        {
            if (data.Applications != null)
            {
                if (Utils.IsFreshStart)
                {
                    await Application.Current.MainPage.Navigation.PushAsync(new WelcomPage(data));
                }
                else
                {
                    if (data.Applications.Count == 1)
                    {
                        AppData appdata = data.Applications[0];

                        await Store.SetJSONAsync(AppConst.CURRENT_APP, appdata);
                        App.Settings.CurrentApplication = appdata;
                        App.Settings.MobilePages = appdata.MobilePages;

                        App.RootMaster = new RootMaster(typeof(Home));
                        Application.Current.MainPage = App.RootMaster;
                    }
                    else
                    {
                        await Application.Current.MainPage.Navigation.PushAsync(new MyApplications());
                    }
                }
            }
        }

        public bool IsValidOTP(string otp)
        {
            if (string.IsNullOrEmpty(otp))
                return false;

            char[] charArray = otp.ToCharArray();

            if (charArray.All(x => char.IsDigit(x)) && charArray.Length == 6)
                return true;
            else
                return false;
        }
    }
}
