using ExpressBase.Mobile.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Services
{
    public interface IMenuServices
    {
        Task<List<MobilePagesWraper>> GetDataAsync();

        Task<List<MobilePagesWraper>> UpdateDataAsync();

        Task<List<MobilePagesWraper>> GetFromMenuPreload(EbApiMeta apimeta);
    }
}
