using System.Threading.Tasks;
using Xamarin.Essentials;

namespace ExpressBase.Mobile.Helpers
{
    public class AppPermission
    {
        public static async Task<bool> ReadStorage()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.StorageRead>();
            }
            return (status == PermissionStatus.Granted);
        }

        public static async Task<bool> WriteStorage()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.StorageWrite>();
            }
            return (status == PermissionStatus.Granted);
        }

        public static async Task<bool> Camera()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Camera>();
            }
            return (status == PermissionStatus.Granted);
        }

        public static async Task<bool> HasStoragePermission()
        {
            var read = await ReadStorage();
            var write = await WriteStorage();

            return (read && write);
        }

        public static async Task<bool> GPSLocation()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }
            return status == PermissionStatus.Granted;
        }

        public static async Task<bool> Audio()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Microphone>();

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Microphone>();
            }
            return status == PermissionStatus.Granted;
        }
    }
}
