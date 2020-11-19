using ExpressBase.Mobile.Configuration;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Services.GoogleMap
{
    public class GoogleMapApiService :BaseService, IGoogleMapApiService
    {
        readonly string GoogleMapApiKey = Device.RuntimePlatform == Device.Android ? EbBuildConfig.GMapAndroidKey : EbBuildConfig.GMapiOSKey;

        const string ApiBaseAddress = "https://maps.googleapis.com/maps/";

        public GoogleMapApiService() : base(ApiBaseAddress) { }

        public async Task<GooglePlaceAutoCompleteResults> GetPlaces(string text)
        {
            try
            {
                RestRequest request = new RestRequest("api/place/autocomplete/json", Method.GET);

                request.AddParameter("input", Uri.EscapeUriString(text));
                request.AddParameter("key", GoogleMapApiKey);

                IRestResponse iresp = await HttpClient.ExecuteAsync(request);

                if (iresp.IsSuccessful)
                {
                    return JsonConvert.DeserializeObject<GooglePlaceAutoCompleteResults>(iresp.Content);
                }
            }
            catch (Exception ex)
            {
                EbLog.Info("Get places api error");
                EbLog.Error(ex.Message);
            }
            return null;
        }

        public async Task<GooglePlace> GetPlaceDetails(string placeId)
        {
            try
            {
                RestRequest request = new RestRequest("api/place/details/json", Method.GET);

                request.AddParameter("placeid", Uri.EscapeUriString(placeId));
                request.AddParameter("key", GoogleMapApiKey);

                IRestResponse iresp = await HttpClient.ExecuteAsync(request);

                if (iresp.IsSuccessful)
                {
                    return JsonConvert.DeserializeObject<GooglePlace>(iresp.Content);
                }
            }
            catch (Exception ex)
            {
                EbLog.Info("Get places api error");
                EbLog.Error(ex.Message);
            }
            return null;
        }
    }
}
