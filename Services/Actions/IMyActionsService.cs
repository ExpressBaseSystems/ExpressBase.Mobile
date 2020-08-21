using ExpressBase.Mobile.Models;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.Services
{
    public interface IMyActionsService
    {
        Task<MyActionsResponse> GetMyActionsAsync();

        Task<EbStageInfo> GetMyActionInfoAsync(int stageid, string refid, int dataid);
    }
}
