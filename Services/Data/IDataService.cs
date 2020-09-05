using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.Services
{
    public interface IDataService
    {
        VisualizationLiveData GetData(MobileVisDataRequest request);

        Task<VisualizationLiveData> GetDataAsync(MobileVisDataRequest request);

        VisualizationLiveData GetData(string datasorce_ref, List<Param> parameters, List<SortColumn> sortOrder, int limit, int offset, bool is_powerselect = false);

        Task<VisualizationLiveData> GetDataAsync(string datasorce_ref, List<Param> parameters, List<SortColumn> sortOrder, int limit, int offset, bool is_powerselect = false);

        Task<ApiFileResponse> GetFile(EbFileCategory category, string filename);

        Task<byte[]> GetLocalFile(string filename);
    }
}
