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
        public static string PassWord
        {
            get { return Store.GetValue(AppConst.PASSWORD); }
        }

        public static List<EbLocation> Locations
        {
            get
            {
                return Store.GetJSON<List<EbLocation>>(AppConst.USER_LOCATIONS);
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

        public static void Alert_NoInternet()
        {
            DependencyService.Get<IToast>().Show("Not connected to internet!");
        }
    }
}
