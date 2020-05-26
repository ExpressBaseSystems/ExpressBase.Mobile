using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Extensions;
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

        public Command AppSelectedCommand => new Command(async (obj) => await ItemSelected(obj));

        public Command ApplicationSubmit => new Command(ResetClicked);

        public MyApplicationsViewModel()
        {
            applicationService = new ApplicationService();
        }

        public override async Task InitializeAsync()
        {
            Applications = await applicationService.GetDataAsync();
            FillRandomColor();
        }

        public async Task Refresh()
        {
            ObservableCollection<AppData> apps = await applicationService.GetDataAsync();
            Applications.Clear();
            Applications.AddRange(apps);
            FillRandomColor();
        }

        private async Task ItemSelected(object selected)
        {
            try
            {
                AppData apData = (AppData)selected;

                if (App.Settings.AppId != apData.AppId)
                {
                    Store.Remove(AppConst.OBJ_COLLECTION);

                    if (!Utils.HasInternet)
                    {
                        DependencyService.Get<IToast>().Show("Not connected to internet!");
                        return;
                    }

                    await Store.SetJSONAsync(AppConst.CURRENT_APP, apData);
                    App.Settings.CurrentApplication = apData;

                    App.RootMaster = new RootMaster(typeof(Views.Home));
                    Application.Current.MainPage = App.RootMaster;
                }
                else
                    await App.RootMaster.Detail.Navigation.PopAsync(true);
            }
            catch (Exception ex)
            {
                Log.Write("AppSelect_ItemSelected---" + ex.Message);
            }
        }

        private void FillRandomColor()
        {
            //fill by randdom colors
            Random random = new Random();

            foreach (AppData appdata in this.Applications)
            {
                var randomColor = ColorSet.Colors[random.Next(6)];
                appdata.BackgroundColor = Color.FromHex(randomColor.BackGround);
                appdata.TextColor = Color.FromHex(randomColor.TextColor);
            }
        }
    }
}
