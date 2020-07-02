using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels.BaseModels;
using ExpressBase.Mobile.Views;
using System;
using System.Collections.ObjectModel;
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

                SolutionInfo copy = Clone(tapedInfo);

                await Store.SetJSONAsync(AppConst.SOLUTION_OBJ, copy);
                App.Settings.CurrentSolution = copy;

                await solutionService.ClearCached();
                await solutionService.CreateDB(copy.SolutionName);
                await solutionService.CreateDirectory();

                Application.Current.MainPage = new NavigationPage()
                {
                    BarBackgroundColor = App.Settings.Vendor.GetPrimaryColor(),
                    BarTextColor = Color.White
                };
                await Application.Current.MainPage.Navigation.PushAsync(new Login());
                App.RootMaster = null;
            }
            catch (Exception ex)
            {
                EbLog.Write(ex.Message);
            }
        }

        private SolutionInfo Clone(SolutionInfo info)
        {
            return new SolutionInfo
            {
                SolutionName = info.SolutionName,
                RootUrl = info.RootUrl,
                LastUser = info.LastUser,
            };
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
                EbLog.Write(ex.Message);
            }
        }
    }
}
