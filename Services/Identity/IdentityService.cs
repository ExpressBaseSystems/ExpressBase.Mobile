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
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Services
{
    public class IdentityService : BaseService, IIdentityService
    {
        public IdentityService() : base(true) { }

        public async Task<ApiAuthResponse> AuthenticateAsync(string username, string password, bool anonymous = false)
        {
            ApiAuthResponse resp;
            try
            {
                RestRequest request = new RestRequest(ApiConstants.AUTHETICATE, Method.GET);

                request.AddParameter("username", username.Trim().ToLower());
                request.AddParameter("password", string.Concat(password, username).ToMD5());
                INativeHelper helper = DependencyService.Get<INativeHelper>();
                request.AddParameter("deviceid", helper.DeviceId);

                if (anonymous) request.AddParameter("anonymous", true);

                IRestResponse response = await HttpClient.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                    resp = JsonConvert.DeserializeObject<ApiAuthResponse>(response.Content);
                else
                    resp = new ApiAuthResponse { IsValid = false, Message = "Auth failed: " + response.ErrorMessage };
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
                resp = new ApiAuthResponse { IsValid = false, Message = "Auth failed: " + ex.Message };
            }
            return resp;
        }

        public async Task<ApiAuthResponse> AuthenticateSSOAsync(string username, string authid, string token)
        {
            ApiAuthResponse resp;
            try
            {
                RestRequest request = new RestRequest(ApiConstants.AUTHETICATE_SSO, Method.GET);

                request.AddParameter("username", username.Trim().ToLower());
                request.AddParameter("authid", authid);
                request.AddParameter("token", token);

                IRestResponse response = await HttpClient.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                    resp = JsonConvert.DeserializeObject<ApiAuthResponse>(response.Content);
                else
                    resp = new ApiAuthResponse { IsValid = false };
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
                resp = new ApiAuthResponse { IsValid = false };
            }
            return resp;
        }

        public async Task<ApiAuthResponse> SendAuthenticationOTP(string username, SignInOtpType type)
        {
            ApiAuthResponse resp;

            RestRequest request = new RestRequest(ApiConstants.SEND_AUTH_OTP, Method.POST);

            request.AddParameter("username", username);
            request.AddParameter("type", (int)type);

            try
            {
                IRestResponse response = await HttpClient.ExecuteAsync(request);

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
            RestRequest request = new RestRequest(ApiConstants.VERIFY_OTP, Method.POST);

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
                IRestResponse response = await HttpClient.ExecuteAsync(request);
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

        public async Task<ApiAuthResponse> VerifyUserByOTP(string token, string authid, string otp)
        {
            RestRequest request = new RestRequest(ApiConstants.VERIFY_OTP, Method.POST);

            request.AddParameter("token", token);
            request.AddParameter("authid", authid);
            request.AddParameter("otp", otp);
            request.AddParameter("user_verification", true);

            try
            {
                IRestResponse response = await HttpClient.ExecuteAsync(request);
                if (response.IsSuccessful)
                {
                    return JsonConvert.DeserializeObject<ApiAuthResponse>(response.Content);
                }
            }
            catch (Exception ex)
            {
                EbLog.Error("user verification failed :: " + ex.Message);
            }
            return null;
        }

        public async Task<ApiGenerateOTPResponse> GenerateOTP(ApiAuthResponse autheresp)
        {
            RestRequest request = new RestRequest(ApiConstants.RESEND_OTP, Method.POST);

            if (!string.IsNullOrEmpty(autheresp.BToken))
            {
                request.AddHeader(AppConst.BTOKEN, autheresp.BToken);
                request.AddHeader(AppConst.RTOKEN, autheresp.RToken);
            }

            request.AddParameter("token", autheresp.TwoFAToken);
            request.AddParameter("authid", autheresp.User.AuthId);

            try
            {
                IRestResponse response = await HttpClient.ExecuteAsync(request);
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

            foreach (SolutionInfo sol in solutions)
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
                    await App.Navigation.NavigateAsync(new WelcomPage(data));
                }
                else
                {
                    if (data.Applications.Count == 1)
                    {
                        AppData appdata = data.Applications[0];

                        await Store.SetJSONAsync(AppConst.CURRENT_APP, appdata);
                        App.Settings.CurrentApplication = appdata;
                        App.Settings.MobilePages = appdata.MobilePages;
                        App.Settings.WebObjects = appdata.WebObjects;

                        App.RootMaster = new RootMaster();
                        Application.Current.MainPage = App.RootMaster;
                    }
                    else
                    {
                        await App.Navigation.NavigateAsync(new MyApplications());
                    }
                }
            }
        }

        public bool IsValidOTP(string otp)
        {
            if (string.IsNullOrEmpty(otp))
                return false;

            char[] charArray = otp.ToCharArray();

            return (charArray.All(x => char.IsDigit(x)) && charArray.Length == 6);
        }

        public static bool IsTokenExpired()
        {
            if (App.Settings.RToken == null)
                return true;
            JwtSecurityToken jwtToken = new JwtSecurityToken(App.Settings.RToken);
            return (DateTime.Compare(jwtToken.ValidTo, DateTime.Now) < 0);
        }
    }
}
