using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Services
{
    public class DataService : BaseService, IDataService
    {
        private static DataService _instance;

        public static DataService Instance => _instance ??= new DataService();

        public DataService() : base(true) { }

        public MobileVisDataResponse GetData(string refid, int limit, int offset, List<Param> param, List<SortColumn> sort, List<Param> search, bool is_powerselect)
        {
            try
            {
                RestRequest request = new RestRequest(ApiConstants.GET_VIS_DATA, Method.POST);
                request.AddParameter("refid", refid);

                if (param != null)
                    request.AddParameter("param", JsonConvert.SerializeObject(param));

                if (sort != null)
                    request.AddParameter("sort_order", JsonConvert.SerializeObject(sort));

                if (search != null)
                    request.AddParameter("search", JsonConvert.SerializeObject(search));

                request.AddParameter("limit", limit);
                request.AddParameter("offset", offset);
                request.AddParameter("is_powerselect", is_powerselect);

                // auth Headers for api
                request.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
                request.AddHeader(AppConst.RTOKEN, App.Settings.RToken);

                IRestResponse iresp = HttpClient.Execute(request);
                return JsonConvert.DeserializeObject<MobileVisDataResponse>(iresp.Content);
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
            return new MobileVisDataResponse();
        }

        public async Task<MobileVisDataResponse> GetDataAsync(string refid, int limit, int offset, List<Param> param, List<SortColumn> sort, List<Param> search, bool is_powerselect)
        {
            try
            {
                RestRequest request = new RestRequest(ApiConstants.GET_VIS_DATA, Method.POST);
                request.AddParameter("refid", refid);

                if (param != null)
                    request.AddParameter("param", JsonConvert.SerializeObject(param));

                if (sort != null)
                    request.AddParameter("sort_order", JsonConvert.SerializeObject(sort));

                if (search != null)
                    request.AddParameter("search", JsonConvert.SerializeObject(search));

                request.AddParameter("limit", limit);
                request.AddParameter("offset", offset);
                request.AddParameter("is_powerselect", is_powerselect);

                // auth Headers for api
                request.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
                request.AddHeader(AppConst.RTOKEN, App.Settings.RToken);

                IRestResponse iresp = await HttpClient.ExecuteAsync(request);
                return JsonConvert.DeserializeObject<MobileVisDataResponse>(iresp.Content);
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
            return new MobileVisDataResponse();
        }

        public async Task<MobileProfileData> GetProfileDataAsync(string refid, int loc_id)
        {
            try
            {
                RestRequest request = new RestRequest(ApiConstants.GET_PROFILE_DATA, Method.GET);

                request.AddParameter("refid", refid);
                request.AddParameter("loc_id", loc_id);

                request.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
                request.AddHeader(AppConst.RTOKEN, App.Settings.RToken);

                IRestResponse iresp = await HttpClient.ExecuteAsync(request);
                if (iresp.IsSuccessful)
                    return JsonConvert.DeserializeObject<MobileProfileData>(iresp.Content);
                else
                    EbLog.Error(iresp.Content);
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
            return null;
        }

        public async Task<ApiFileResponse> GetFileAsync(EbFileCategory category, string filename)
        {
            ApiFileResponse resp = null;
            try
            {
                RestRequest request = new RestRequest("api/get_file", Method.GET);
                // auth Headers for api
                request.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
                request.AddHeader(AppConst.RTOKEN, App.Settings.RToken);

                request.AddParameter("category", (int)category);
                request.AddParameter("filename", filename);

                IRestResponse response = await HttpClient.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                    resp = JsonConvert.DeserializeObject<ApiFileResponse>(response.Content);
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
            return resp;
        }

        public ApiFileResponse GetFile(EbFileCategory category, string filename)
        {
            ApiFileResponse resp = null;
            try
            {
                RestRequest request = new RestRequest("api/get_file", Method.GET);
                // auth Headers for api
                request.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
                request.AddHeader(AppConst.RTOKEN, App.Settings.RToken);

                request.AddParameter("category", (int)category);
                request.AddParameter("filename", filename);

                IRestResponse response = HttpClient.Execute(request);

                if (response.StatusCode == HttpStatusCode.OK)
                    resp = JsonConvert.DeserializeObject<ApiFileResponse>(response.Content);
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
            return resp;
        }

        public byte[] GetLocalFile(string filename)
        {
            try
            {
                INativeHelper helper = DependencyService.Get<INativeHelper>();

                byte[] bytes = helper.GetFile($"{App.Settings.AppDirectory}/{App.Settings.Sid}/FILES/{filename}");
                if (bytes != null)
                    return bytes;
            }
            catch (Exception ex)
            {
                EbLog.Error("GetLocalFile in Dataservice got some error :: " + ex.Message);
            }
            return null;
        }
    }
}
