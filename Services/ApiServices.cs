using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ExpressBase.Mobile.Services
{
    public class Api
    {
        public static PushResponse Push(WebformData WebFormData, int RowId, string WebFormRefId, int LocId)
        {
            try
            {
                RestClient client = new RestClient(Settings.RootUrl);

                RestRequest request = new RestRequest("api/push_data", Method.POST);
                request.AddParameter("webform_data", JsonConvert.SerializeObject(WebFormData));
                request.AddParameter("rowid", RowId);
                request.AddParameter("refid", WebFormRefId);
                request.AddParameter("locid", LocId);

                // auth Headers for api
                request.AddHeader(AppConst.BTOKEN, Store.GetValue(AppConst.BTOKEN));
                request.AddHeader(AppConst.RTOKEN, Store.GetValue(AppConst.RTOKEN));

                IRestResponse response = client.Execute(request);
                return JsonConvert.DeserializeObject<PushResponse>(response.Content);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return null;
        }

        public static List<ApiFileData> PushFiles(List<FileWrapper> Files)
        {
            List<ApiFileData> FileData = null;
            try
            {
                RestClient client = new RestClient(Settings.RootUrl);
                RestRequest request = new RestRequest("api/files/upload", Method.POST);

                foreach(FileWrapper file in Files)
                {
                    request.AddFileBytes(file.Name, file.Bytea, file.FileName);
                }

                // auth Headers for api
                request.AddHeader(AppConst.BTOKEN, Store.GetValue(AppConst.BTOKEN));
                request.AddHeader(AppConst.RTOKEN, Store.GetValue(AppConst.RTOKEN));

                IRestResponse response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.OK)
                    FileData = JsonConvert.DeserializeObject<List<ApiFileData>>(response.Content);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return FileData;
        }

        public static ValidateSidResponse ValidateSid(string url)
        {
            ValidateSidResponse Vresp = new ValidateSidResponse();
            try
            {
                RestClient client = new RestClient("https://" + url);
                RestRequest request = new RestRequest("api/validate_solution", Method.GET);
                IRestResponse response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.OK)
                    Vresp = JsonConvert.DeserializeObject<ValidateSidResponse>(response.Content);
            }
            catch (Exception e)
            {
                Vresp.IsValid = false;
                Console.WriteLine(e.Message);
            }
            return Vresp;
        }

        public static List<AppData> GetAppCollections()
        {
            List<AppData> _Apps = null;
            try
            {
                RestClient client = new RestClient(Settings.RootUrl);
                RestRequest request = new RestRequest("api/menu", Method.GET);
                request.AddHeader(AppConst.BTOKEN, Store.GetValue(AppConst.BTOKEN));
                request.AddHeader(AppConst.RTOKEN, Store.GetValue(AppConst.RTOKEN));

                var response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    _Apps = JsonConvert.DeserializeObject<AppCollection>(response.Content).Applications;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return _Apps;
        }

        public static MobilePageCollection GetEbObjects(int AppId, int LocationId, bool PullData = false)
        {
            try
            {
                RestClient client = new RestClient(Settings.RootUrl);
                RestRequest request = new RestRequest("api/objects_by_app", Method.GET);
                request.AddHeader(AppConst.BTOKEN, Store.GetValue(AppConst.BTOKEN));
                request.AddHeader(AppConst.RTOKEN, Store.GetValue(AppConst.RTOKEN));

                request.AddParameter("appid", AppId);
                request.AddParameter("locid", LocationId);
                request.AddParameter("pull_data", PullData);

                var response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<MobilePageCollection>(response.Content);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new MobilePageCollection();
            }
            return new MobilePageCollection();
        }
    }
}
