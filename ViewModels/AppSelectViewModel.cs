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
        private int SelectedAppid { set; get; }

        private string SelectedAppName { set; get; }

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
            {
                this.Applications = JsonConvert.DeserializeObject<List<AppData>>(_apps);
            }
        }

        private async Task ItemSelected(object selected)
        {
            try
            {
                var apData = selected as AppData;

                if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    DependencyService.Get<IToast>().Show("Not connected to internet!");
                    return;
                }

                this.SelectedAppid = apData.AppId;
                this.SelectedAppName = apData.AppName;

                await Store.SetValueAsync(AppConst.APPID, this.SelectedAppid.ToString());
                await Store.SetValueAsync(AppConst.APPNAME, this.SelectedAppName.ToString());

                IsBusy = true;
                await PullObjectsByApp(this.SelectedAppid);

                IsBusy = false;
                App.RootMaster = new RootMaster(typeof(ObjectsRenderer));
                Application.Current.MainPage = App.RootMaster;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task PullObjectsByApp(int appid)
        {
            try
            {
                bool _pull = this.PulledTableExist() ? false : true;

                MobilePageCollection Coll = await RestServices.Instance.GetEbObjects(appid, Settings.LocationId, _pull);

                await Store.SetValueAsync(AppConst.OBJ_COLLECTION, JsonConvert.SerializeObject(Coll.Pages));

                if (Coll.TableNames != null)
                {
                    Store.SetValue(string.Format(AppConst.APP_PULL_TABLE, Settings.AppId), string.Join(",", Coll.TableNames.ToArray()));

                    if (_pull == true && Coll.TableNames.Count > 0)
                        await this.LoadDTAsync(Coll.Data);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private bool PulledTableExist()
        {
            string sv = Store.GetValue(string.Format(AppConst.APP_PULL_TABLE, Settings.AppId));

            string[] Tables = (sv == null) ? new string[0] : sv.Split(',');

            if (Tables.Length <= 0)
                return false;

            foreach (string s in Tables)
            {
                var status = App.DataDB.DoScalar(string.Format(StaticQueries.TABLE_EXIST, s));
                if (Convert.ToInt32(status) <= 0)
                    return false;
            }
            return true;
        }

        private async Task LoadDTAsync(EbDataSet DS)
        {
            if (DS.Tables.Count > 0)
                await CommonServices.Instance.LoadLocalData(DS);
        }
    }
}
