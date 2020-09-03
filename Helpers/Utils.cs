using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Helpers
{
    public class Utils
    {
        public static List<EbLocation> Locations
        {
            get
            {
                return Store.GetJSON<List<EbLocation>>(AppConst.USER_LOCATIONS) ?? new List<EbLocation>();
            }
        }

        public static bool HasInternet
        {
            get
            {
                return (Connectivity.NetworkAccess == NetworkAccess.Internet);
            }
        }

        public static List<SolutionInfo> Solutions
        {
            get
            {
                return Store.GetJSON<List<SolutionInfo>>(AppConst.MYSOLUTIONS) ?? new List<SolutionInfo>();
            }
        }

        public static List<AppData> Applications
        {
            get
            {
                return Store.GetJSON<List<AppData>>(AppConst.APP_COLLECTION) ?? new List<AppData>();
            }
        }

        private static IToast toastservice;

        public static IToast ToastService
        {
            get
            {
                if (toastservice == null)
                    toastservice = DependencyService.Get<IToast>();

                return toastservice;
            }
        }

        public static void Alert_NoInternet()
        {
            ToastService.Show("Not connected to internet!");
        }

        public static void Alert_SlowNetwork()
        {
            ToastService.Show("Slow network detected!");
        }

        public static void Alert_NetworkError()
        {
            ToastService.Show("Network error");
        }

        public static void Toast(string message)
        {
            ToastService.Show(message);
        }
    }
}
