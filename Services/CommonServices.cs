using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Models;
using Newtonsoft.Json;
using RestSharp;
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
            EbObjectToMobResponse wraper = null;
            try
            {
                RestClient client = new RestClient(Settings.RootUrl + "api/object_by_ref");
                RestRequest request = new RestRequest(Method.GET);
                request.AddHeader(AppConst.BTOKEN, Store.GetValue(AppConst.BTOKEN));
                request.AddHeader(AppConst.RTOKEN, Store.GetValue(AppConst.RTOKEN));
                request.AddParameter("refid", refid);
                var resp = client.Execute(request);
                wraper = JsonConvert.DeserializeObject<EbObjectToMobResponse>(resp.Content);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                wraper = new EbObjectToMobResponse();
            }
            return wraper;
        }

        public static WebFormSaveResponse PushWebFormData(string Form, string RefId, int Locid, int RowId)
        {
            string uri = Settings.RootUrl + "api/webform_save";
            WebFormSaveResponse Response = null;
            try
            {
                RestClient client = new RestClient(uri);
                RestRequest request = new RestRequest(Method.POST);

                request.AddHeader(AppConst.BTOKEN, Store.GetValue(AppConst.BTOKEN));
                request.AddHeader(AppConst.RTOKEN, Store.GetValue(AppConst.RTOKEN));

                request.AddParameter("webform_data", Form);
                request.AddParameter("refid", RefId);
                request.AddParameter("locid", Locid);
                request.AddParameter("rowid", RowId);

                var resp = client.Execute(request);
                Response =  JsonConvert.DeserializeObject<WebFormSaveResponse>(resp.Content);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Response = new WebFormSaveResponse();
            }
            return Response;
        }
    }
}
