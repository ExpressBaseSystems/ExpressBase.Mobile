using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressBase.Mobile.Models
{
    public class Settings
    {
        public static string RootUrl
        {
            get
            {
                return "https://" + Store.GetValue(AppConst.ROOT_URL);
            }
        }

        public static int LocationId
        {
            get
            {
                return Convert.ToInt32(Store.GetValue(AppConst.CURRENT_LOCATION));
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
                return JsonConvert.DeserializeObject<User>(Store.GetValue(AppConst.USER_OBJECT));
            }
        }

        public static List<EbLocation> Locations
        {
            get
            {
                return JsonConvert.DeserializeObject<List<EbLocation>>(Store.GetValue(AppConst.USER_LOCATIONS));
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
                return JsonConvert.DeserializeObject<List<MobilePagesWraper>>(Store.GetValue(AppConst.OBJ_COLLECTION));
            }
        }
    }
}
