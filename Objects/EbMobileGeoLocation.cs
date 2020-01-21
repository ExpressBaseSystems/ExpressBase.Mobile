using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Structures;
using System;
using System.Linq;
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

        public string Place { set; get; }

        private WebView WebView { set; get; }

        public override void InitXControl(FormMode Mode)
        {
            this.BuildXControl();

            if(Mode != FormMode.EDIT)
            {
                this.SetCordinates();
            }
        }

        private void BuildXControl()
        {
            Frame _frame = new Frame()
            {
                HasShadow = false,
                Padding = new Thickness(0, 0, 0, 10),
                BorderColor = Color.FromHex("cccccc"),
                CornerRadius = 10.0f,
            };

            WebView = new WebView { HeightRequest = 300 };
            _frame.Content = WebView;

            this.XControl = _frame;
        }

        private async void SetCordinates()
        {
            Cordinates = await GetCurrentLocation();
            if (Cordinates != null)
            {
                Placemark plc = await GetAddressByCordinates(Cordinates.Latitude, Cordinates.Longitude);

                if (plc != null)
                {
                    Place = $"{plc.FeatureName},{plc.Locality},{plc.Locality}";

                    SetWebViewUrl(Cordinates.Latitude, Cordinates.Longitude, this.Place);
                }
            }
        }

        private void SetWebViewUrl(double lat, double lon, string place)
        {
            string url = $"{Settings.RootUrl}/api/map?bToken={AppConst.BTOKEN}&rToken={AppConst.RTOKEN}&type=GOOGLEMAP&latitude={lat}&longitude={lon}&place={place}";
            this.WebView.Source = new UrlWebViewSource { Url = url };
        }

        private async Task<Location> GetLastKnownLocation()
        {
            Location _loc = null;
            try
            {
                return await Geolocation.GetLastKnownLocationAsync();
            }
            catch (Exception ex)
            {
                AlertExeptionMessage(ex);
            }
            return _loc;
        }

        private async Task<Location> GetCurrentLocation()
        {
            Location _loc = null;
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Best);
                _loc = await Geolocation.GetLocationAsync(request);

                if (_loc != null)
                {
                    _loc = await GetLastKnownLocation();
                }
            }
            catch (Exception ex)
            {
                AlertExeptionMessage(ex);
            }
            return _loc;
        }

        private async Task<Placemark> GetAddressByCordinates(double lat, double lng)
        {
            Placemark placemark = null;
            try
            {
                var placemarks = await Geocoding.GetPlacemarksAsync(lat, lng);

                placemark = placemarks?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return placemark;
        }

        private void AlertExeptionMessage(Exception _Exception)
        {
            IToast toast = DependencyService.Get<IToast>();

            if (_Exception is FeatureNotSupportedException)
                toast.Show("Feature not supproted");
            else if (_Exception is FeatureNotEnabledException)
                toast.Show("Feature not enabled");
            else if (_Exception is PermissionException)
                toast.Show("No permission");
            else
                toast.Show("Something went wrong");
        }

        public override object GetValue()
        {
            try
            {
                Uri uri = new Uri((WebView.Source as UrlWebViewSource).Url);
                var query = HttpUtility.ParseQueryString(uri.Query);

                double lat = Convert.ToDouble(query.Get("latitude"));
                double lon = Convert.ToDouble(query.Get("longitude"));

                if (lat == Cordinates.Latitude && lon == Cordinates.Longitude)
                    return $"{Cordinates.Latitude},{Cordinates.Longitude}";
                else
                    return $"{lat},{lon}";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        public override bool SetValue(object value)
        {
            if (value == null)
                return false;

            string[] cordinates = (value as string).Split(',');
            if (cordinates.Length >= 2)
            {
                double lat = Convert.ToDouble(cordinates[0]);
                double lng = Convert.ToDouble(cordinates[1]);

                Task.Run(async () =>
                {
                    Placemark plc = await GetAddressByCordinates(lat, lng);
                    this.Place = $"{plc.FeatureName},{plc.Locality},{plc.Locality}";

                    SetWebViewUrl(lat, lng, this.Place);
                    WebView.Reload();
                });
            }
            return true;
        }
    }
}
