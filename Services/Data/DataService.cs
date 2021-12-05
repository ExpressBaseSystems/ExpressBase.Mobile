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

        public MobileDataResponse GetData(string refid, int limit, int offset, List<Param> param, List<SortColumn> sort, List<Param> search, bool is_powerselect)
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
                return JsonConvert.DeserializeObject<MobileDataResponse>(iresp.Content);
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
            return new MobileDataResponse();
        }

        public async Task<MobileDataResponse> GetDataAsync(string refid, int limit, int offset, List<Param> param, List<SortColumn> sort, List<Param> search, bool is_powerselect, bool no_wrap = false)
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
                request.AddParameter("no_wrap", no_wrap);

                // auth Headers for api
                request.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
                request.AddHeader(AppConst.RTOKEN, App.Settings.RToken);

                IRestResponse iresp = await HttpClient.ExecuteAsync(request);
                return JsonConvert.DeserializeObject<MobileDataResponse>(iresp.Content);
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
            return new MobileDataResponse();
        }

        public async Task<MobileDataResponse> GetDataAsyncPs(string refid, int limit, int offset, List<Param> param, List<Param> search)
        {
            try
            {
                RestRequest req = new RestRequest(ApiConstants.GET_DATA_PS, Method.POST);
                req.AddParameter("refid", refid);

                if (param != null && param.Count > 0)
                    req.AddParameter("param", JsonConvert.SerializeObject(param));
                if (search != null && search.Count > 0)
                    req.AddParameter("search", JsonConvert.SerializeObject(search));

                req.AddParameter("limit", limit);
                req.AddParameter("offset", offset);

                // auth Headers for api
                req.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
                req.AddHeader(AppConst.RTOKEN, App.Settings.RToken);

                IRestResponse iresp = await HttpClient.ExecuteAsync(req);
                return JsonConvert.DeserializeObject<MobileDataResponse>(iresp.Content);
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
            return new MobileDataResponse();
        }

        public async Task<MobileDataResponse> GetDataAsyncV2(string refid, int limit, int offset, List<Param> param, List<SortColumn> sort, List<Param> search)
        {
            try
            {
                RestRequest request = base.GetRequest(ApiConstants.GET_VIS_DATA_V2, Method.GET);

                request.AddParameter("refid", refid);

                if (param != null) request.AddParameter("param", JsonConvert.SerializeObject(param));
                if (sort != null) request.AddParameter("sort_order", JsonConvert.SerializeObject(sort));
                if (search != null) request.AddParameter("search", JsonConvert.SerializeObject(search));


                request.AddParameter("limit", limit);
                request.AddParameter("offset", offset);

                IRestResponse iresp = await HttpClient.ExecuteAsync(request);

                if (iresp.IsSuccessful)
                    return JsonConvert.DeserializeObject<MobileDataResponse>(iresp.Content);
                else
                    base.LogHttpResponse(iresp);
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
            return null;
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

        public async Task<MobileDataResponse> GetDataFlatAsync(string refid)
        {
            try
            {
                RestRequest request = new RestRequest(ApiConstants.GET_DATA_FLAT, Method.GET);

                request.AddParameter("refid", refid);

                request.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
                request.AddHeader(AppConst.RTOKEN, App.Settings.RToken);

                IRestResponse iresp = await HttpClient.ExecuteAsync(request);
                if (iresp.IsSuccessful)
                    return JsonConvert.DeserializeObject<MobileDataResponse>(iresp.Content);
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
