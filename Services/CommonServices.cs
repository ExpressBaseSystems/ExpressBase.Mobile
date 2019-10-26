using ExpressBase.Mobile.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace ExpressBase.Mobile.Services
{
    public class CommonServices
    {

        public static EbObjectToMobResponse GetObjectByRef(string refid)
        {
            EbObjectToMobResponse wraper = new EbObjectToMobResponse();
            HttpClient client = new HttpClient();
            string uri = Settings.RootUrl + string.Format("api/object_by_ref?refid={0}", refid);

            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
            requestMessage.Headers.Add(Constants.BTOKEN, Store.GetValue(Constants.BTOKEN));
            requestMessage.Headers.Add(Constants.RTOKEN, Store.GetValue(Constants.RTOKEN));

            try
            {
                var response = client.SendAsync(requestMessage).Result;
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content.ReadAsStringAsync();
                    wraper = JsonConvert.DeserializeObject<EbObjectToMobResponse>(responseContent.Result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return wraper;
        }
    }
}
