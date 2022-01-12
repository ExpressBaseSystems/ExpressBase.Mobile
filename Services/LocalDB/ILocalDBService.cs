using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Models;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.Services
{
    public interface ILocalDBService
    {
        Task<SyncResponse> PushDataToCloud(Loader loader = null);
    }
}
