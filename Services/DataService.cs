using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.Services
{
    public interface IDataService
    {
        VisualizationLiveData GetData(string datasorce_ref, List<Param> parameters, List<SortColumn> sortOrder, int limit, int offset, bool is_powerselect = false);

        Task<VisualizationLiveData> GetDataAsync(string datasorce_ref, List<Param> parameters, List<SortColumn> sortOrder, int limit, int offset, bool is_powerselect = false);
    }

    public class DataService : IDataService
    {
        private static DataService _instance;

        public static DataService Instance => _instance ?? (_instance = new DataService());

        public VisualizationLiveData GetData(string datasorce_ref, List<Param> parameters, List<SortColumn> sortOrder, int limit, int offset, bool is_powerselect = false)
        {
            try
            {
                var client = new RestClient(App.Settings.RootUrl);

                RestRequest request = new RestRequest("api/get_data", Method.GET);
                request.AddParameter("refid", datasorce_ref);

                if (parameters != null)
                    request.AddParameter("param", JsonConvert.SerializeObject(parameters));

                if (sortOrder != null)
                    request.AddParameter("sort_order", JsonConvert.SerializeObject(sortOrder));

                request.AddParameter("limit", limit);
                request.AddParameter("offset", offset);
                request.AddParameter("is_powerselect", is_powerselect);

                // auth Headers for api
                request.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
                request.AddHeader(AppConst.RTOKEN, App.Settings.RToken);

                IRestResponse iresp = client.Execute(request);
                return JsonConvert.DeserializeObject<VisualizationLiveData>(iresp.Content);
            }
            catch (Exception ex)
            {
                EbLog.Write(ex.Message);
            }
            return new VisualizationLiveData();
        }

        public async Task<VisualizationLiveData> GetDataAsync(string datasorce_ref, List<Param> parameters, List<SortColumn> sortOrder, int limit, int offset, bool is_powerselect = false)
        {
            try
            {
                var client = new RestClient(App.Settings.RootUrl);

                RestRequest request = new RestRequest("api/get_data", Method.GET);
                request.AddParameter("refid", datasorce_ref);

                if (parameters != null)
                    request.AddParameter("param", JsonConvert.SerializeObject(parameters));

                if (sortOrder != null)
                    request.AddParameter("sort_order", JsonConvert.SerializeObject(sortOrder));

                request.AddParameter("limit", limit);
                request.AddParameter("offset", offset);
                request.AddParameter("is_powerselect", is_powerselect);

                // auth Headers for api
                request.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
                request.AddHeader(AppConst.RTOKEN, App.Settings.RToken);

                IRestResponse iresp = await client.ExecuteAsync(request);
                return JsonConvert.DeserializeObject<VisualizationLiveData>(iresp.Content);
            }
            catch (Exception ex)
            {
                EbLog.Write(ex.Message);
            }
            return new VisualizationLiveData();
        }
    }
}
