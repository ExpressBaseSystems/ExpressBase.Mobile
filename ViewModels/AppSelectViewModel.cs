using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels.BaseModels;
using ExpressBase.Mobile.Views;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels
{
    public class AppSelectViewModel : StaticBaseViewModel
    {
        public IList<AppData> Applications { get; private set; }

        public Command AppSelectedCommand => new Command(async (obj) => await ItemSelected(obj));

        public Command ApplicationSubmit => new Command(ResetClicked);

        public AppSelectViewModel()
        {
            PageTitle = "Choose Application";
            PullApplications();
        }

        private void PullApplications()
        {
            var _apps = Store.GetJSON<List<AppData>>(AppConst.APP_COLLECTION);

            if (_apps == null || _apps.Count <= 0)
            {
                this.Applications = RestServices.Instance.GetAppCollections();
                this.Applications.OrderBy(x => x.AppName);

                Store.SetJSON(AppConst.APP_COLLECTION, this.Applications);
            }
            else
                this.Applications = _apps;

            //fill by randdom colors
            Random random = new Random();
            foreach (AppData appdata in this.Applications)
            {
                var randomColor = ColorSet.Colors[random.Next(6)];
                appdata.BackgroundColor = Color.FromHex(randomColor.BackGround);
                appdata.TextColor = Color.FromHex(randomColor.TextColor);
            }
        }

        private async Task ItemSelected(object selected)
        {
            try
            {
                var apData = selected as AppData;

                if (Settings.AppId != apData.AppId)
                {
                    Store.Remove(AppConst.OBJ_COLLECTION);

                    if (!Settings.HasInternet)
                    {
                        DependencyService.Get<IToast>().Show("Not connected to internet!");
                        return;
                    }

                    await Store.SetValueAsync(AppConst.APPID, apData.AppId.ToString());
                    await Store.SetValueAsync(AppConst.APPNAME, apData.AppName);

                    IsBusy = true;
                    await PullObjectsByApp(apData.AppId);

                    IsBusy = false;
                    App.RootMaster = new RootMaster(typeof(ObjectsRenderer));
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

        private async Task PullObjectsByApp(int appid)
        {
            try
            {
                MobilePageCollection Coll = await RestServices.Instance.GetEbObjects(appid, Settings.LocationId, true);

                Store.SetJSON(AppConst.OBJ_COLLECTION, Coll.Pages);

                if (Coll.TableNames?.Count > 0)
                    await CommonServices.Instance.LoadLocalData(Coll.Data);
            }
            catch (Exception ex)
            {
                Log.Write("AppSelect_PullObjectsByApp---" + ex.Message);
            }
        }
    }
}
