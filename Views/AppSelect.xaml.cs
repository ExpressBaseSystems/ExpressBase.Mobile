using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppSelect : ContentPage
    {
        public IList<AppData> Applications { get; private set; }

        public int SelectedAppid { get; set; }

        public string SelectedAppName { get; set; }

        public AppSelect()
        {
            InitializeComponent();
            NavigationPage.SetHasBackButton(this, false);

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
            BindingContext = this;
        }
        
        void OnListViewItemSelected(ListView sender, EventArgs e)
        {
            AppData App = (sender.SelectedItem as AppData);
            this.SelectedAppid = App.AppId;
            this.SelectedAppName = App.AppName;
        }

        void AppSelectFinish(object sender, EventArgs e)
        {
            Store.SetValue(AppConst.APPID, this.SelectedAppid.ToString());
            Store.SetValue(AppConst.APPNAME, this.SelectedAppName.ToString());
            Application.Current.MainPage = new RootMaster(typeof(ObjectsRenderer));
        }
    }
}