using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Linq;
using SQLite;

namespace ExpressBase.Mobile.Services
{
    public class CommonServices
    {
        public static void PushWebFormData(string Form, string RefId, int Locid, int RowId)
        {
            string uri = Settings.RootUrl + "api/webform_save";
            //WebFormSaveResponse Response = null;
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
                //Response =  JsonConvert.DeserializeObject<WebFormSaveResponse>(resp.Content);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                // Response = new WebFormSaveResponse();
            }
            //return Response;
        }

        public void CreateLocalTable4Form(EbMobileForm form)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("CREATE TABLE IF NOT EXISTS {0} (", form.TableName);

                foreach (EbMobileControl Control in form.ChiledControls)
                {
                    if(Control is EbMobileTableLayout)
                    {

                    }
                    else
                    {
                        sb.AppendFormat("{0} {1}", Control.Name, Control.SQLiteType);
                        if (Control != form.ChiledControls.Last())
                            sb.Append(",");
                    }
                }
                sb.Append(");");

                int status = App.DataDB.DoNonQuery(sb.ToString());
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
