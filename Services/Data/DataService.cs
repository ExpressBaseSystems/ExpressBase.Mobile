﻿using ExpressBase.Mobile.Constants;
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
    public class DataService : IDataService
    {
        private static DataService _instance;

        public static DataService Instance => _instance ??= new DataService();

        public MobileVisDataRespnse GetData(string refid, int limit, int offset, List<Param> param, List<SortColumn> sort, List<Param> search, bool is_powerselect)
        {
            try
            {
                RestClient client = new RestClient(App.Settings.RootUrl);

                RestRequest request = new RestRequest(ApiConstants.GET_VIS_DATA, Method.POST);
                request.AddParameter("refid", refid);

                if (param != null)
                    request.AddParameter("param", JsonConvert.SerializeObject(param));

                if (sort != null)
                    request.AddParameter("sort_order", JsonConvert.SerializeObject(sort));

                if(search !=null)
                    request.AddParameter("search", JsonConvert.SerializeObject(search));

                request.AddParameter("limit", limit);
                request.AddParameter("offset", offset);
                request.AddParameter("is_powerselect", is_powerselect);

                // auth Headers for api
                request.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
                request.AddHeader(AppConst.RTOKEN, App.Settings.RToken);

                IRestResponse iresp = client.Execute(request);
                return JsonConvert.DeserializeObject<MobileVisDataRespnse>(iresp.Content);
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
            return new MobileVisDataRespnse();
        }

        public async Task<MobileVisDataRespnse> GetDataAsync(string refid, int limit, int offset, List<Param> param, List<SortColumn> sort, List<Param> search, bool is_powerselect)
        {
            try
            {
                var client = new RestClient(App.Settings.RootUrl);

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

                IRestResponse iresp = await client.ExecuteAsync(request);
                return JsonConvert.DeserializeObject<MobileVisDataRespnse>(iresp.Content);
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
            return new MobileVisDataRespnse();
        }

        public async Task<ApiFileResponse> GetFile(EbFileCategory category, string filename)
        {
            ApiFileResponse resp = null;
            try
            {
                RestClient client = new RestClient(App.Settings.RootUrl);

                RestRequest request = new RestRequest("api/get_file", Method.GET);
                // auth Headers for api
                request.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
                request.AddHeader(AppConst.RTOKEN, App.Settings.RToken);

                request.AddParameter("category", (int)category);
                request.AddParameter("filename", filename);

                IRestResponse response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                    resp = JsonConvert.DeserializeObject<ApiFileResponse>(response.Content);
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
            return resp;
        }

        public async Task<byte[]> GetLocalFile(string filename)
        {
            try
            {
                await Task.Delay(1);

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
