using ExpressBase.Mobile.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.Services
{
    public interface IApplicationService
    {
        Task<ObservableCollection<AppData>> GetDataAsync();

        Task UpdateDataAsync(ObservableCollection<AppData> collection);
    }
}
