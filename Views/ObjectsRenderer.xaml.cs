using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ExpressBase.Mobile.Constants;
using Xamarin.Essentials;
using ExpressBase.Mobile.Helpers;

namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ObjectsRenderer : ContentPage
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

        public List<MobilePagesWraper> ObjectList { set; get; }

        public ObjectsRenderer()
        {
            InitializeComponent();

            string _objlist = Store.GetValue(AppConst.OBJ_COLLECTION);
            if(_objlist == null)
            {
                this.ObjectList = Api.GetEbObjects(this.AppId,this.LocationId);
                Store.SetValue(AppConst.OBJ_COLLECTION,JsonConvert.SerializeObject(this.ObjectList));
            }
            else
            {
                this.ObjectList = JsonConvert.DeserializeObject<List<MobilePagesWraper>>(_objlist);
            }
            BindingContext = this;
        }

        void OnObjectSelected(ListView sender, EventArgs e)
        {
            MobilePagesWraper item = (sender.SelectedItem as MobilePagesWraper);
            try
            {
                string regexed = EbSerializers.JsonToNETSTD(item.Json);
                EbMobilePage page = EbSerializers.Json_Deserialize<EbMobilePage>(regexed);

                if (page.Container is EbMobileForm)
                {
                    (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(new FormRender(page));
                }
                else if (page.Container is EbMobileVisualization)
                {
                    (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(new VisRender(page));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        protected override bool OnBackButtonPressed()
        {
            Device.BeginInvokeOnMainThread(async () => {
                bool status = await this.DisplayAlert("Alert!", "Do you really want to exit?", "Yes", "No");
                if (status)
                {
                    INativeHelper nativeHelper = null;
                    nativeHelper = DependencyService.Get<INativeHelper>();
                    nativeHelper.CloseApp();
                }
            });
            return true;
        }

        void OnSyncClick(object sender, EventArgs e)
        {
            var current = Connectivity.NetworkAccess;

            if (current == NetworkAccess.Internet)
            {
                try
                {
                    CommonServices services = new CommonServices();
                    services.PushFormData();
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            else
            {
                DependencyService.Get<IToast>().Show("You are not connected to internet !");
            }
        }
    }
}