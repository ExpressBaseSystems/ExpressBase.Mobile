using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.Services
{
    public interface IDataService
    {
        MobileDataResponse GetData(string refid, int limit, int offset, List<Param> param, List<SortColumn> sort, List<Param> search, bool is_powerselect);

        Task<MobileDataResponse> GetDataAsync(string refid, int limit, int offset, List<Param> param, List<SortColumn> sort, List<Param> search, bool is_powerselect);

        Task<MobileDataResponse> GetDataAsyncV2(string refid, int limit, int offset, List<Param> param, List<SortColumn> sort, List<Param> search);

        Task<MobileProfileData> GetProfileDataAsync(string refid, int loc_id);

        Task<MobileDataResponse> GetDataFlatAsync(string refid);

        Task<ApiFileResponse> GetFileAsync(EbFileCategory category, string filename);

        ApiFileResponse GetFile(EbFileCategory category, string filename);

        byte[] GetLocalFile(string filename);
    }
}
