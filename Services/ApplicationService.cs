using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.Services
{
    public interface IApplicationService
    {
        Task<ObservableCollection<AppData>> GetDataAsync();
    }

    public class ApplicationService : IApplicationService
    {
        public async Task<ObservableCollection<AppData>> GetDataAsync()
        {
            List<AppData> applications = null;
            try
            {
                applications = Store.GetJSON<List<AppData>>(AppConst.APP_COLLECTION);

                if (applications == null || applications.Count <= 0)
                {
                    applications = await this.GetAppCollections(App.Settings.CurrentLocId);
                    applications.OrderBy(x => x.AppName);
                    await Store.SetJSONAsync(AppConst.APP_COLLECTION, applications);
                }
            }
            catch (Exception ex)
            {
                EbLog.Write(ex.Message);
            }
            return applications.ToObservableCollection();
        }

        private async Task<List<AppData>> GetAppCollections(int locid)
        {
            List<AppData> _Apps = null;
            try
            {
                RestClient client = new RestClient(App.Settings.RootUrl);

                RestRequest request = new RestRequest("api/menu", Method.GET);
                request.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
                request.AddHeader(AppConst.RTOKEN, App.Settings.RToken);

                request.AddParameter("locid", locid);

                var response = await client.ExecuteAsync(request);
                if (response.StatusCode == HttpStatusCode.OK)
                    _Apps = JsonConvert.DeserializeObject<AppCollection>(response.Content).Applications;
            }
            catch (Exception ex)
            {
                EbLog.Write(ex.Message);
                _Apps = new List<AppData>();
            }
            return _Apps;
        }
    }
}
