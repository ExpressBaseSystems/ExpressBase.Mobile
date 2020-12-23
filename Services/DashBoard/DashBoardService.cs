using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.Services.DashBoard
{
    public class DashBoardService : BaseService, IDashBoardService
    {
        public async Task<EbDataTable> GetDataAsync(string refid)
        {
            if (!string.IsNullOrEmpty(refid))
            {
                try
                {
                    MobileVisDataResponse response = await DataService.Instance.GetDataAsync(refid, 0, 0, null, null, null, false);

                    if (response != null && response.Data != null)
                    {
                        if (response.Data.Tables.Count >= 2)
                        {
                            return response.Data.Tables[1];
                        }
                    }
                }
                catch (Exception ex)
                {
                    EbLog.Error(ex.Message);
                }
            }
            return null;
        }

        public async Task<EbDataTable> GetLocalDataAsync(EbScript script)
        {
            return null;
        }
    }
}
