using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.Services
{
    public class ApplicationService : IApplicationService
    {
        public async Task<ObservableCollection<AppData>> GetDataAsync()
        {
            await Task.Delay(1);

            List<AppData> applications = Utils.Applications;
            return applications?.ToObservableCollection();
        }

        public async Task UpdateDataAsync(ObservableCollection<AppData> collection)
        {
            try
            {
                EbMobileSolutionData data = await App.Settings.GetSolutionDataAsync(false);

                if (data != null)
                {
                    ObservableCollection<AppData> obs_collection = data.Applications.ToObservableCollection();
                    collection.Update(obs_collection);
                }
            }
            catch (Exception ex)
            {
                EbLog.Error("Failed to get solution data :: " + ex.Message);
            }
        }
    }
}
