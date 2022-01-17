using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.Services
{
    public class ApplicationService : BaseService, IApplicationService
    {
        public List<AppData> GetDataAsync()
        {
            return Utils.Applications;
        }

        public async Task<List<AppData>> UpdateDataAsync(Loader loader)
        {
            List<AppData> apps = null;
            loader.IsVisible = true;
            loader.Message = "Refreshing...";
            try
            {
                EbMobileSolutionData data = await App.Settings.GetSolutionDataAsync(loader);

                if (data != null)
                {
                    apps = data.Applications;
                    Utils.Toast("Refreshed");
                }
            }
            catch (Exception ex)
            {
                EbLog.Error("Failed to get solution data :: " + ex.Message);
            }
            loader.IsVisible = false;
            return apps;
        }
    }
}
