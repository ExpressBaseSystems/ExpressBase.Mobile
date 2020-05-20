using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Structures;
using System;
using System.Threading.Tasks;
using System.Web;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbMobileGeoLocation : EbMobileControl
    {
        public override EbDbTypes EbDbType { get { return EbDbTypes.String; } set { } }

        public bool HideSearchBox { set; get; }

        private Location Cordinates { set; get; }

        private WebView WebView { set; get; }

        private ActivityIndicator Loader { set; get; }

        private string PlaceHolder => @"<html><head><style>body{height:100%;width:100%;display:flex;justify-content:center;
                                    align-items:center;flex-direction:column}</style></head><body style='background:#eee'> 
                                    <container style='line-height:1.5'><p style='text-align: center;'>
                                    @content@ </p> </container></body></html>";

        public override void InitXControl(FormMode Mode, NetworkMode Network)
        {
            base.InitXControl(Mode, Network);

            try
            {
                this.BuildXControl();

                if (this.NetworkType == NetworkMode.Online)
                {
                    if (Mode != FormMode.EDIT)
                        this.SetCordinates();
                }
                else if (this.NetworkType == NetworkMode.Mixed)
                {
                    if (Mode != FormMode.EDIT)
                    {
                        if (Settings.HasInternet)
                            this.SetCordinates();
                        else
                        {
                            Loader.IsVisible = false;
                            WebView.HeightRequest = 100;
                            WebView.Source = new HtmlWebViewSource
                            {
                                Html = PlaceHolder.Replace("@content@", "You are not connected to internet")
                            };
                            WebView.IsVisible = true;
                        }
                    }
                }
                else
                {
                    Loader.IsVisible = false;
                    WebView.HeightRequest = 100;
                    WebView.Source = new HtmlWebViewSource
                    {
                        Html = PlaceHolder.Replace("@content@", "Offline")
                    };
                    WebView.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }

        private void BuildXControl()
        {
            try
            {
                Loader = new ActivityIndicator
                {
                    WidthRequest = 25,
                    HeightRequest = 25,
                    Color = Color.FromHex("315eff"),
                    IsRunning = true,
                    HorizontalOptions = LayoutOptions.Center
                };

                Frame _frame = new Frame()
                {
                    BackgroundColor = Color.FromHex("eeeeee"),
                    HasShadow = false,
                    Padding = new Thickness(0, 0, 0, 10),
                    CornerRadius = 10.0f
                };

                StackLayout Layout = new StackLayout
                {
                    BackgroundColor = Color.Transparent,
                    Children = { Loader }
                };
                WebView = new WebView { HeightRequest = 300, IsVisible = false };
                WebView.Navigated += WebView_Navigated;
                Layout.Children.Add(WebView);
                _frame.Content = Layout;

                this.XControl = Layout;
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
                Log.Write(ex.StackTrace);
            }
        }

        private void WebView_Navigated(object sender, WebNavigatedEventArgs e)
        {
            Loader.IsVisible = false;
            WebView.IsVisible = true;
        }

        private async void SetCordinates()
        {
            if (Cordinates == null)
            {
                Cordinates = await GeoLocation.Instance.GetCurrentGeoLocation();
                this.SetWebViewUrl(Cordinates.Latitude, Cordinates.Longitude);
            }
            else
                this.SetWebViewUrl(Cordinates.Latitude, Cordinates.Longitude);
        }

        private void SetWebViewUrl(double lat, double lon)
        {
            Auth.AuthIfTokenExpired();//auth again if token expires

            string url = $"{Settings.RootUrl}/api/map?bToken={Settings.BToken}&rToken={Settings.RToken}&type=GOOGLEMAP&latitude={lat}&longitude={lon}";
            this.WebView.Source = new UrlWebViewSource { Url = url };
        }

        public override object GetValue()
        {
            try
            {
                if (WebView.Source is UrlWebViewSource)
                {
                    Uri uri = new Uri((WebView.Source as UrlWebViewSource).Url);
                    var query = HttpUtility.ParseQueryString(uri.Query);

                    double lat = Convert.ToDouble(query.Get("latitude"));
                    double lon = Convert.ToDouble(query.Get("longitude"));

                    if (Cordinates == null)
                        Cordinates = new Location { Latitude = lat, Longitude = lon };

                    return $"{lat},{lon}";
                }
            }
            catch (Exception ex)
            {
                Log.Write("EbMobileGeoLocation.GetValue" + ex.Message);
            }
            return null;
        }

        public override bool SetValue(object value)
        {
            try
            {
                if (value == null)
                    return false;

                string[] cordinates = (value as string).Split(',');
                if (cordinates.Length >= 2)
                {
                    double lat = Convert.ToDouble(cordinates[0]);
                    double lng = Convert.ToDouble(cordinates[1]);

                    this.SetWebViewUrl(lat, lng);
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
                Log.Write(ex.StackTrace);
            }
            return true;
        }
    }
}
