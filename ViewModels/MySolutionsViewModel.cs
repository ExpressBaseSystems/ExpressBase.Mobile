using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels.BaseModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels
{
    public class MySolutionsViewModel : StaticBaseViewModel
    {
        private readonly ISolutionService solutionService;

        private string sid;

        private SolutionInfo current;

        private List<SolutionInfo> mySolutions;

        public List<SolutionInfo> MySolutions
        {
            set
            {
                mySolutions = value;
                NotifyPropertyChanged();
            }
            get => mySolutions;
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

                if (tapedInfo.SolutionName == sid && tapedInfo.RootUrl == current.RootUrl)
                    return;

                SolutionInfo copy = solutionService.Clone(tapedInfo);

                await Store.SetJSONAsync(AppConst.SOLUTION_OBJ, copy);
                App.Settings.CurrentSolution = copy;

                await solutionService.ClearCached();
                await solutionService.CreateDB(copy.SolutionName);
                await solutionService.CreateDirectory();

                Application.Current.MainPage = new NavigationPage();
                await NavigationService.LoginWithCS();
                App.RootMaster = null;
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
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
                await solutionService.Remove(info);
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
        }
    }
}
