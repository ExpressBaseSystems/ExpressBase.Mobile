using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
namespace ExpressBase.Mobile.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SolutionSelect : ContentPage
    {
        public bool Running { set; get; }

        public SolutionSelect()
        {
            InitializeComponent();

            NavigationPage.SetHasNavigationBar(this, false);
        }

        void StoreSidVal(object sender, EventArgs e)
        {
            string url = this.Sid.Text.Trim();

            if (!string.IsNullOrEmpty(url))
            {
                if (this.ValidateSid(url))
                {
                    string _sid = url.Split('.')[0];
                    Store.SetValue(AppConst.SID, _sid);
                    Store.SetValue(AppConst.ROOT_URL, url);
                    Application.Current.MainPage.Navigation.PushAsync(new Login(true));
                    this.CreateDB(_sid);
                }
                else
                {
                    Device.BeginInvokeOnMainThread(async () => {
                        await this.DisplayAlert("Alert!", "Invalid solution URL", "Ok");
                    });
                }
            }
        }

        private bool ValidateSid(string url)
        {
            try
            {
                RestClient client = new RestClient("https://" + url);
                IRestResponse response = client.Execute(new RestRequest(Method.GET));
                if (response.StatusCode == HttpStatusCode.OK)
                    return true;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return false;
        }

        public void CreateDB(string sid)
        {
            App.DataDB.CreateDB(sid);
        }
    }
}