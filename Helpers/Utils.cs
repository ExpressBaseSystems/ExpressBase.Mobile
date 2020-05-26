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
        public static string RootUrl
        {
            get
            {
                return "https://" + Store.GetValue(AppConst.ROOT_URL);
            }
        }

        public static string BToken
        {
            get
            {
                return Store.GetValue(AppConst.BTOKEN);
            }
        }

        public static string RToken
        {
            get
            {
                return Store.GetValue(AppConst.RTOKEN);
            }
        }

        public static int LocationId
        {
            get
            {
                return Store.GetValue<int>(AppConst.CURRENT_LOCATION);
            }
        }

        public static EbLocation CurrentLocObject
        {
            get
            {
                return Locations.Find(item => item.LocId == LocationId);
            }
        }

        public static int UserId
        {
            get
            {
                string _id = Store.GetValue(AppConst.USER_ID);
                return (_id == null) ? 1 : Convert.ToInt32(_id);
            }
        }

        public static string UserName
        {
            get { return Store.GetValue(AppConst.USERNAME); }
        }

        public static string PassWord
        {
            get { return Store.GetValue(AppConst.PASSWORD); }
        }

        public static string SolutionId
        {
            get
            {
                return Store.GetValue(AppConst.SID);
            }
        }

        public static User UserObject
        {
            get
            {
                return Store.GetJSON<User>(AppConst.USER_OBJECT);
            }
        }

        public static List<EbLocation> Locations
        {
            get
            {
                return Store.GetJSON<List<EbLocation>>(AppConst.USER_LOCATIONS);
            }
        }

        public static int AppId
        {
            get
            {
                return Convert.ToInt32(Store.GetValue(AppConst.APPID));
            }
        }

        public static string AppName
        {
            get
            {
                return Store.GetValue(AppConst.APPNAME);
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
