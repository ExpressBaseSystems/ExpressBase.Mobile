using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Helpers
{
    public class GeoLocation
    {
        private static GeoLocation instance;

        public static GeoLocation Instance => instance ?? (instance = new GeoLocation());

        public async Task<Location> GetCurrentGeoLocation()
        {
            Location _loc = null;
            try
            {
                _loc = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium));

                if (_loc != null)
                {
                    _loc = await GetLastKnownGeoLocation();
                }
            }
            catch (Exception ex)
            {
                AlertExeptionMessage(ex);
            }
            return _loc;
        }

        public async Task<Location> GetLastKnownGeoLocation()
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

        public async Task<Placemark> GetAddressByCordinates(double lat, double lng)
        {
            Placemark placemark = null;
            try
            {
                var placemarks = await Geocoding.GetPlacemarksAsync(lat, lng);

                placemark = placemarks?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                AlertExeptionMessage(ex);
            }
            return placemark;
        }

        private void AlertExeptionMessage(Exception _Exception)
        {
            IToast toast = DependencyService.Get<IToast>();

            EbLog.Write(_Exception.Message);

            if (_Exception is FeatureNotSupportedException)
                toast.Show("Feature not supproted");
            else if (_Exception is FeatureNotEnabledException)
                toast.Show("Feature not enabled");
            else if (_Exception is PermissionException)
                toast.Show("No permission");
            else
                toast.Show("Something went wrong");
        }
    }
}
