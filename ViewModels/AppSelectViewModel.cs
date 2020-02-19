using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
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
    public class AppSelectViewModel : BaseViewModel
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
            string _apps = Store.GetValue(AppConst.APP_COLLECTION);

            if (_apps == null)
            {
                this.Applications = RestServices.Instance.GetAppCollections();
                this.Applications.OrderBy(x => x.AppName);

                Store.SetValue(AppConst.APP_COLLECTION, JsonConvert.SerializeObject(this.Applications));
            }
            else
                this.Applications = JsonConvert.DeserializeObject<List<AppData>>(_apps);
        }

        private async Task ItemSelected(object selected)
        {
            try
            {
                var apData = selected as AppData;

                if (Settings.AppId != apData.AppId)
                {
                    Store.Remove(AppConst.OBJ_COLLECTION);

                    if (Connectivity.NetworkAccess != NetworkAccess.Internet)
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

                await Store.SetValueAsync(AppConst.OBJ_COLLECTION, JsonConvert.SerializeObject(Coll.Pages));

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
