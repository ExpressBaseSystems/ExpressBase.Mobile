using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;

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

        public static List<MobilePagesWraper> Objects
        {
            get
            {
                return Store.GetJSON<List<MobilePagesWraper>>(AppConst.OBJ_COLLECTION);
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
    }
}
