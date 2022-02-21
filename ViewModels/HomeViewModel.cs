using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels.BaseModels;
using ExpressBase.Mobile.Views.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels
{
    public class HomeViewModel : StaticBaseViewModel
    {
        private readonly IMenuServices menuServices;

        private List<MobilePagesWraper> objectList;

        public List<MobilePagesWraper> ObjectList
        {
            get => objectList;
            set
            {
                objectList = value;
                NotifyPropertyChanged();
            }
        }

        private ImageSource solutionlogo;

        public ImageSource SolutionLogo
        {
            get => solutionlogo;
            set
            {
                solutionlogo = value;
                NotifyPropertyChanged();
            }
        }

        public bool HasBranding => App.Settings.Vendor.HasBrandingInHome;

        public bool HasLocationSwitcher => App.Settings.Vendor.HasLocationSwitcher && Utils.Locations.Count > 1;

        public bool RefreshOnAppearing => App.Settings.CurrentApplication.HasMenuApi();

        public Command MenuItemTappedCommand => new Command<MobilePagesWraper>(async (o) => await ItemTapedEvent(o));

        public Command RefreshDataCommand => new Command(async () => await UpdateAsync());

        public EbCPLayout EbLayout { get; set; }

        private bool isTapped;

        public HomeViewModel() : base(App.Settings.CurrentApplication?.AppName)
        {
            menuServices = new MenuServices();
        }

        public override async Task InitializeAsync()
        {
            try
            {
                ObjectList = await menuServices.GetDataAsync();
                UpdateIsEmptyFlag();

                SolutionLogo = CommonServices.GetLogo(App.Settings.Sid);
                await HelperFunctions.CreateDirectory("FILES");

                CreateFormContainersTables();
                LogApplicationInfo();
                await ShowMsgIfLatestAppAvailable();
            }
            catch (Exception ex)
            {
                EbLog.Error("Home page initialization data request failed ::" + ex.Message);
            }
        }

        public override async Task UpdateAsync()
        {
            try
            {
                if (!Utils.HasInternet)
                {
                    Utils.Alert_NoInternet();
                    IsRefreshing = false;
                    return;
                }

                //ObjectList = await menuServices.UpdateDataAsync();
                UpdateIsEmptyFlag();

                CreateFormContainersTables();
                LogApplicationInfo();
            }
            catch (Exception ex)
            {
                EbLog.Error("Home page update data request failed ::" + ex.Message);
            }
            IsRefreshing = false;
        }

        private async Task ItemTapedEvent(MobilePagesWraper item)
        {
            if (isTapped || EbPageHelper.IsShortTap())
                return;
            isTapped = true;
            Loader msgLoader = EbLayout.GetMessageLoader();
            msgLoader.IsVisible = true;
            msgLoader.Message = "Loading...";
            await Task.Delay(100);
            try
            {
                EbMobilePage page = EbPageHelper.GetPage(item.RefId);

                if (page == null)
                {
                    Utils.Toast("page not found");
                    EbLog.Error($"requested page with refid '{item.RefId}' not found");
                    msgLoader.IsVisible = false;
                    isTapped = false;
                    return;
                }

                bool render = true; string message = string.Empty;

                if (page.Container is EbMobileForm form)
                {
                    form.NetworkType = page.NetworkMode;
                    message = await EbPageHelper.ValidateFormRendering(form, msgLoader);
                    if (message != null) render = false;
                }

                if (render)
                {
                    EbLog.Info($"Rendering page '{page.Name}'");

                    ContentPage renderer = EbPageHelper.GetPageByContainer(page);
                    await App.Navigation.NavigateMasterAsync(renderer);
                }
                else
                    await App.Navigation.NavigateMasterAsync(new Redirect(message));
            }
            catch (Exception ex)
            {
                EbLog.Error("Failed to open page ::" + ex.Message);
            }
            msgLoader.IsVisible = false;
            isTapped = false;
        }

        public async Task LocationSwitched()
        {
            ObjectList = await menuServices.GetDataAsync();

            CreateFormContainersTables();
        }

        public async Task SyncData(Loader loader)
        {
            try
            {
                if (IdentityService.IsTokenExpired())
                {
                    await App.Navigation.NavigateToLogin(true);
                    return;
                }

                LocalDBServie service = new LocalDBServie();

                if (loader.IsVisible)
                    return;

                Device.BeginInvokeOnMainThread(() =>
                {
                    loader.IsVisible = true;
                    loader.Message = "Sync started...";
                });

                EbLog.Info("Sync started...");

                SyncResponse response = await service.PushDataToCloud(loader);
                string popupTitle = null, popupMsg = null;

                Device.BeginInvokeOnMainThread(() =>
                {
                    if (response.Status)
                        loader.Message = string.Empty;
                    else
                    {
                        loader.Message = response.Message + " \n";
                        popupTitle = "Warning";
                        popupMsg = response.Message;
                    }
                    loader.Message += "Fetching data from server...";
                });

                EbLog.Info("Fetching data from server...");

                response = await App.Settings.GetSolutionDataAsyncV2(loader);
                if (response.Status)
                {
                    Utils.Toast("Sync completed");
                    EbLog.Info("Sync completed");

                    App.Settings.MobilePages = App.Settings.CurrentApplication.MobilePages;
                    App.Settings.WebObjects = App.Settings.CurrentApplication.WebObjects;

                    ObjectList = await menuServices.GetDataAsync();

                    UpdateIsEmptyFlag();
                    CreateFormContainersTables();
                    LogApplicationInfo();

                    if (response.Message != null)
                    {
                        popupTitle = "Message";
                        popupMsg = response.Message;
                    }
                }
                else
                {
                    EbLog.Warning(response.Message ?? "Sync failed");
                    if (popupTitle == null) popupTitle = "Warning";
                    if (popupMsg != null) popupMsg += "\n";
                    popupMsg += response.Message;
                }

                if (popupTitle != null)
                    EbLayout.ShowMessage(popupTitle, popupMsg);
            }
            catch (Exception ex)
            {
                Utils.Toast("Failed to sync: " + ex.Message);
                EbLog.Error("Failed to sync::" + ex.Message);
            }

            Device.BeginInvokeOnMainThread(() => { loader.IsVisible = false; });
        }

        private void UpdateIsEmptyFlag()
        {
            IsEmpty = ObjectList == null || !ObjectList.Any();
        }

        private void LogApplicationInfo()
        {
            EbLog.Info($"Current solution : '{App.Settings.Sid}'");
            EbLog.Info($"Current Application :'{PageTitle}'");
            int objeCount = ObjectList == null ? 0 : ObjectList.Count;
            EbLog.Info($"Rendering total of {objeCount} pages with location id: {App.Settings.CurrentLocation?.LocId}");
        }

        private async Task ShowMsgIfLatestAppAvailable()
        {
            try
            {
                if (App.Settings.SyncInfo.LatestAppVersion != null)
                {
                    if (App.Settings.SyncInfo.AppUpdateLastNotifyTs < DateTime.Now.Subtract(new TimeSpan(4, 0, 0)))
                    {
                        INativeHelper helper = DependencyService.Get<INativeHelper>();
                        if (Version.Parse(App.Settings.SyncInfo.LatestAppVersion).CompareTo(Version.Parse(helper.AppVersion)) > 0)
                        {
                            string msg = $"Latest app version {App.Settings.SyncInfo.LatestAppVersion} is available. \n(Current version: {helper.AppVersion})";
                            EbLayout.ShowMessage("Update available", msg);
                            App.Settings.SyncInfo.AppUpdateLastNotifyTs = DateTime.Now;
                        }
                        else
                            App.Settings.SyncInfo.LatestAppVersion = null;
                        await Store.SetJSONAsync(AppConst.LAST_SYNC_INFO, App.Settings.SyncInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                EbLog.Error("ShowMsgIfLatestAppAvailable: " + ex.Message);
            }
        }

        private void CreateFormContainersTables()
        {
            if (ObjectList == null || ObjectList.Count <= 0) return;

            Task.Run(() =>
            {
                foreach (MobilePagesWraper wraper in ObjectList)
                {
                    EbMobilePage mpage = wraper.GetPage();

                    if (mpage != null && mpage.Container is EbMobileForm form)
                    {
                        if (mpage.NetworkMode != NetworkMode.Online && !form.RenderAsFilterDialog)
                        {
                            form.CreateTableSchema();
                        }
                    }
                }
            });
        }
    }
}
