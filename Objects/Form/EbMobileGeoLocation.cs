using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.CustomControls.Views;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Structures;
using ExpressBase.Mobile.Views.Shared;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbMobileGeoLocation : EbMobileControl
    {
        public override EbDbTypes EbDbType { get { return EbDbTypes.String; } set { } }

        public bool HideSearchBox { set; get; }

        public int ZoomLevel { set; get; }

        private GoogleMap mapView;

        public EbGeoLocation Cordinates { set; get; }

        public override void InitXControl(FormMode Mode, NetworkMode Network)
        {
            base.InitXControl(Mode, Network);

            this.XControl = new Frame()
            {
                Style = (Style)HelperFunctions.GetResourceValue("GeoLocFrame"),
                Content = mapView = new GoogleMap
                {
                    ChangeLocationEnabled = true
                }
            };

            mapView.ChangeLocationClicked += async (sender, e) =>
            {
                if (Utils.HasInternet)
                    await this.ShowMapFullScreen();
                else
                    Utils.Alert_NoInternet();
            };
            this.SetCordinates();
        }

        private async Task ShowMapFullScreen()
        {
            EbGeoLocation loc = new EbGeoLocation();

            if (Cordinates != null)
            {
                loc.Latitude = Cordinates.Latitude;
                loc.Longitude = Cordinates.Longitude;
            }

            EbMapBinding binding = new EbMapBinding
            {
                Location = loc,
                ResultCommand = new Command<EbGeoLocation>(async (obj) => await GetResult(obj))
            };

            await App.Navigation.NavigateModalByRenderer(new GeoMapView(binding));
        }

        private async Task GetResult(EbGeoLocation geoLocation)
        {
            Cordinates = geoLocation;
            mapView.SetLocation(Cordinates.Latitude, Cordinates.Longitude);
            await App.Navigation.PopModalByRenderer(true);
        }

        private async void SetCordinates()
        {
            try
            {
                bool hasPermission = await AppPermission.GPSLocation();

                if (!hasPermission)
                {
                    EbLog.Error("[location} permission revoked by user, failed to set dcurrent gps location");
                    Utils.Toast("Location permission revoked");
                    return;
                }

                Location current = await EbGeoLocationHelper.GetCurrentLocationAsync();

                if (current != null)
                {
                    Cordinates = new EbGeoLocation { Latitude = current.Latitude, Longitude = current.Longitude };
                    mapView.SetLocation(current.Latitude, current.Longitude);
                }
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
        }

        public override object GetValue()
        {
            if (Cordinates != null)
            {
                return $"{Cordinates.Latitude},{Cordinates.Longitude}";
            }
            return null;
        }

        public override void SetValue(object value)
        {
            if (value == null) return;
            try
            {
                string[] cordinates = value.ToString().Split(CharConstants.COMMA);

                if (cordinates.Length >= 2)
                {
                    double lat = Convert.ToDouble(cordinates[0]);
                    double lng = Convert.ToDouble(cordinates[1]);

                    Cordinates = new EbGeoLocation { Latitude = lat, Longitude = lng };
                    mapView.SetLocation(lat, lng);
                }
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
                EbLog.Error(ex.StackTrace);
            }
        }

        public override bool Validate()
        {
            string value = this.GetValue() as string;

            if (this.Required && string.IsNullOrEmpty(value))
                return false;

            return true;
        }
    }
}
