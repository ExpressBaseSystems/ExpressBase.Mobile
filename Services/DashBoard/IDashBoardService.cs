using ExpressBase.Mobile.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.Services.DashBoard
{
    public interface IDashBoardService
    {
        Task<EbDataSet> GetDataAsync(string refid);

        Task<EbDataSet> GetLocalDataAsync(EbScript script);
    }
}
