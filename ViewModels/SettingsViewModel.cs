using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels.BaseModels;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels
{
    public class SettingsViewModel : StaticBaseViewModel
    {
        public string LogFilePath { set; get; }

        public bool SyncVisibility => App.Settings.Vendor.HasOfflineFeature;

        public Command SyncButtonCommand => new Command(async () => await SyncLocalDataToCloud());

        public Command RefreshSolutionCommand => new Command(async () => await RefreshSolution());

        public SettingsViewModel()
        {
            Initialize();
        }

        public override void Initialize()
        {
            LogFilePath = $"../{App.Settings.AppDirectory}/{App.Settings.Sid}/log.txt";
        }

        private async Task SyncLocalDataToCloud()
        {
            try
            {
                if (IdentityService.IsTokenExpired())
                {
                    await App.Navigation.NavigateToLogin(true);
                    return;
                }

                LocalDBServie service = new LocalDBServie();

                Device.BeginInvokeOnMainThread(() => { IsBusy = true; });

                SyncResponse response = await service.PushDataToCloud();

                Device.BeginInvokeOnMainThread(() =>
                {
                    IsBusy = false;
                    Utils.Toast(response.Message);
                });
            }
            catch (Exception ex)
            {
                EbLog.Error("Failed to sync::" + ex.Message);
            }
        }

        private async Task RefreshSolution()
        {
            try
            {
                if (IdentityService.IsTokenExpired())
                {
                    await App.Navigation.NavigateToLogin(true);
                    return;
                }

                SolutionService service = new SolutionService();

                Device.BeginInvokeOnMainThread(() => { IsBusy = true; });

                ValidateSidResponse response = await service.ValidateSid(App.Settings.RootUrl.Replace(ApiConstants.PROTOCOL, string.Empty));

                if (response.IsValid)
                {
                    SolutionInfo Info = App.Settings.CurrentSolution.Clone();
                    Info.SignUpPage = response.SignUpPage;
                    Info.SolutionObject = response.SolutionObj;
                    await service.SetDataAsync(Info);
                }

                Device.BeginInvokeOnMainThread(() =>
                {
                    IsBusy = false;
                    Utils.Toast("settings updated");
                });
            }
            catch (Exception ex)
            {
                EbLog.Error("Failed to sync::" + ex.Message);
            }
        }
    }
}
