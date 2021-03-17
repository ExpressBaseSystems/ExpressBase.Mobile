﻿using ExpressBase.Mobile.Helpers;
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
                this.ObjectList = await menuServices.GetDataAsync();
                await menuServices.DeployFormTables(ObjectList);
                this.IsEmpty = IsObjectsEmpty();

                SolutionLogo = CommonServices.GetLogo(App.Settings.Sid);
                await HelperFunctions.CreateDirectory("FILES");

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
                    return;
                }

                this.ObjectList = await menuServices.UpdateDataAsync();
                this.IsEmpty = IsObjectsEmpty();

                await menuServices.DeployFormTables(ObjectList);
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
            await menuServices.DeployFormTables(ObjectList);
        }

        public bool IsObjectsEmpty()
        {
            return !ObjectList.Any();
        }

        private void LogApplicationInfo()
        {
            EbLog.Info($"Current solution : '{App.Settings.Sid}'");
            EbLog.Info($"Current Application :'{PageTitle}'");
            int objeCount = ObjectList == null ? 0 : ObjectList.Count;
            EbLog.Info($"Rendering total of {objeCount} pages with location id: {App.Settings.CurrentLocation?.LocId}");
        }
    }
}
