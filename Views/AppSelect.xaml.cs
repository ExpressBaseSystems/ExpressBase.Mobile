using ExpressBase.Mobile.Constants;
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
        public IList<AppDataToMob> Applications { get; private set; }

        public int SelectedAppid { get; set; }

        public string SelectedAppName { get; set; }

        public AppSelect()
        {
            InitializeComponent();
            NavigationPage.SetHasBackButton(this, false);

            string _apps = Store.GetValue(AppConst.APP_COLLECTION);
            if (_apps == null)
            {
                this.Applications = this.GetAppCollection();
                Store.SetValue(AppConst.APP_COLLECTION, JsonConvert.SerializeObject(this.Applications));
            }
            else
            {
                this.Applications = JsonConvert.DeserializeObject<List<AppDataToMob>>(_apps);
            }
            BindingContext = this;
        }

        //api call
        private List<AppDataToMob> GetAppCollection()
        {
            List<AppDataToMob> _Apps = null;

            HttpClient client = new HttpClient();
            Uri uri = new Uri(Settings.RootUrl + "api/menu");
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, uri.ToString());

            requestMessage.Headers.Add(AppConst.BTOKEN, Store.GetValue(AppConst.BTOKEN));
            requestMessage.Headers.Add(AppConst.RTOKEN, Store.GetValue(AppConst.RTOKEN));

            try
            {
                var response = client.SendAsync(requestMessage).Result;
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content.ReadAsStringAsync();
                    _Apps = JsonConvert.DeserializeObject<MenuData>(responseContent.Result).Applications;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return _Apps;
        }

        void OnListViewItemSelected(ListView sender, EventArgs e)
        {
            AppDataToMob App = (sender.SelectedItem as AppDataToMob);
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