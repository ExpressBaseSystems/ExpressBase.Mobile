using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Cryptography;
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
            Uri uri = new Uri(Settings.RootUrl + "api/auth");
            ApiAuthResponse resp = null;

            username = username.Trim();
            password = password.Trim();

            var formContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "username", username.Trim() },
                { "password", ToMD5Hash(string.Concat(password,username)) }
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
            Store.SetValue(AppConst.BTOKEN, resp.BToken);
            Store.SetValue(AppConst.RTOKEN, resp.RToken);
            Store.SetValue(AppConst.USER_ID, resp.UserId.ToString());
            Store.SetValue(AppConst.DISPLAY_NAME, resp.DisplayName);
            Store.SetValue(AppConst.USERNAME, username.Trim());
            Store.SetValue(AppConst.PASSWORD, password.Trim());
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
