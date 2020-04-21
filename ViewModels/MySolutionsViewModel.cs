using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels.BaseModels;
using ExpressBase.Mobile.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels
{
    public class MySolutionsViewModel : StaticBaseViewModel
    {
        public string CurrentSolution { set; get; }

        public ObservableCollection<SolutionInfo> MySolutions { set; get; }

        public Command SolutionTapedCommand => new Command(SolutionTapedEvent);

        public MySolutionsViewModel()
        {
            try
            {
                List<SolutionInfo> solutions = Store.GetJSON<List<SolutionInfo>>(AppConst.MYSOLUTIONS) ?? new List<SolutionInfo>();
                this.MySolutions = new ObservableCollection<SolutionInfo>(solutions);

                CurrentSolution = Store.GetValue(AppConst.SID);
                foreach (var info in this.MySolutions)
                {
                    info.SetLogo();
                    info.IsCurrent = info.SolutionName == CurrentSolution ? true : false;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }

        private void SolutionTapedEvent(object obj)
        {
            try
            {
                SolutionInfo tapedInfo = obj as SolutionInfo;
                if (tapedInfo.SolutionName == CurrentSolution) return;

                Store.SetValue(AppConst.SID, tapedInfo.SolutionName);
                Store.SetValue(AppConst.ROOT_URL, tapedInfo.RootUrl);

                this.ClearCached();
                App.DataDB.CreateDB(tapedInfo.SolutionName);
                HelperFunctions.CreatePlatFormDir();
                RestServices.Instance.UpdateBaseUrl();

                Application.Current.MainPage.Navigation.PushAsync(new Login());
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }

        public async Task AddSolution(string solutionUrl, ValidateSidResponse response)
        {
            try
            {
                string _sid = solutionUrl.Trim().Split('.')[0];
                CurrentSolution = _sid;

                SolutionInfo info = new SolutionInfo
                {
                    SolutionName = _sid,
                    RootUrl = solutionUrl
                };

                List<SolutionInfo> sol = Store.GetJSON<List<SolutionInfo>>(AppConst.MYSOLUTIONS) ?? new List<SolutionInfo>();
                sol.Add(info);
                Store.SetJSON(AppConst.MYSOLUTIONS, sol);
                await Store.SetValueAsync(AppConst.SID, _sid);
                await Store.SetValueAsync(AppConst.ROOT_URL, solutionUrl);

                //update page with newly added solution
                info.IsCurrent = true;
                this.MySolutions.Add(info);

                this.ClearCached();
                App.DataDB.CreateDB(_sid);
                HelperFunctions.CreatePlatFormDir();
                RestServices.Instance.UpdateBaseUrl();

                if (response.Logo != null)
                    this.SaveLogo(response.Logo);
            }
            catch (Exception ex)
            {
                Log.Write("SolutionSelect_ConfirmClicked" + ex.Message);
            }
        }

        void SaveLogo(byte[] imageByte)
        {
            INativeHelper helper = DependencyService.Get<INativeHelper>();
            try
            {
                if (!helper.DirectoryOrFileExist($"ExpressBase/{CurrentSolution}/logo.png", SysContentType.File))
                    File.WriteAllBytes(helper.NativeRoot + $"/ExpressBase/{CurrentSolution}/logo.png", imageByte);
            }
            catch (Exception ex)
            {
                Log.Write("SolutionSelect_SaveLogo" + ex.Message);
            }
        }

        void ClearCached()
        {
            Store.RemoveJSON(AppConst.OBJ_COLLECTION);//remove obj collection
            Store.RemoveJSON(AppConst.APP_COLLECTION);
            Store.RemoveJSON(AppConst.USER_LOCATIONS);
            Store.RemoveJSON(AppConst.USER_OBJECT);
            Store.Remove(AppConst.APPID);
            Store.Remove(AppConst.USERNAME);
            Store.Remove(AppConst.BTOKEN);
            Store.Remove(AppConst.RTOKEN);
        }
    }
}
