using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.Services
{
    public class RestServices
    {
        private static RestServices instance;

        public static RestServices Instance => instance ?? (instance = new RestServices());

        public RestClient Client { set; get; }

        private RestServices()
        {
            Client = new RestClient(Settings.RootUrl);
        }

        public void UpdateBaseUrl()
        {
            Client.BaseUrl = new Uri(Settings.RootUrl);
        }

        public static async Task<ValidateSidResponse> ValidateSid(string url)
        {
            ValidateSidResponse Vresp = new ValidateSidResponse();
            try
            {
                RestClient client = new RestClient("https://" + url);
                RestRequest request = new RestRequest("api/validate_solution", Method.GET);
                IRestResponse response = await client.ExecuteAsync(request);
                if (response.StatusCode == HttpStatusCode.OK)
                    Vresp = JsonConvert.DeserializeObject<ValidateSidResponse>(response.Content);
            }
            catch (Exception e)
            {
                Vresp.IsValid = false;
                Log.Write("RestServices.ValidateSid---" + e.Message);
            }
            return Vresp;
        }

        public List<AppData> GetAppCollections(int locid)
        {
            List<AppData> _Apps = null;
            try
            {
                RestRequest request = new RestRequest("api/menu", Method.GET);
                request.AddHeader(AppConst.BTOKEN, Settings.BToken);
                request.AddHeader(AppConst.RTOKEN, Settings.RToken);

                request.AddParameter("locid", locid);

                var response = Client.Execute(request);
                if (response.StatusCode == HttpStatusCode.OK)
                    _Apps = JsonConvert.DeserializeObject<AppCollection>(response.Content).Applications;
            }
            catch (Exception ex)
            {
                Log.Write("RestServices.GetAppCollections---" + ex.Message);
            }
            return _Apps;
        }

        public async Task<MobilePageCollection> GetEbObjects(int AppId, int LocationId, bool PullData = false)
        {
            try
            {
                RestRequest request = new RestRequest("api/objects_by_app", Method.GET);
                request.AddHeader(AppConst.BTOKEN, Settings.BToken);
                request.AddHeader(AppConst.RTOKEN, Settings.RToken);

                request.AddParameter("appid", AppId);
                request.AddParameter("locid", LocationId);
                request.AddParameter("pull_data", PullData);

                var response = await Client.ExecuteAsync(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<MobilePageCollection>(response.Content);
                }
            }
            catch (Exception ex)
            {
                Log.Write("RestServices.GetEbObjects---" + ex.Message);
                return new MobilePageCollection();
            }
            return new MobilePageCollection();
        }

        public PushResponse Push(WebformData WebFormData, int RowId, string WebFormRefId, int LocId)
        {
            try
            {
                RestRequest request = new RestRequest("api/push_data", Method.POST);
                request.AddParameter("webform_data", JsonConvert.SerializeObject(WebFormData));
                request.AddParameter("rowid", RowId);
                request.AddParameter("refid", WebFormRefId);
                request.AddParameter("locid", LocId);

                // auth Headers for api
                request.AddHeader(AppConst.BTOKEN, Settings.BToken);
                request.AddHeader(AppConst.RTOKEN, Settings.RToken);

                IRestResponse response = Client.Execute(request);
                return JsonConvert.DeserializeObject<PushResponse>(response.Content);
            }
            catch (Exception e)
            {
                Log.Write("RestServices.Push---" + e.Message);
            }
            return null;
        }

        public async Task<PushResponse> PushAsync(WebformData WebFormData, int RowId, string WebFormRefId, int LocId)
        {
            try
            {
                RestRequest request = new RestRequest("api/push_data", Method.POST);
                request.AddParameter("webform_data", JsonConvert.SerializeObject(WebFormData));
                request.AddParameter("rowid", RowId);
                request.AddParameter("refid", WebFormRefId);
                request.AddParameter("locid", LocId);

                // auth Headers for api
                request.AddHeader(AppConst.BTOKEN, Settings.BToken);
                request.AddHeader(AppConst.RTOKEN, Settings.RToken);

                IRestResponse response = await Client.ExecuteAsync(request);
                return JsonConvert.DeserializeObject<PushResponse>(response.Content);
            }
            catch (Exception e)
            {
                Log.Write("RestServices.PushAsync---" + e.Message);
            }
            return null;
        }

        public List<ApiFileData> PushFiles(List<FileWrapper> Files)
        {
            List<ApiFileData> FileData = null;
            try
            {
                RestRequest request = new RestRequest("api/files/upload", Method.POST);

                foreach (FileWrapper file in Files)
                    request.AddFileBytes(file.Name, file.Bytea, file.FileName);

                // auth Headers for api
                request.AddHeader(AppConst.BTOKEN, Settings.BToken);
                request.AddHeader(AppConst.RTOKEN, Settings.RToken);

                IRestResponse response = Client.Execute(request);
                if (response.StatusCode == HttpStatusCode.OK)
                    FileData = JsonConvert.DeserializeObject<List<ApiFileData>>(response.Content);
            }
            catch (Exception e)
            {
                Log.Write("RestServices.PushFiles---" + e.Message);
            }
            return FileData;
        }

        public async Task<List<ApiFileData>> PushFilesAsync(List<FileWrapper> Files)
        {
            List<ApiFileData> FileData = null;
            try
            {
                RestRequest request = new RestRequest("api/files/upload", Method.POST);

                foreach (FileWrapper file in Files)
                    request.AddFileBytes(file.Name, file.Bytea, file.FileName);

                request.AddParameter("tags", "mobile");

                // auth Headers for api
                request.AddHeader(AppConst.BTOKEN, Settings.BToken);
                request.AddHeader(AppConst.RTOKEN, Settings.RToken);

                IRestResponse response = await Client.ExecuteAsync(request);
                if (response.StatusCode == HttpStatusCode.OK)
                    FileData = JsonConvert.DeserializeObject<List<ApiFileData>>(response.Content);
            }
            catch (Exception e)
            {
                Log.Write("RestServices.PushFilesAsync---" + e.Message);
            }
            return FileData;
        }

        public VisualizationLiveData PullReaderData(string datasorce_ref, List<Param> parameters, int limit, int offset, bool is_powerselect = false)
        {
            try
            {
                RestRequest request = new RestRequest("api/get_data", Method.GET);
                request.AddParameter("refid", datasorce_ref);
                if (parameters != null)
                {
                    request.AddParameter("param", JsonConvert.SerializeObject(parameters));
                }
                request.AddParameter("limit", limit);
                request.AddParameter("offset", offset);
                request.AddParameter("is_powerselect", is_powerselect);

                // auth Headers for api
                request.AddHeader(AppConst.BTOKEN, Settings.BToken);
                request.AddHeader(AppConst.RTOKEN, Settings.RToken);

                IRestResponse iresp = Client.Execute(request);
                return JsonConvert.DeserializeObject<VisualizationLiveData>(iresp.Content);
            }
            catch (Exception ex)
            {
                Log.Write("RestService.PullReaderData---" + ex.Message);
            }
            return new VisualizationLiveData();
        }

        public async Task<VisualizationLiveData> PullReaderDataAsync(string datasorce_ref, List<Param> parameters, int limit, int offset, bool is_powerselect = false)
        {
            try
            {
                RestRequest request = new RestRequest("api/get_data", Method.GET);
                request.AddParameter("refid", datasorce_ref);
                if (parameters != null)
                {
                    request.AddParameter("param", JsonConvert.SerializeObject(parameters));
                }
                request.AddParameter("limit", limit);
                request.AddParameter("offset", offset);
                request.AddParameter("is_powerselect", is_powerselect);

                // auth Headers for api
                request.AddHeader(AppConst.BTOKEN, Settings.BToken);
                request.AddHeader(AppConst.RTOKEN, Settings.RToken);

                IRestResponse iresp = await Client.ExecuteAsync(request);
                return JsonConvert.DeserializeObject<VisualizationLiveData>(iresp.Content);
            }
            catch (Exception ex)
            {
                Log.Write("RestService.PullReaderData---" + ex.Message);
            }
            return new VisualizationLiveData();
        }

        public MyActionsResponse GetMyActions()
        {
            try
            {
                RestRequest request = new RestRequest("api/get_actions", Method.GET);

                // auth Headers for api
                request.AddHeader(AppConst.BTOKEN, Settings.BToken);
                request.AddHeader(AppConst.RTOKEN, Settings.RToken);

                IRestResponse iresp = Client.Execute(request);
                return JsonConvert.DeserializeObject<MyActionsResponse>(iresp.Content);
            }
            catch (Exception ex)
            {
                Log.Write("RestService.GetMyActions---" + ex.Message);
            }
            return new MyActionsResponse();
        }

        public async Task<MyActionsResponse> GetMyActionsAsync()
        {
            try
            {
                RestRequest request = new RestRequest("api/get_actions", Method.GET);

                // auth Headers for api
                request.AddHeader(AppConst.BTOKEN, Settings.BToken);
                request.AddHeader(AppConst.RTOKEN, Settings.RToken);

                IRestResponse iresp = await Client.ExecuteAsync(request);
                return JsonConvert.DeserializeObject<MyActionsResponse>(iresp.Content);
            }
            catch (Exception ex)
            {
                Log.Write("RestService.GetMyActions---" + ex.Message);
            }
            return new MyActionsResponse();
        }

        public async Task<EbStageInfo> GetMyActionInfoAsync(int stageid, string refid, int dataid)
        {
            try
            {
                RestRequest request = new RestRequest("api/get_action_info", Method.GET);

                // auth Headers for api
                request.AddHeader(AppConst.BTOKEN, Settings.BToken);
                request.AddHeader(AppConst.RTOKEN, Settings.RToken);

                request.AddParameter("stageid", stageid);
                request.AddParameter("refid", refid);
                request.AddParameter("dataid", dataid);

                IRestResponse iresp = await Client.ExecuteAsync(request);
                return JsonConvert.DeserializeObject<EbStageInfo>(iresp.Content);
            }
            catch (Exception ex)
            {
                Log.Write("RestService.GetMyActions---" + ex.Message);
            }
            return new EbStageInfo();
        }

        public WebformData PullFormData(string refid, int row_id, int loc_id)
        {
            try
            {
                RestRequest request = new RestRequest("api/get_formdata", Method.GET);
                request.AddParameter("refid", refid);
                request.AddParameter("row_id", row_id);
                request.AddParameter("loc_id", loc_id);

                // auth Headers for api
                request.AddHeader(AppConst.BTOKEN, Settings.BToken);
                request.AddHeader(AppConst.RTOKEN, Settings.RToken);

                IRestResponse iresp = Client.Execute(request);
                MobileFormLiveData response = JsonConvert.DeserializeObject<MobileFormLiveData>(iresp.Content);
                return response.Data;
            }
            catch (Exception ex)
            {
                Log.Write("RestService.PullFormData---" + ex.Message);
                Log.Write("RestService.PullFormData---" + ex.StackTrace);
            }
            return new WebformData();
        }

        public void GetFile(string filename)
        {
            try
            {
                RestRequest request = new RestRequest($"api/files/{filename}", Method.GET);

                // auth Headers for api
                request.AddHeader(AppConst.BTOKEN, Settings.BToken);
                request.AddHeader(AppConst.RTOKEN, Settings.RToken);

                IRestResponse iresp = Client.Execute(request);
            }
            catch (Exception ex)
            {
                Log.Write("RestService.PullFormData---" + ex.Message);
                Log.Write("RestService.PullFormData---" + ex.StackTrace);
            }
        }
    }
}
