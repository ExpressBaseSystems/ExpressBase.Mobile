using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels.BaseModels;
using ExpressBase.Mobile.Views;
using ExpressBase.Mobile.Views.Dynamic;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels
{
    public class HomeViewModel : StaticBaseViewModel
    {
        private readonly IMenuServices menuServices;

        private List<MobilePagesWraper> _objectList;

        public List<MobilePagesWraper> ObjectList
        {
            get { return _objectList; }
            set
            {
                _objectList = value;
                NotifyPropertyChanged();
            }
        }

        public Command SyncButtonCommand => new Command(async () => await SyncButtonEvent());

        public Command MenuItemTappedCommand => new Command<object>(async (o) => await ItemTapedEvent(o));

        public Command MyActionsTappedcommand => new Command(async () => await MyActionsTapedEvent());

        public HomeViewModel()
        {
            PageTitle = App.Settings.CurrentApplication?.AppName;
            menuServices = new MenuServices();
        }

        public override async Task InitializeAsync()
        {
            try
            {
                MobilePageCollection collection = await menuServices.GetDataAsync();

                if (collection != null)
                {
                    ObjectList = collection.Pages;

                    await Task.Run(async () =>
                    {
                        await menuServices.DeployFormTables(collection.Pages);

                        await CommonServices.Instance.LoadLocalData(collection.Data);
                    });
                }
            }
            catch (Exception ex)
            {
                EbLog.Write(ex.Message);
            }
        }

        public async Task Refresh()
        {
            try
            {
                MobilePageCollection collection = await menuServices.GetDataAsync();

                if (collection != null)
                {
                    ObjectList.Clear();
                    ObjectList.AddRange(collection.Pages);

                    await Task.Run(async () =>
                    {
                        await menuServices.DeployFormTables(collection.Pages);
                        await CommonServices.Instance.LoadLocalData(collection.Data);
                    });
                }
            }
            catch (Exception ex)
            {
                EbLog.Write(ex.Message);
            }
        }

        private async Task SyncButtonEvent()
        {
            try
            {
                IToast toast = DependencyService.Get<IToast>();
                if (!Utils.HasInternet)
                {
                    toast.Show("You are not connected to internet !");
                    return;
                }

                await Task.Run(async () =>
                {
                    Device.BeginInvokeOnMainThread(() => { IsBusy = true; });

                    await IdentityService.AuthIfTokenExpiredAsync();

                    SyncResponse response = await menuServices.Sync();

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        IsBusy = false;
                        toast.Show(response.Message);
                    });
                });
            }
            catch (Exception ex)
            {
                EbLog.Write("ObjectRender_OnSyncClick" + ex.Message);
            }
        }

        private bool IsTapped;

        private async Task ItemTapedEvent(object obj)
        {
            if (IsTapped) return;

            MobilePagesWraper item = (obj as CustomShadowFrame).PageWraper;

            try
            {
                EbMobilePage page = HelperFunctions.GetPage(item.RefId);
                if (page == null) return;

                IsTapped = true;

                ContentPage renderer = this.GetPageByContainer(page);
                if (renderer != null)
                    await (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(renderer);

                IsTapped = false;
            }
            catch (Exception ex)
            {
                IsTapped = false;
                EbLog.Write("ObjectRender_ObjFrame_Clicked" + ex.Message);
            }
        }

        private ContentPage GetPageByContainer(EbMobilePage page)
        {
            ContentPage renderer = null;
            try
            {
                switch (page.Container)
                {
                    case EbMobileForm f:
                        renderer = new FormRender(page);
                        break;
                    case EbMobileVisualization v:
                        renderer = new ListRender(page);
                        break;
                    case EbMobileDashBoard d:
                        renderer = new DashBoardRender(page);
                        break;
                    case EbMobilePdf p:
                        renderer = new PdfRender(page);
                        break;
                    default:
                        EbLog.Write("inavlid container type");
                        break;
                }
            }
            catch (Exception ex)
            {
                EbLog.Write(ex.Message);
            }
            return renderer;
        }

        private async Task MyActionsTapedEvent()
        {
            try
            {
                if (!Utils.HasInternet)
                {
                    DependencyService.Get<IToast>().Show("You are not connected to internet.");
                    return;
                }

                await (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(new MyActions());
            }
            catch (Exception ex)
            {
                EbLog.Write(ex.Message);
            }
        }
    }
}
