using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Home : ContentPage
    {
        public IList<AppDataToMob> Applications { get; private set; }

        public int SelectedAppid { get; set; }

        public string SelectedAppName { get; set; }

        public Home()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            HttpClient client = new HttpClient();
            Uri uri = new Uri(Settings.RootUrl + "api/menu");
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, uri.ToString());

            requestMessage.Headers.Add(Constants.BTOKEN, Store.GetValue(Constants.BTOKEN));
            requestMessage.Headers.Add(Constants.RTOKEN, Store.GetValue(Constants.RTOKEN));
           
            try
            {
                var response = client.SendAsync(requestMessage).Result;
                //var response = client.GetAsync(_url).Result;
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content.ReadAsStringAsync();
                    List<AppDataToMob> apps = JsonConvert.DeserializeObject<MenuData>(responseContent.Result).Applications;
                    this.CreateList(apps);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void CreateList(List<AppDataToMob> apps)
        {
            this.Applications = apps;
            BindingContext = this;
        }

        void OnCheckBoxCheckedChanged(object sender, EventArgs e)
        {

        }

        void OnListViewItemSelected(ListView sender, EventArgs e)
        {
            this.SelectedAppid = (sender.SelectedItem as AppDataToMob).AppId;
            this.SelectedAppName = (sender.SelectedItem as AppDataToMob).AppName;
        }

        void AppSelectFinish(object sender, EventArgs e)
        {
            Store.SetValue(Constants.APPID, this.SelectedAppid.ToString());
            Store.SetValue(Constants.APPNAME, this.SelectedAppName.ToString());
            Application.Current.MainPage = new RootMaster(typeof(ObjectsRenderer));
        }
    }
}