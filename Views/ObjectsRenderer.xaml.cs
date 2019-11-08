using ExpressBase.Mobile;
using ExpressBase.Mobile.Structures;
using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Extensions;
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
using ExpressBase.Mobile.Views.Shared;
using ExpressBase.Mobile.Constants;

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

        public List<ObjWrap> ObjectList { set; get; }

        public ObjectsRenderer()
        {
            InitializeComponent();

            string _objlist = Store.GetValue(AppConst.OBJ_COLLECTION);
            if(_objlist == null)
            {
                this.ObjectList = this.GetObjectList();
                Store.SetValue(AppConst.OBJ_COLLECTION,JsonConvert.SerializeObject(this.ObjectList));
            }
            else
            {
                this.ObjectList = JsonConvert.DeserializeObject<List<ObjWrap>>(_objlist);
            }
            BindingContext = this;
        }

        private List<ObjWrap> GetObjectList()
        {
            List<ObjWrap> _objlist = new List<ObjWrap>();

            HttpClient client = new HttpClient();
            string content = string.Format("?appid={0}&locid={1}", this.AppId, this.LocationId);
            string uri = Settings.RootUrl + "api/objects_by_app" + content;

            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
            requestMessage.Headers.Add(AppConst.BTOKEN, Store.GetValue(AppConst.BTOKEN));
            requestMessage.Headers.Add(AppConst.RTOKEN, Store.GetValue(AppConst.RTOKEN));

            try
            {
                var response = client.SendAsync(requestMessage).Result;
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content.ReadAsStringAsync();
                    ObjectListToMob dict = JsonConvert.DeserializeObject<ObjectListToMob>(responseContent.Result);

                    foreach (KeyValuePair<int, List<ObjWrap>> pair in dict.ObjectTypes)
                    {
                        _objlist.AddRange(pair.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return _objlist;
        }

        void OnObjectSelected(ListView sender, EventArgs e)
        {
            ObjWrap item = (sender.SelectedItem as ObjWrap);
            try
            {
                if (item.EbObjectType == (int)EbObjectTypes.WebForm)
                {
                    (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(new FormRender(item.Refid));
                }
                else if (item.EbObjectType == (int)EbObjectTypes.Report)
                {
                    (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(new ReportRender());
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
    }
}