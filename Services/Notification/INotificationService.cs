using ExpressBase.Mobile.Models;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.Services
{
    public interface INotificationService
    {
        Task<string> GetAzureTokenAsync();

        Task<bool> CreateOrUpdateRegistration(string regId, DeviceRegistration device);

        Task<bool> UnRegisterAsync(string regId);

        Task UpdateNHRegisratation();
    }
}
