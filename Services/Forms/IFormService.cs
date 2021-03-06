﻿using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Models;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.Services
{
    public interface IFormService
    {
        Task<WebformData> GetFormLiveDataAsync(string refid, int row_id, int loc_id);

        Task<EbDataSet> GetFormLocalDataAsync(EbMobileForm form, int rowid);

        Task<PushResponse> SendFormDataAsync(string mobilePageRefid, WebformData WebFormData, int RowId, string WebFormRefId, int LocId);

        Task<ApiFileResponse> GetFile(EbFileCategory category, string filename);
    }
}
