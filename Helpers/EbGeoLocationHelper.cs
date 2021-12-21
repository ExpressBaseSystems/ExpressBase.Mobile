using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Helpers
{
    public class EbGeoLocationHelper
    {
        public static async Task<Location> GetCurrentLocationAsync()
        {
            try
            {
                return await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium));
            }
            catch (Exception ex)
            {
                AlertExeptionMessage(ex);
            }
            return null;
        }

        public static async Task<Location> GetLastKnownLocationAsync()
        {
            try
            {
                return await Geolocation.GetLastKnownLocationAsync();
            }
            catch (Exception ex)
            {
                AlertExeptionMessage(ex);
            }
            return null;
        }

        public static async Task<Placemark> GetAddressByCordinates(double lat, double lng)
        {
            Placemark placemark = null;
            try
            {
                IEnumerable<Placemark> placemarks = await Geocoding.GetPlacemarksAsync(lat, lng);

                placemark = placemarks?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                AlertExeptionMessage(ex);
            }
            return placemark;
        }

        private static void AlertExeptionMessage(Exception _Exception)
        {
            IToast toast = DependencyService.Get<IToast>();

            EbLog.Error(_Exception.Message);

            if (_Exception is FeatureNotSupportedException)
                toast.Show("Feature not supported");
            else if (_Exception is FeatureNotEnabledException)
                toast.Show("Feature not enabled");
            else if (_Exception is PermissionException)
                toast.Show("No permission");
            else
                toast.Show("Something went wrong");
        }
    }
}
