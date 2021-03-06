﻿using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels.BaseModels;
using System;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.ViewModels
{
    public class NewSolutionViewModel : StaticBaseViewModel
    {
        private readonly ISolutionService solutionService;

        public string Logo => App.Settings.Vendor.Logo;

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
                    RootUrl = solutionUrl,
                    SolutionObject = response.SolutionObj,
                    SignUpPage = response.SignUpPage
                };

                await solutionService.SetDataAsync(info);
                await solutionService.ClearCached();
                await solutionService.CreateDB(sid);
                await solutionService.CreateDirectory();

                if (response.Logo != null)
                {
                    solutionService.SaveLogo(sid, response.Logo);
                }
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
        }

        public async Task RedirectToExistingSolution(string rooturl, bool masterPage)
        {
            SolutionInfo solInfo = solutionService.GetSolution(rooturl);

            if (solInfo != null)
            {
                SolutionInfo copy = solInfo.Clone();
                await Store.SetJSONAsync(AppConst.SOLUTION_OBJ, copy);
                App.Settings.CurrentSolution = copy;

                await App.Navigation.NavigateToLogin(masterPage);
            }
        }
    }
}
