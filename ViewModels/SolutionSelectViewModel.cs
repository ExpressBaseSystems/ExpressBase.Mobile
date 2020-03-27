using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.ViewModels.BaseModels;
using ExpressBase.Mobile.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels
{
    public class SolutionSelectViewModel : StaticBaseViewModel
    {
        private string solutionurtl;

        public string SolutionUrl
        {
            get { return this.solutionurtl; }
            set
            {
                this.solutionurtl = value;
                this.NotifyPropertyChanged();
            }
        }

        public List<SolutionInfo> MySolutions { set; get; }

        public Command SolutionTapedCommand => new Command(SolutionTapedEvent);

        private ValidateSidResponse ValidateSIDResponse { set; get; }

        public SolutionSelectViewModel()
        {
            try
            {
                this.MySolutions = Store.GetJSON<List<SolutionInfo>>(AppConst.MYSOLUTIONS) ?? new List<SolutionInfo>();
                foreach (var info in this.MySolutions)
                    info.SetLogo();
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }

        private void SolutionTapedEvent(object obj)
        {

        }

        public async Task AddSolution()
        {
            try
            {
                string _sid = this.SolutionUrl.Split('.')[0];

                SolutionInfo info = new SolutionInfo
                {
                    SolutionName = _sid,
                    RootUrl = this.SolutionUrl,
                    IsCurrent = true,
                };

                this.MySolutions.Add(info);
                Store.SetJSON(AppConst.MYSOLUTIONS, this.MySolutions);
                await Store.SetValueAsync(AppConst.SID, _sid);
                await Store.SetValueAsync(AppConst.ROOT_URL, this.SolutionUrl);

                this.ClearCached();

                App.DataDB.CreateDB(_sid);
                HelperFunctions.CreatePlatFormDir();

                if (ValidateSIDResponse.Logo != null)
                    this.SaveLogo(ValidateSIDResponse.Logo);
            }
            catch (Exception ex)
            {
                Log.Write("SolutionSelect_ConfirmClicked" + ex.Message);
            }
        }

        void SaveLogo(byte[] imageByte)
        {
            INativeHelper helper = DependencyService.Get<INativeHelper>();
            string sid = Settings.SolutionId;
            try
            {
                if (!helper.DirectoryOrFileExist($"ExpressBase/{sid}/logo.png", SysContentType.File))
                    File.WriteAllBytes(helper.NativeRoot + $"/ExpressBase/{sid}/logo.png", imageByte);
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
            Store.Remove(AppConst.APPID);
            Store.Remove(AppConst.USERNAME);
        }
    }
}
