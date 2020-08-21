using ExpressBase.Mobile.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Services
{
    public interface IMenuServices
    {
        Task<SyncResponse> Sync();

        Task<List<MobilePagesWraper>> GetDataAsync();

        Task<List<MobilePagesWraper>> UpdateDataAsync();

        Task DeployFormTables(List<MobilePagesWraper> objlist);

        Task<List<MobilePagesWraper>> GetFromMenuPreload(EbApiMeta apimeta);

        Task<ImageSource> GetLogo(string sid);
    }
}
