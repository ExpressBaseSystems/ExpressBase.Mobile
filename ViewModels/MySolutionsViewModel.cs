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

        public string CurrentSolution { set; get; }

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

        public MySolutionsViewModel()
        {
            solutionService = new SolutionService();
        }

        public override async Task InitializeAsync()
        {
            CurrentSolution = await Store.GetValueAsync(AppConst.SID);

            MySolutions = await solutionService.GetDataAsync();
        }

        private async Task SolutionTapedEvent(object obj)
        {
            try
            {
                SolutionInfo tapedInfo = (SolutionInfo)obj;
                if (tapedInfo.SolutionName == CurrentSolution) return;

                await Store.SetValueAsync(AppConst.SID, tapedInfo.SolutionName);
                await Store.SetValueAsync(AppConst.ROOT_URL, tapedInfo.RootUrl);

                await solutionService.ClearCached();
                await solutionService.CreateDB(CurrentSolution);
                await solutionService.CreateDirectory();

                await Application.Current.MainPage.Navigation.PushAsync(new Login());
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

                await solutionService.SetDataAsync(info);

                info.IsCurrent = true;
                this.MySolutions.Add(info);

                await solutionService.ClearCached();
                await solutionService.CreateDB(CurrentSolution);
                await solutionService.CreateDirectory();

                RestServices.Instance.UpdateBaseUrl();

                if (response.Logo != null)
                    await solutionService.SaveLogoAsync(CurrentSolution, response.Logo);
            }
            catch (Exception ex)
            {
                Log.Write("SolutionSelect_ConfirmClicked" + ex.Message);
            }
        }

        public async Task RemoveSolution(string sname)
        {
            if (sname == this.CurrentSolution)
                return;

            SolutionInfo info = this.MySolutions.Single(item => item.SolutionName == sname);

            if (info != null)
            {
                this.MySolutions.Remove(info);
                await solutionService.RemoveSolution(info);
            }
        }
    }
}
