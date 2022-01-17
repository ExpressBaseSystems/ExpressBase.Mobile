using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels.BaseModels;
using ExpressBase.Mobile.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels
{
    public class MyApplicationsViewModel : StaticBaseViewModel
    {
        private readonly IApplicationService appService;

        private List<AppData> applications;

        private Loader msgLoader;

        public List<AppData> Applications
        {
            get => applications;
            set
            {
                applications = value;
                NotifyPropertyChanged();
            }
        }

        private bool isResetVisibile;

        public bool IsResetVisible
        {
            get => isResetVisibile;
            set
            {
                isResetVisibile = value;
                NotifyPropertyChanged();
            }
        }

        public bool ShowCurrentLocation => App.Settings.Vendor.HasLocationSwitcher;

        public bool IsInternal { set; get; }

        public Command AppSelectedCommand => new Command<AppData>(async (obj) => await AppSelected(obj));

        public Command RefreshListCommand => new Command(async () => await UpdateAsync());

        public MyApplicationsViewModel(Loader loader)
        {
            msgLoader = loader;
            appService = new ApplicationService();
        }

        public override void Initialize()
        {
            Applications = appService.GetDataAsync();
            IsEmpty = IsNullOrEmpty();
            IsResetVisible = IsEmpty;
        }

        public override async Task UpdateAsync()
        {
            Applications = await appService.UpdateDataAsync(msgLoader);
            IsEmpty = IsNullOrEmpty();
            IsRefreshing = false;
            IsResetVisible = IsEmpty;
        }

        public async Task AppSelected(AppData app)
        {
            try
            {
                if (IsCurrent(app.AppId))
                {
                    await App.Navigation.PopMasterAsync(true);
                }
                else
                {
                    await Store.SetJSONAsync(AppConst.CURRENT_APP, app);
                    App.Settings.CurrentApplication = app;
                    App.Settings.MobilePages = app.MobilePages;

                    App.RootMaster = new RootMaster();
                    Application.Current.MainPage = App.RootMaster;
                }
            }
            catch (Exception ex)
            {
                EbLog.Info("Error at AppSelected in applications view model");
                EbLog.Error(ex.Message);
            }
        }

        private bool IsCurrent(int id)
        {
            return (App.Settings.AppId == id);
        }

        public bool IsNullOrEmpty()
        {
            return this.Applications == null || !this.Applications.Any();
        }
    }
}
