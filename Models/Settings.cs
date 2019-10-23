using ExpressBase.Mobile.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressBase.Mobile.Models
{
    public static class Settings
    {
        public static string RootUrl { get
            {
                return "https://" + Store.GetValue(Constants.SID) + ".eb-test.xyz/";
            }
        }
    }

    public static class Constants
    {
        public const string BTOKEN = "bToken";

        public const string RTOKEN = "rToken";

        public const string USER_ID = "userId";

        public const string DISPLAY_NAME = "displayName";

        public const string SID = "sid";

        public const string APPID = "appid";

        public const string APPNAME = "appname";

        public const string USERNAME = "username";

        public const string PASSWORD = "password";

    }

    public static class RegexConstants
    {
        public const string COR_LIB = "System.Private.CoreLib";

        public const string EB_COM = "ExpressBase.Common";

        public const string EB_M_COM = "ExpressBase.Mobile.Common";

        public const string EB_OBJ = "ExpressBase.Objects";

        public const string EB_M_OBJ = "ExpressBase.Mobile.Objects";

        public const string MS_LIB = "mscorlib";

    }
}
