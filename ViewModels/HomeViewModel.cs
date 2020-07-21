using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels.BaseModels;
using ExpressBase.Mobile.Views.Dynamic;
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

        private List<MobilePagesWraper> _objectList;

        public List<MobilePagesWraper> ObjectList
        {
            get => _objectList;
            set
            {
                _objectList = value;
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

        public Command SyncButtonCommand => new Command(async () => await SyncButtonEvent());

        public Command MenuItemTappedCommand => new Command<object>(async (o) => await ItemTapedEvent(o));

        private bool isTapped;

        public HomeViewModel()
        {
            PageTitle = App.Settings.CurrentApplication?.AppName;
            menuServices = new MenuServices();
        }

        public override async Task InitializeAsync()
        {
            ObjectList = await menuServices.GetDataAsync();

            SolutionLogo = await menuServices.GetLogo(App.Settings.Sid);

            await menuServices.DeployFormTables(ObjectList);
        }

        public override async Task UpdateAsync()
        {
            await menuServices.UpdateDataAsync(ObjectList);
        }

        private async Task SyncButtonEvent()
        {
            try
            {
                await Task.Run(async () =>
                {
                    Device.BeginInvokeOnMainThread(() => { IsBusy = true; });

                    await IdentityService.AuthIfTokenExpiredAsync();

                    SyncResponse response = await menuServices.Sync();

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        IsBusy = false;
                        DependencyService.Get<IToast>().Show(response.Message);
                    });
                });
            }
            catch (Exception ex)
            {
                EbLog.Write("Failed to sync::" + ex.Message);
            }
        }

        private async Task ItemTapedEvent(object obj)
        {
            if (isTapped) return;

            MobilePagesWraper item = (obj as CustomShadowFrame).PageWraper;

            try
            {
                EbMobilePage page = HelperFunctions.GetPage(item.RefId);
                if (page == null) return;

                isTapped = true;

                ContentPage renderer = this.GetPageByContainer(page);
                if (renderer != null)
                    await (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(renderer);

                isTapped = false;
            }
            catch (Exception ex)
            {
                isTapped = false;
                EbLog.Write("Failed to open page ::" + ex.Message);
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

        public async Task LocationSwitched()
        {
            List<MobilePagesWraper> objList = await menuServices.GetDataAsync();
            ObjectList.Update(objList);
            await menuServices.DeployFormTables(objList);
        }

        public bool IsEmpty()
        {
            return !ObjectList.Any();
        }
    }
}
