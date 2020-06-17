using System;
using System.Collections.Generic;
using System.Text;
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
    }
}
