using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using System;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.Services.DashBoard
{
    public class DashBoardService : BaseService, IDashBoardService
    {
        public async Task<EbDataSet> GetDataAsync(string refid)
        {
            if (!string.IsNullOrEmpty(refid))
            {
                try
                {
                    MobileDataResponse response = await DataService.Instance.GetDataFlatAsync(refid);
                    return response?.Data;
                }
                catch (Exception ex)
                {
                    EbLog.Info("failed to fetch [dashboard] data");
                    EbLog.Error(ex.Message);
                }
            }
            return null;
        }

        public async Task<EbDataSet> GetLocalDataAsync(EbScript script)
        {
            return null;
        }
    }
}
