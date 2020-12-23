using ExpressBase.Mobile.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.Services.DashBoard
{
    public interface IDashBoardService
    {
        Task<EbDataTable> GetDataAsync(string refid);

        Task<EbDataTable> GetLocalDataAsync(EbScript script);
    }
}
