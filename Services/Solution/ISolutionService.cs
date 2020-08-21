using ExpressBase.Mobile.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.Services
{
    public interface ISolutionService
    {
        Task<ObservableCollection<SolutionInfo>> GetDataAsync();

        Task<ValidateSidResponse> ValidateSid(string url);

        Task SetDataAsync(SolutionInfo info);

        Task SaveLogoAsync(string solutionname, byte[] imageByte);

        Task ClearCached();

        Task CreateDB(string slnName);

        Task CreateDirectory();

        Task Remove(SolutionInfo info);

        bool IsSolutionExist(string url);
    }
}
