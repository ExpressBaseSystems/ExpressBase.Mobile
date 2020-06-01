using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels.BaseModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels
{
    public class NewSolutionViewModel : StaticBaseViewModel
    {
        private readonly ISolutionService solutionService;

        public NewSolutionViewModel()
        {
            solutionService = new SolutionService();
        }

        public async Task<ValidateSidResponse> Validate(string url)
        {
            return await solutionService.ValidateSid(url);
        }

        public bool IsSolutionExist(string url)
        {
            return solutionService.IsSolutionExist(url);
        }

        public async Task AddSolution(string solutionUrl, ValidateSidResponse response)
        {
            try
            {
                string sid = solutionUrl.Split(CharConstants.DOT)[0];

                SolutionInfo info = new SolutionInfo
                {
                    SolutionName = sid,
                    RootUrl = solutionUrl
                };

                await solutionService.SetDataAsync(info);

                await solutionService.ClearCached();
                await solutionService.CreateDB(sid);
                await solutionService.CreateDirectory();

                if (response.Logo != null)
                    await solutionService.SaveLogoAsync(sid, response.Logo);
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }
    }
}
