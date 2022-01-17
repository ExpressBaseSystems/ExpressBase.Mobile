using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.Services
{
    public interface IApplicationService
    {
        List<AppData> GetDataAsync();

        Task<List<AppData>> UpdateDataAsync(Loader loader);
    }
}
