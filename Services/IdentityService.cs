using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Services
{
    public interface IIdentityService
    {
        Task<ApiAuthResponse> AuthenticateAsync(string username, string password);

        Task<ImageSource> GetLogo(string sid);

        Task UpdateAuthInfo(ApiAuthResponse resp, string username, string password);
    }

    public class IdentityService : IIdentityService
    {
        public RestClient Client { set; get; }

        public IdentityService()
        {
            Client = new RestClient(Settings.RootUrl);
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
                Log.Write("Auth.TryAuthenticate---" + ex.Message);
                resp = new ApiAuthResponse { IsValid = false };
            }
            return resp;
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
                Log.Write("GetLogo" + ex.Message);
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
                await Store.SetValueAsync(AppConst.USER_ID, resp.UserId.ToString());
                await Store.SetValueAsync(AppConst.DISPLAY_NAME, resp.DisplayName);
                await Store.SetValueAsync(AppConst.USERNAME, username.Trim());
                await Store.SetValueAsync(AppConst.PASSWORD, password.Trim());
                await Store.SetValueAsync(AppConst.CURRENT_LOCATION, resp.User.Preference.DefaultLocation.ToString());

                //json data
                await Store.SetJSONAsync(AppConst.USER_OBJECT, resp.User);
                await Store.SetJSONAsync(AppConst.USER_LOCATIONS, resp.Locations);


                if (resp.DisplayPicture != null)
                {
                    INativeHelper helper = DependencyService.Get<INativeHelper>();
                    string url = helper.NativeRoot + $"/ExpressBase/{Settings.SolutionId.ToUpper()}/user.png";
                    File.WriteAllBytes(url, resp.DisplayPicture);
                }
            }
            catch (Exception ex)
            {
                Log.Write("UpdateAuthInfo---" + ex.Message);
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
            if (IsTokenExpired(Settings.RToken))
            {
                string _username = Settings.UserName;
                string _password = Settings.PassWord;

                IdentityService service = new IdentityService();

                ApiAuthResponse response = await service.AuthenticateAsync(_username, _password);
                if (response.IsValid)
                    await service.UpdateAuthInfo(response, _username, _password);
            }
        }
    }
}
