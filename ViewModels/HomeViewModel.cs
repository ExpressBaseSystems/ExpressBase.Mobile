using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Structures;
using ExpressBase.Mobile.ViewModels.BaseModels;
using ExpressBase.Mobile.Views.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        public Command SyncButtonCommand => new Command(OnSyncClick);

        public Command MenuItemTappedCommand => new Command<object>(async (o) => await ItemTapedEvent(o));

        public HomeViewModel()
        {
            PageTitle = Settings.AppName;
            menuServices = new MenuServices();
        }

        public override async Task InitializeAsync()
        {
            try
            {
                ObjectList = await menuServices.GetDataAsync(Settings.AppId, Settings.LocationId);
                await menuServices.DeployFormTables(ObjectList);
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }

        public async Task Refresh()
        {
            try
            {
                List<MobilePagesWraper> objList = await menuServices.GetDataAsync(Settings.AppId, Settings.LocationId);

                ObjectList.Clear();
                ObjectList.AddRange(objList);
                await menuServices.DeployFormTables(ObjectList);
            }
            catch(Exception ex)
            {
                Log.Write(ex.Message);
            }
        }

        private async void OnSyncClick(object sender)
        {
            IToast toast = DependencyService.Get<IToast>();
            if (!Settings.HasInternet)
            {
                toast.Show("You are not connected to internet !");
                return;
            }

            await Task.Run(() =>
            {
                try
                {
                    Device.BeginInvokeOnMainThread(() => { IsBusy = true; });

                    Auth.AuthIfTokenExpired();

                    SyncResponse response = SyncServices.Instance.Sync();

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        IsBusy = false;
                        toast.Show(response.Message);
                    });
                }
                catch (Exception ex)
                {
                    Log.Write("ObjectRender_OnSyncClick" + ex.Message);
                }
            });
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

                ContentPage renderer = await GetPageByContainer(page);
                if (renderer != null)
                    await (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(renderer);

                IsTapped = false;
            }
            catch (Exception ex)
            {
                IsTapped = false;
                Log.Write("ObjectRender_ObjFrame_Clicked" + ex.Message);
            }
        }

        private async Task<ContentPage> GetPageByContainer(EbMobilePage page)
        {
            ContentPage renderer = null;
            try
            {
                await Task.Delay(100);

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
                        Log.Write("inavlid container type");
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
            return renderer;
        }
    }
}
