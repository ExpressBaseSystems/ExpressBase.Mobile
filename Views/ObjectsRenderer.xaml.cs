﻿using ExpressBase.Mobile.Common.Objects;
using ExpressBase.Mobile.CoreStructures;
using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Objects;
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
    public partial class ObjectsRenderer : ContentPage
    {
        public int AppId
        {
            get
            {
                return Convert.ToInt32(Store.GetValue(Constants.APPID));
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

            //this.ToolbarItems.AddToolBarItems(new CustomToolBar().ToolBar);

            this.ObjectList = new List<ObjWrap>();

            HttpClient client = new HttpClient();
            string content = string.Format("?appid={0}&locid={1}", this.AppId, this.LocationId);
            string uri = Settings.RootUrl + "api/objects_by_app" + content;

            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
            requestMessage.Headers.Add(Constants.BTOKEN, Store.GetValue(Constants.BTOKEN));
            requestMessage.Headers.Add(Constants.RTOKEN, Store.GetValue(Constants.RTOKEN));

            try
            {
                var response = client.SendAsync(requestMessage).Result;
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content.ReadAsStringAsync();
                    ObjectListToMob dict = JsonConvert.DeserializeObject<ObjectListToMob>(responseContent.Result);

                    foreach (KeyValuePair<string, List<ObjWrap>> pair in dict.Objects)
                    {
                        ObjectList.AddRange(pair.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            BindingContext = this;
        }

        void OnObjectSelected(ListView sender, EventArgs e)
        {
            ObjWrap item = (sender.SelectedItem as ObjWrap);
            EbObjectWrapper wraper = this.GetObjectByRef(item.Refid);
            try
            {
                if(wraper.EbObjectType == (int)EbObjectTypes.WebForm)
                {
                    Application.Current.MainPage = new NavigationPage(new FormRender(wraper));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        private EbObjectWrapper GetObjectByRef(string refid)
        {
            EbObjectWrapper wraper = null;
            HttpClient client = new HttpClient();
            string uri = Settings.RootUrl + string.Format("api/object_by_ref?refid={0}", refid);

            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
            requestMessage.Headers.Add(Constants.BTOKEN, Store.GetValue(Constants.BTOKEN));
            requestMessage.Headers.Add(Constants.RTOKEN, Store.GetValue(Constants.RTOKEN));

            try
            {
                var response = client.SendAsync(requestMessage).Result;
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content.ReadAsStringAsync();
                    wraper = JsonConvert.DeserializeObject<EbObjectWrapper>(responseContent.Result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return wraper;
        }
    }
}