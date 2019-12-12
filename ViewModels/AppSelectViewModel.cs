using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Views;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels
{
    public class AppSelectViewModel : BaseViewModel
    {
        public AppSelectViewModel()
        {
            PullApplications();
            AppSelectedCommand = new Command(ItemSelected);
            PageTitle = "Choose Application";
        }

        private int SelectedAppid { set; get; }

        private string SelectedAppName { set; get; }

        public IList<AppData> Applications { get; private set; }

        public Command AppSelectedCommand { get; set; }

        public Command ApplicationSubmit { get; set; }

        private void PullApplications()
        {
            string _apps = Store.GetValue(AppConst.APP_COLLECTION);

            if (_apps == null)
            {
                this.Applications = Api.GetAppCollections();
                Store.SetValue(AppConst.APP_COLLECTION, JsonConvert.SerializeObject(this.Applications));
            }
            else
            {
                this.Applications = JsonConvert.DeserializeObject<List<AppData>>(_apps);
            }
        }

        private void ItemSelected(object selected)
        {
            this.SelectedAppid = (selected as AppData).AppId;
            this.SelectedAppName = (selected as AppData).AppName;
            Store.SetValue(AppConst.APPID, this.SelectedAppid.ToString());
            Store.SetValue(AppConst.APPNAME, this.SelectedAppName.ToString());
            Application.Current.MainPage = new RootMaster(typeof(ObjectsRenderer));
        }
    }
}
