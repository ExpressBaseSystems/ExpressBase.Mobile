using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels.BaseModels;
using ExpressBase.Mobile.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels
{
    public class MyApplicationsViewModel : StaticBaseViewModel
    {
        private readonly IApplicationService applicationService;

        private ObservableCollection<AppData> _applications;

        public ObservableCollection<AppData> Applications
        {
            get { return _applications; }
            set
            {
                _applications = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsResetVisible => (App.Settings.Vendor.BuildType != AppBuildType.Embedded);

        public Command AppSelectedCommand => new Command(async (obj) => await AppSelected(obj));

        public MyApplicationsViewModel()
        {
            applicationService = new ApplicationService();
        }

        public override async Task InitializeAsync()
        {
            Applications = await applicationService.GetDataAsync();
        }

        public override async Task UpdateAsync()
        {
            await applicationService.UpdateDataAsync(this.Applications);
        }

        public async Task AppSelected(object selected)
        {
            try
            {
                AppData apData = (AppData)selected;

                if (IsCurrent(apData.AppId))
                {
                    await App.RootMaster.Detail.Navigation.PopAsync(true);
                }
                else
                {
                    await Store.SetJSONAsync(AppConst.CURRENT_APP, apData);
                    App.Settings.CurrentApplication = apData;
                    App.Settings.MobilePages = apData.MobilePages;

                    App.RootMaster = new RootMaster(typeof(Home));
                    Application.Current.MainPage = App.RootMaster;
                }
            }
            catch (Exception ex)
            {
                EbLog.Error("Failed to switch application :: " + ex.Message);
            }
        }

        private bool IsCurrent(int id)
        {
            return (App.Settings.AppId == id);
        }

        public bool IsEmpty()
        {
            return !Applications.Any();
        }
    }
}
