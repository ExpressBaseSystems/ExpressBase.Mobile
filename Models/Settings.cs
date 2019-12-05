using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Services;
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
                return 1;
            }
            set
            {
            }
        }

        public static int UserId
        {
            get
            {
                return Convert.ToInt32(Store.GetValue(AppConst.USER_ID));
            }
        }
    }
}
