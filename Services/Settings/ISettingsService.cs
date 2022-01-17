using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Models;
using RestSharp;
using System;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.Services
{
    public interface ISettingsService
    {
        Task InitializeSettings();

        void InitializeConfig();

        void ResetSettings();

        Task<EbMobileSolutionData> GetSolutionDataAsync(Loader loader);
    }
}
