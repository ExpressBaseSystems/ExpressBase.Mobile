using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Services
{
    public class Auth
    {
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

                ApiAuthResponse response = await Auth.TryAuthenticateAsync(_username, _password);
                if (response.IsValid)
                    Auth.UpdateStore(response, _username, _password);
            }
        }

        public static void AuthIfTokenExpired()
        {
            if (IsTokenExpired(Settings.RToken))
            {
                string _username = Settings.UserName;
                string _password = Settings.PassWord;

                ApiAuthResponse response = Auth.TryAuthenticate(_username, _password);
                if (response.IsValid)
                    Auth.UpdateStore(response, _username, _password);
            }
        }

        public static ApiAuthResponse TryAuthenticate(string username, string password)
        {
            ApiAuthResponse resp;
            try
            {
                RestClient client = new RestClient(Settings.RootUrl);
                RestRequest request = new RestRequest("api/auth", Method.GET);

                request.AddParameter("username", username.Trim());
                request.AddParameter("password", ToMD5Hash(string.Concat(password, username)));

                var response = client.Execute(request);

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

        public static async Task<ApiAuthResponse> TryAuthenticateAsync(string username, string password)
        {
            ApiAuthResponse resp;
            try
            {
                RestClient client = new RestClient(Settings.RootUrl);
                RestRequest request = new RestRequest("api/auth", Method.GET);

                request.AddParameter("username", username.Trim());
                request.AddParameter("password", ToMD5Hash(string.Concat(password, username)));

                var response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                    resp = JsonConvert.DeserializeObject<ApiAuthResponse>(response.Content);
                else
                    resp = new ApiAuthResponse { IsValid = false };
            }
            catch (Exception ex)
            {
                Log.Write("Auth.TryAuthenticateAsync---" + ex.Message);
                resp = new ApiAuthResponse { IsValid = false };
            }
            return resp;
        }

        public static void UpdateStore(ApiAuthResponse resp, string username, string password)
        {
            Store.SetValue(AppConst.BTOKEN, resp.BToken);
            Store.SetValue(AppConst.RTOKEN, resp.RToken);
            Store.SetValue(AppConst.USER_ID, resp.UserId.ToString());
            Store.SetValue(AppConst.DISPLAY_NAME, resp.DisplayName);
            Store.SetValue(AppConst.USERNAME, username.Trim());
            Store.SetValue(AppConst.PASSWORD, password.Trim());
            Store.SetJSON(AppConst.USER_OBJECT, resp.User);
            Store.SetJSON(AppConst.USER_LOCATIONS, resp.Locations);
            Store.SetValue(AppConst.CURRENT_LOCATION, resp.User.Preference.DefaultLocation.ToString());
            try
            {
                if (resp.DisplayPicture != null)
                {
                    INativeHelper helper = DependencyService.Get<INativeHelper>();
                    string url = helper.NativeRoot + $"/ExpressBase/{Settings.SolutionId.ToUpper()}/user.png";
                    File.WriteAllBytes(url, resp.DisplayPicture);
                }
            }
            catch (Exception ex)
            {
                Log.Write("Auth.UpdateStore (DisplayPicture)---" + ex.Message);
            }
        }

        public static string ToMD5Hash(string str)
        {
            var md5 = MD5.Create();

            byte[] result = md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(str));

            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                strBuilder.Append(result[i].ToString("x2"));
            }
            return strBuilder.ToString();
        }
    }
}
