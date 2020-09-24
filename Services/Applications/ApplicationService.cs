using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.Services
{
    public class ApplicationService : IApplicationService
    {
        public List<AppData> GetDataAsync()
        {
            return Utils.Applications;
        }

        public async Task<List<AppData>> UpdateDataAsync()
        {
            List<AppData> apps = null;
            try
            {
                EbMobileSolutionData data = await App.Settings.GetSolutionDataAsync(false);

                if (data != null)
                {
                    apps = data.Applications;
                }
            }
            catch (Exception ex)
            {
                EbLog.Error("Failed to get solution data :: " + ex.Message);
            }
            return apps;
        }
    }
}
