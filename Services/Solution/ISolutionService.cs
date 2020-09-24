using ExpressBase.Mobile.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.Services
{
    public interface ISolutionService
    {
        Task<List<SolutionInfo>> GetDataAsync();

        Task<ValidateSidResponse> ValidateSid(string url);

        Task SetDataAsync(SolutionInfo info);

        Task SaveLogoAsync(string solutionname, byte[] imageByte);

        Task ClearCached();

        Task CreateDB(string slnName);

        Task CreateDirectory();

        SolutionInfo Clone(SolutionInfo info);

        Task Remove(SolutionInfo info);

        bool IsSolutionExist(string url);
    }
}
