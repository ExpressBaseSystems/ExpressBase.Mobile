using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Services
{
    public interface ISolutionService
    {
        Task<ObservableCollection<SolutionInfo>> GetDataAsync();

        Task SetDataAsync(SolutionInfo info);

        Task SaveLogoAsync(string solutionname, byte[] imageByte);

        Task ClearCached();

        Task CreateDB(string slnName);

        Task CreateDirectory();

        Task RemoveSolution(SolutionInfo info);
    }

    public class SolutionService : ISolutionService
    {
        public async Task<ObservableCollection<SolutionInfo>> GetDataAsync()
        {
            ObservableCollection<SolutionInfo> sln = new ObservableCollection<SolutionInfo>();
            try
            {

                List<SolutionInfo> solutions = Store.GetJSON<List<SolutionInfo>>(AppConst.MYSOLUTIONS) ?? new List<SolutionInfo>();

                string current = await Store.GetValueAsync(AppConst.SID);

                foreach (SolutionInfo info in solutions)
                {
                    info.SetLogo();
                    info.IsCurrent = info.SolutionName == current ? true : false;

                    sln.Add(info);//add to the observable collection
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
            return sln;
        }

        public async Task SetDataAsync(SolutionInfo info)
        {
            try
            {
                List<SolutionInfo> sol = Store.GetJSON<List<SolutionInfo>>(AppConst.MYSOLUTIONS) ?? new List<SolutionInfo>();
                sol.Add(info);

                await Store.SetJSONAsync(AppConst.MYSOLUTIONS, sol);
                await Store.SetValueAsync(AppConst.SID, info.SolutionName);
                await Store.SetValueAsync(AppConst.ROOT_URL, info.RootUrl);
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
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
                Log.Write("SolutionSelect_SaveLogo" + ex.Message);
            }
        }

        public async Task ClearCached()
        {
            await Task.Delay(1);

            Store.RemoveJSON(AppConst.OBJ_COLLECTION);//remove obj collection
            Store.RemoveJSON(AppConst.APP_COLLECTION);
            Store.RemoveJSON(AppConst.USER_LOCATIONS);
            Store.RemoveJSON(AppConst.USER_OBJECT);
            Store.Remove(AppConst.APPID);
            Store.Remove(AppConst.USERNAME);
            Store.Remove(AppConst.BTOKEN);
            Store.Remove(AppConst.RTOKEN);
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

        public async Task RemoveSolution(SolutionInfo info)
        {
            List<SolutionInfo> sol = Store.GetJSON<List<SolutionInfo>>(AppConst.MYSOLUTIONS);
            sol.Remove(info);
            await Store.SetJSONAsync(AppConst.MYSOLUTIONS, sol);
        }
    }
}
