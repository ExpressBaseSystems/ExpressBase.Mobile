using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressBase.Mobile.Models
{
    public static class Settings
    {
        public static string RootUrl
        {
            get
            {
                return "https://" + Store.GetValue(AppConst.SID) + ".eb-test.xyz/";
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
    }
}
