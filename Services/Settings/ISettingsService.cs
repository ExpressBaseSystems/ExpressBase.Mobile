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

        Task<EbMobileSolutionData> GetSolutionDataAsync(bool export, int timeout, Action<ResponseStatus> callback);
    }
}
