using ExpressBase.Mobile.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Text;

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

        public static ApiAuthResponse TryAuthenticate(string username,string password)
        {
            HttpClient client = new HttpClient();
            Uri uri = new Uri(Settings.RootUrl + "api/authenticate");
            ApiAuthResponse resp = null;

            var formContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "username", username.Trim() },
                { "password", password.Trim() }
            });

            try
            {
                var response = client.PostAsync(uri.ToString(), formContent).Result;
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content.ReadAsStringAsync();
                    resp = JsonConvert.DeserializeObject<ApiAuthResponse>(responseContent.Result);
                }
                else
                    resp = new ApiAuthResponse { IsValid = false };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                resp = new ApiAuthResponse { IsValid = false };
            }
            return resp;
        }

        public static void UpdateStore(ApiAuthResponse resp,string username,string password)
        {
            Store.SetValue(Constants.BTOKEN, resp.BToken);
            Store.SetValue(Constants.RTOKEN, resp.RToken);
            Store.SetValue(Constants.USER_ID, resp.UserId.ToString());
            Store.SetValue(Constants.DISPLAY_NAME, resp.DisplayName);
            Store.SetValue(Constants.USERNAME, username.Trim());
            Store.SetValue(Constants.PASSWORD, password.Trim());
        }
    }
}
