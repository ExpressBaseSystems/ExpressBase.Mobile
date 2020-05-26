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
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels
{
    public class MySolutionsViewModel : StaticBaseViewModel
    {
        private readonly ISolutionService solutionService;

        private string sid;

        private SolutionInfo current;

        private ObservableCollection<SolutionInfo> _mysolution;

        public ObservableCollection<SolutionInfo> MySolutions
        {
            set
            {
                _mysolution = value;
                NotifyPropertyChanged();
            }
            get { return _mysolution; }
        }

        public Command SolutionTapedCommand => new Command<object>(async (o) => await SolutionTapedEvent(o));

        public Command SolutionRemoveCommand => new Command<object>(async (o) => await SolutionRemoveEvent(o));

        public MySolutionsViewModel()
        {
            solutionService = new SolutionService();
        }

        public override async Task InitializeAsync()
        {
            current = App.Settings.CurrentSolution;
            sid = App.Settings.Sid;

            MySolutions = await solutionService.GetDataAsync();
        }

        private async Task SolutionTapedEvent(object obj)
        {
            try
            {
                SolutionInfo tapedInfo = (SolutionInfo)obj;
                if (tapedInfo.SolutionName == sid) return;

                await Store.SetJSONAsync(AppConst.SOLUTION_OBJ, tapedInfo);
                App.Settings.CurrentSolution = tapedInfo;

                await solutionService.ClearCached();
                await solutionService.CreateDB(tapedInfo.SolutionName);
                await solutionService.CreateDirectory();

                await Application.Current.MainPage.Navigation.PushAsync(new Login());
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }

        private async Task SolutionRemoveEvent(object obj)
        {
            try
            {
                SolutionInfo info = (SolutionInfo)obj;

                if (current != null && info.SolutionName == current.SolutionName && info.RootUrl == current.RootUrl)
                    return;
                this.MySolutions.Remove(info);
                await solutionService.UpdateSolutions(this.MySolutions);
            }
            catch (Exception)
            {

            }
        }

        public async Task AddSolution(string solutionUrl, ValidateSidResponse response)
        {
            try
            {
                sid = solutionUrl.Trim().Split('.')[0];

                SolutionInfo info = new SolutionInfo
                {
                    SolutionName = sid,
                    RootUrl = solutionUrl
                };

                await solutionService.SetDataAsync(info);

                info.IsCurrent = true;
                this.MySolutions.Add(info);

                await solutionService.ClearCached();
                await solutionService.CreateDB(sid);
                await solutionService.CreateDirectory();

                if (response.Logo != null)
                    await solutionService.SaveLogoAsync(sid, response.Logo);
            }
            catch (Exception ex)
            {
                Log.Write("SolutionSelect_ConfirmClicked" + ex.Message);
            }
        }

        public async Task<ValidateSidResponse> Validate(string url)
        {
            return await solutionService.ValidateSid(url);
        }

        public bool IsSolutionExist(string url)
        {
            url = url.Trim();
            return this.MySolutions.Any(item => item.SolutionName == url.Split('.')[0] && item.RootUrl == url);
        }
    }
}
