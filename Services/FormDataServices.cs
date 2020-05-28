using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.Services
{
    public interface IFormDataService
    {
        Task<WebformData> GetFormLiveDataAsync(string refid, int row_id, int loc_id);

        Task<EbDataSet> GetFormLocalDataAsync(EbMobileForm form, int rowid);

        Task<PushResponse> SendFormDataAsync(WebformData WebFormData, int RowId, string WebFormRefId, int LocId);

        Tuple<string, byte[]> GetFile(EbFileCategory category, string filename);
    }

    public class FormDataServices : IFormDataService
    {
        private static FormDataServices _instance;

        public static FormDataServices Instance => _instance ?? (_instance = new FormDataServices());

        public async Task<WebformData> GetFormLiveDataAsync(string refid, int row_id, int loc_id)
        {
            WebformData wd;
            try
            {
                RestClient client = new RestClient(App.Settings.RootUrl);

                RestRequest request = new RestRequest("api/get_formdata", Method.GET);
                request.AddParameter("refid", refid);
                request.AddParameter("row_id", row_id);
                request.AddParameter("loc_id", loc_id);

                // auth Headers for api
                request.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
                request.AddHeader(AppConst.RTOKEN, App.Settings.RToken);

                IRestResponse iresp = await client.ExecuteAsync(request);
                MobileFormLiveData response = JsonConvert.DeserializeObject<MobileFormLiveData>(iresp.Content);
                wd = response.Data;
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
                wd = new WebformData();
            }
            return wd;
        }

        public async Task<EbDataSet> GetFormLocalDataAsync(EbMobileForm form, int rowid)
        {
            EbDataSet ds = new EbDataSet();
            try
            {
                //only to remove the warning
                await Task.Delay(1);

                EbDataTable masterData = App.DataDB.DoQuery($"SELECT * FROM {form.TableName} WHERE id = {rowid};");
                masterData.TableName = form.TableName;
                ds.Tables.Add(masterData);

                foreach (var pair in form.ControlDictionary)
                {
                    if (pair.Value is ILinesEnabled)
                    {
                        string linesQuery = $"SELECT * FROM {(pair.Value as ILinesEnabled).TableName} WHERE {form.TableName}_id = {rowid};";

                        EbDataTable linesData = App.DataDB.DoQuery(linesQuery);
                        linesData.TableName = (pair.Value as ILinesEnabled).TableName;
                        ds.Tables.Add(linesData);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
            return ds;
        }

        public async Task<PushResponse> SendFormDataAsync(WebformData WebFormData, int RowId, string WebFormRefId, int LocId)
        {
            try
            {
                RestClient client = new RestClient(App.Settings.RootUrl);

                RestRequest request = new RestRequest("api/push_data", Method.POST);
                request.AddParameter("webform_data", JsonConvert.SerializeObject(WebFormData));
                request.AddParameter("rowid", RowId);
                request.AddParameter("refid", WebFormRefId);
                request.AddParameter("locid", LocId);

                // auth Headers for api
                request.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
                request.AddHeader(AppConst.RTOKEN, App.Settings.RToken);

                IRestResponse response = await client.ExecuteAsync(request);
                return JsonConvert.DeserializeObject<PushResponse>(response.Content);
            }
            catch (Exception e)
            {
                Log.Write(e.Message);
            }
            return null;
        }

        public async Task<List<ApiFileData>> SendFilesAsync(List<FileWrapper> Files)
        {
            List<ApiFileData> FileData = null;
            try
            {
                RestClient client = new RestClient(App.Settings.RootUrl);

                RestRequest request = new RestRequest("api/files/upload", Method.POST);

                foreach (FileWrapper file in Files)
                    request.AddFileBytes(file.Name, file.Bytea, file.FileName);

                // auth Headers for api
                request.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
                request.AddHeader(AppConst.RTOKEN, App.Settings.RToken);

                IRestResponse response = await client.ExecuteAsync(request);
                if (response.StatusCode == HttpStatusCode.OK)
                    FileData = JsonConvert.DeserializeObject<List<ApiFileData>>(response.Content);
            }
            catch (Exception e)
            {
                Log.Write(e.Message);
            }
            return FileData;
        }

        public Tuple<string, byte[]> GetFile(EbFileCategory category, string filename)
        {
            try
            {
                string imgType = category == EbFileCategory.File ? "files" : "images";

                RestClient client = new RestClient(App.Settings.RootUrl);

                RestRequest request = new RestRequest($"api/{imgType}/{filename}", Method.GET);

                // auth Headers for api
                request.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
                request.AddHeader(AppConst.RTOKEN, App.Settings.RToken);

                IRestResponse iresp = client.Execute(request);

                if (iresp.StatusCode == HttpStatusCode.OK)
                {
                    return new Tuple<string, byte[]>(iresp.ContentType, iresp.RawBytes);
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
            return null;
        }
    }
}
