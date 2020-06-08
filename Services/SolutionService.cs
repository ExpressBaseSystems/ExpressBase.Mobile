using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Services
{
    public interface ISolutionService
    {
        Task<ObservableCollection<SolutionInfo>> GetDataAsync();

        Task<ValidateSidResponse> ValidateSid(string url);

        Task SetDataAsync(SolutionInfo info);

        Task SaveLogoAsync(string solutionname, byte[] imageByte);

        Task ClearCached();

        Task CreateDB(string slnName);

        Task CreateDirectory();

        Task Remove(SolutionInfo info);

        bool IsSolutionExist(string url);
    }

    public class SolutionService : ISolutionService
    {
        public static SolutionService Instance => new SolutionService();

        public async Task<ObservableCollection<SolutionInfo>> GetDataAsync()
        {
            ObservableCollection<SolutionInfo> sln = new ObservableCollection<SolutionInfo>();
            try
            {
                await Task.Delay(1);

                List<SolutionInfo> solutions = Store.GetJSON<List<SolutionInfo>>(AppConst.MYSOLUTIONS) ?? new List<SolutionInfo>();

                foreach (SolutionInfo info in solutions)
                {
                    info.SetLogo();
                    info.IsCurrent = (info.SolutionName == App.Settings.Sid && info.RootUrl == App.Settings.RootUrl) ? true : false;

                    sln.Add(info);//add to the observable collection
                }
            }
            catch (Exception ex)
            {
                EbLog.Write(ex.Message);
            }
            return sln;
        }

        public async Task<ValidateSidResponse> ValidateSid(string url)
        {
            ValidateSidResponse Vresp = new ValidateSidResponse();
            try
            {
                RestClient client = new RestClient("https://" + url);
                RestRequest request = new RestRequest("api/validate_solution", Method.GET);
                IRestResponse response = await client.ExecuteAsync(request);
                if (response.StatusCode == HttpStatusCode.OK)
                    Vresp = JsonConvert.DeserializeObject<ValidateSidResponse>(response.Content);
            }
            catch (Exception e)
            {
                Vresp.IsValid = false;
                EbLog.Write("Validate Sid---" + e.Message);
            }
            return Vresp;
        }

        public async Task SetDataAsync(SolutionInfo info)
        {
            try
            {
                List<SolutionInfo> sol = Store.GetJSON<List<SolutionInfo>>(AppConst.MYSOLUTIONS) ?? new List<SolutionInfo>();
                sol.Add(info);

                await Store.SetJSONAsync(AppConst.MYSOLUTIONS, sol);
                await Store.SetJSONAsync(AppConst.SOLUTION_OBJ, info);

                App.Settings.CurrentSolution = info;
            }
            catch (Exception ex)
            {
                EbLog.Write(ex.Message);
            }
        }

        public async Task SaveLogoAsync(string solutionname, byte[] imageByte)
        {
            try
            {
                await Task.Delay(1);

                INativeHelper helper = DependencyService.Get<INativeHelper>();

                if (!helper.DirectoryOrFileExist($"ExpressBase/{solutionname}/logo.png", SysContentType.File))
                    File.WriteAllBytes(helper.NativeRoot + $"/ExpressBase/{solutionname}/logo.png", imageByte);
            }
            catch (Exception ex)
            {
                EbLog.Write("SolutionSelect_SaveLogo" + ex.Message);
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
            await Task.Run(() =>
            {
                HelperFunctions.CreatePlatFormDir();
            });
        }

        public async Task Remove(SolutionInfo info)
        {
            List<SolutionInfo> sol = Store.GetJSON<List<SolutionInfo>>(AppConst.MYSOLUTIONS) ?? new List<SolutionInfo>();
            sol.Remove(sol.Find(item => item.SolutionName == info.SolutionName && item.RootUrl == info.RootUrl));
            await Store.SetJSONAsync(AppConst.MYSOLUTIONS, sol);
        }

        public bool IsSolutionExist(string url)
        {
            url = url.Trim();
            List<SolutionInfo> solutions = Store.GetJSON<List<SolutionInfo>>(AppConst.MYSOLUTIONS) ?? new List<SolutionInfo>();
            return solutions.Any(item => item.SolutionName == url.Split('.')[0] && item.RootUrl == url);
        }
    }
}
