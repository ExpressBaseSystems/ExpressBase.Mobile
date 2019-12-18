using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Views;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels
{
    public class ObjectsRenderViewModel : BaseViewModel
    {
        public int AppId
        {
            get
            {
                return Convert.ToInt32(Store.GetValue(AppConst.APPID));
            }
        }

        public string AppName
        {
            get
            {
                return Store.GetValue(AppConst.APPNAME);
            }
        }

        public int LocationId
        {
            get
            {
                return 1;
            }
        }

        private string _loaderMessage;
        public string LoaderMessage
        {
            get
            {
                return this._loaderMessage;
            }
            set
            {
                if (this._loaderMessage == value)
                {
                    return;
                }
                this._loaderMessage = value;
                this.NotifyPropertyChanged();
            }
        }

        public List<MobilePagesWraper> ObjectList { set; get; }

        public Command SyncButtonCommand { set; get; }

        public Command ObjectSelectCommand { set; get; }

        public ObjectsRenderViewModel()
        {
            LoaderMessage = "Loading...";
            SetUpData();
            SyncButtonCommand = new Command(OnSyncClick);
            ObjectSelectCommand = new Command(OnObjectClick);
            PageTitle = AppName;
        }

        private void SetUpData()
        {
            string _objlist = Store.GetValue(AppConst.OBJ_COLLECTION);

            if (_objlist == null)
            {
                bool _pull = this.PulledTableExist() ? false : true;

                MobilePageCollection Coll = Api.GetEbObjects(this.AppId, this.LocationId, _pull);
                this.ObjectList = Coll.Pages;

                Store.SetValue(AppConst.OBJ_COLLECTION, JsonConvert.SerializeObject(Coll.Pages));

                if (Coll.TableNames != null)
                {
                    Store.SetValue(string.Format(AppConst.APP_PULL_TABLE, this.AppId), string.Join(",", Coll.TableNames.ToArray()));

                    if (_pull == true && Coll.TableNames.Count > 0)
                    {
                        this.LoadDTAsync(Coll.Data);
                    }
                }
            }
            else
            {
                List<MobilePagesWraper> _list = JsonConvert.DeserializeObject<List<MobilePagesWraper>>(_objlist);
                this.ObjectList = _list;
            }
        }

        private bool PulledTableExist()
        {
            string sv = Store.GetValue(string.Format(AppConst.APP_PULL_TABLE, this.AppId));

            string[] Tables = (sv == null) ? new string[0] : sv.Split(',');

            if (Tables.Length <= 0)
                return false;

            foreach (string s in Tables)
            {
                var status = App.DataDB.DoScalar(string.Format(StaticQueries.TABLE_EXIST, s));
                if (Convert.ToInt32(status) <= 0)
                {
                    return false;
                }
            }
            return true;
        }

        private async void LoadDTAsync(EbDataSet DS)
        {
            if (DS.Tables.Count > 0)
            {
                CommonServices _service = new CommonServices();
                int st = await _service.LoadLocalData(DS);
            }
        }

        private void OnSyncClick(object sender)
        {
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    CommonServices services = new CommonServices();
                    services.PushFormData();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            else
            {
                DependencyService.Get<IToast>().Show("You are not connected to internet !");
            }
        }

        private void OnObjectClick(object obj)
        {
            MobilePagesWraper item = (obj as MobilePagesWraper);
            Task.Run(() =>
            {
                Device.BeginInvokeOnMainThread(() => IsBusy = true);
                try
                {
                    string regexed = EbSerializers.JsonToNETSTD(item.Json);
                    EbMobilePage page = EbSerializers.Json_Deserialize<EbMobilePage>(regexed);

                    if (page.Container is EbMobileForm)
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            IsBusy = false;
                            (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(new FormRender(page));
                        });
                    }
                    else if (page.Container is EbMobileVisualization)
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            IsBusy = false;
                            (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(new VisRender(page));
                        });
                    }
                }
                catch (Exception ex)
                {
                    Device.BeginInvokeOnMainThread(() => IsBusy = false);
                    Console.WriteLine(ex.StackTrace);
                }
            });
        }
    }
}
