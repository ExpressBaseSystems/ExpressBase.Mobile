using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.Services
{
    public interface IDataService
    {
        MobileVisDataRespnse GetData(string refid, int limit, int offset, List<Param> param, List<SortColumn> sort, List<Param> search, bool is_powerselect);

        Task<MobileVisDataRespnse> GetDataAsync(string refid, int limit, int offset, List<Param> param, List<SortColumn> sort, List<Param> search, bool is_powerselect);

        Task<ApiFileResponse> GetFile(EbFileCategory category, string filename);

        Task<byte[]> GetLocalFile(string filename);
    }
}
