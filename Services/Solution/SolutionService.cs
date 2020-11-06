using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Services
{
    public class SolutionService : ISolutionService
    {
        public async Task<List<SolutionInfo>> GetDataAsync()
        {
            List<SolutionInfo> sln = new List<SolutionInfo>();
            try
            {
                await Task.Delay(1);

                List<SolutionInfo> solutions = Utils.Solutions;

                string _currentroot = App.Settings.RootUrl.Replace(ApiConstants.PROTOCOL, string.Empty);

                foreach (SolutionInfo info in solutions)
                {
                    info.SetLogo();
                    info.IsCurrent = (info.SolutionName == App.Settings.Sid && info.RootUrl == _currentroot);
                    sln.Add(info);
                }
            }
            catch (Exception ex)
            {
                EbLog.Info("Failed to get solution data");
                EbLog.Error(ex.Message);
            }
            return sln;
        }

        public async Task<ValidateSidResponse> ValidateSid(string url)
        {
            ValidateSidResponse response = null;

            RestClient client = new RestClient(ApiConstants.PROTOCOL + url);
            RestRequest request = new RestRequest(ApiConstants.VALIDATE_SOL, Method.GET);

            try
            {
                IRestResponse iresp = await client.ExecuteAsync(request);
                if (iresp.IsSuccessful)
                {
                    response = JsonConvert.DeserializeObject<ValidateSidResponse>(iresp.Content);
                }
            }
            catch (Exception e)
            {
                EbLog.Info("validate_solution api failure");
                EbLog.Error(e.Message);
            }
            //safe return
            return response ?? new ValidateSidResponse();
        }

        public async Task SetDataAsync(SolutionInfo info)
        {
            try
            {
                List<SolutionInfo> solutions = Utils.Solutions;

                if (App.Settings.Vendor.HasSolutionSwitcher)
                    solutions.Add(info);
                else
                {
                    solutions.Clear();
                    solutions.Add(info);
                }

                //store solution data to store
                await Store.SetJSONAsync(AppConst.MYSOLUTIONS, solutions);
                await Store.SetJSONAsync(AppConst.SOLUTION_OBJ, info);

                App.Settings.CurrentSolution = info;
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
        }

        public async Task SaveLogoAsync(string solutionname, byte[] imageByte)
        {
            try
            {
                await Task.Delay(1);

                INativeHelper helper = DependencyService.Get<INativeHelper>();
                string root = App.Settings.AppDirectory;

                if (!helper.Exist($"{root}/{solutionname}/logo.png", SysContentType.File))
                {
                    File.WriteAllBytes(helper.NativeRoot + $"/{root}/{solutionname}/logo.png", imageByte);
                }
            }
            catch (Exception ex)
            {
                EbLog.Info($"Unable to create logo for solution '{solutionname}'");
                EbLog.Error(ex.Message);
            }
        }

        public async Task ClearCached()
        {
            await Task.Run(() =>
            {
                Store.ResetCashedSolutionData();
            });
        }

        public async Task CreateDB(string slnName)
        {
            await Task.Run(() =>
            {
                App.DataDB.CreateDB(slnName);
            });
        }

        public async Task CreateDirectory()
        {
            await HelperFunctions.CreateDirectory();
        }

        public async Task Remove(SolutionInfo info)
        {
            List<SolutionInfo> sol = Utils.Solutions;

            sol.Remove(sol.Find(item => item.SolutionName == info.SolutionName && item.RootUrl == info.RootUrl));
            await Store.SetJSONAsync(AppConst.MYSOLUTIONS, sol);
        }

        public bool IsSolutionExist(string url)
        {
            if (!App.Settings.Vendor.HasSolutionSwitcher)
            {
                return false;
            }
            url = url.Trim();
            string sname = url.Split(CharConstants.DOT)[0];
            return Utils.Solutions.Any(item => item.SolutionName == sname && item.RootUrl == url);
        }
    }
}
