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
using System.Net;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Services
{
    public interface IIdentityService
    {
        Task<ApiAuthResponse> AuthenticateAsync(string username, string password);

        Task<ApiAuthResponse> AuthenticateSSOAsync(string username, SignInOtpType type);

        Task<ImageSource> GetLogo(string sid);

        Task UpdateAuthInfo(ApiAuthResponse resp, string username, string password);

        Task UpdateLastUser(string username, LoginType logintype = LoginType.SSO);

        Task<ApiAuthResponse> VerifyOTP(ApiAuthResponse autheresp, string otp);

        Task<ApiGenerateOTPResponse> GenerateOTP(ApiAuthResponse autheresp);

        Task Navigate(EbMobileSolutionData data);
    }

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
                EbLog.Write("Auth.TryAuthenticate---" + ex.Message);
                resp = new ApiAuthResponse { IsValid = false };
            }
            return resp;
        }

        public async Task<ApiAuthResponse> AuthenticateSSOAsync(string username, SignInOtpType type)
        {
            ApiAuthResponse resp;

            RestRequest request = new RestRequest("api/auth_sso", Method.GET);

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
                EbLog.Write("AuthenticateSSOAsync failed :: " + ex.Message);
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
            request.AddParameter("authid", autheresp.User.AuthId);
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
                EbLog.Write("2FA verification failed :: " + ex.Message);
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
                EbLog.Write("2FA verification failed :: " + ex.Message);
            }

            return null;
        }

        public async Task<ImageSource> GetLogo(string sid)
        {
            try
            {
                await Task.Delay(1);

                INativeHelper helper = DependencyService.Get<INativeHelper>();

                var bytes = helper.GetPhoto($"ExpressBase/{sid}/logo.png");
                if (bytes != null)
                    return ImageSource.FromStream(() => new MemoryStream(bytes));
            }
            catch (Exception ex)
            {
                EbLog.Write("GetLogo" + ex.Message);
            }
            return null;
        }

        public async Task UpdateAuthInfo(ApiAuthResponse resp, string username, string password)
        {
            try
            {
                //primitive data
                await Store.SetValueAsync(AppConst.BTOKEN, resp.BToken);
                await Store.SetValueAsync(AppConst.RTOKEN, resp.RToken);

                App.Settings.RToken = resp.RToken;
                App.Settings.BToken = resp.BToken;
                App.Settings.CurrentUser = resp.User;

                await Store.SetValueAsync(AppConst.PASSWORD, password.Trim());
                await Store.SetJSONAsync(AppConst.USER_OBJECT, resp.User);

                if (resp.DisplayPicture != null)
                {
                    INativeHelper helper = DependencyService.Get<INativeHelper>();
                    string url = helper.NativeRoot + $"/ExpressBase/{ App.Settings.Sid.ToUpper()}/user.png";
                    File.WriteAllBytes(url, resp.DisplayPicture);
                }
            }
            catch (Exception ex)
            {
                EbLog.Write("UpdateAuthInfo---" + ex.Message);
            }
        }

        public static bool IsTokenExpired(string rtoken)
        {
            var jwtToken = new JwtSecurityToken(rtoken);

            if (DateTime.Compare(jwtToken.ValidTo, DateTime.Now) < 0)
                return true;
            else
                return false;
        }

        public static async Task AuthIfTokenExpiredAsync()
        {
            if (IsTokenExpired(App.Settings.RToken))
            {
                string _username = App.Settings.UserName;
                string _password = Utils.PassWord;

                IdentityService service = IdentityService.Instance;

                ApiAuthResponse response = await service.AuthenticateAsync(_username, _password);
                if (response.IsValid)
                    await service.UpdateAuthInfo(response, _username, _password);
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

            await Store.SetJSONAsync(AppConst.MYSOLUTIONS, solutions);
        }

        public async Task Navigate(EbMobileSolutionData data)
        {
            if (data.Applications != null)
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
}
