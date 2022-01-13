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
            if (isTapped) return;
            try
            {
                EbMobilePage page = EbPageHelper.GetPage(item.RefId);

                if (page == null)
                {
                    Utils.Toast("page not found");
                    EbLog.Error($"requested page with refid '{item.RefId}' not found");
                    return;
                }

                isTapped = true;

                bool render = true; string message = string.Empty;

                if (page.Container is EbMobileForm form)
                {
                    Device.BeginInvokeOnMainThread(() => IsBusy = true);
                    render = await EbPageHelper.ValidateFormRendering(form);
                    message = form.MessageOnFailed;
                    Device.BeginInvokeOnMainThread(() => IsBusy = false);
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
                Device.BeginInvokeOnMainThread(() => IsBusy = false);
                EbLog.Error("Failed to open page ::" + ex.Message);
            }
            isTapped = false;
        }

        public async Task LocationSwitched()
        {
            ObjectList = await menuServices.GetDataAsync();

            CreateFormContainersTables();
        }

        public async void SyncData(Loader loader)
        {
            try
            {
                if (IdentityService.IsTokenExpired())
                {
                    await App.Navigation.NavigateToLogin(true);
                    return;
                }

                LocalDBServie service = new LocalDBServie();

                Device.BeginInvokeOnMainThread(() =>
                {
                    loader.IsVisible = true;
                    loader.Message = "Sync started...";
                });

                SyncResponse response = await service.PushDataToCloud(loader);

                if (response.Status)
                {
                    Device.BeginInvokeOnMainThread(() => { loader.Message = "Fetching data from server..."; });

                    if (await App.Settings.GetSolutionDataAsync(loader))
                    {
                        Utils.Toast("Sync completed");
                        ObjectList = await menuServices.GetDataAsync();
                        UpdateIsEmptyFlag();
                    }
                    else
                        Utils.Toast("Sync failed");
                }
                else
                    Utils.Toast(response.Message);
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
                        if (mpage.NetworkMode != NetworkMode.Online)
                        {
                            form.CreateTableSchema();
                        }
                    }
                }
            });
        }
    }
}
