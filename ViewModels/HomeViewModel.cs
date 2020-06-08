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

        public Command MyActionsTappedcommand => new Command<object>(async (o) => await MyActionsTapedEvent(o));

        public HomeViewModel()
        {
            PageTitle = App.Settings.CurrentApplication?.AppName;
            menuServices = new MenuServices();
        }

        public override async Task InitializeAsync()
        {
            try
            {
                ObjectList = await menuServices.GetDataAsync();
                await menuServices.DeployFormTables(ObjectList);
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
                List<MobilePagesWraper> objList = await menuServices.GetDataAsync();

                ObjectList.Clear();
                ObjectList.AddRange(objList);
                await menuServices.DeployFormTables(ObjectList);
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
            IToast toast = DependencyService.Get<IToast>();

            try
            {
                EbMobilePage page = HelperFunctions.GetPage(item.RefId);
                if (page == null)
                {
                    toast.Show("This page is no longer available.");
                    return;
                }
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

        private async Task MyActionsTapedEvent(object obj)
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
