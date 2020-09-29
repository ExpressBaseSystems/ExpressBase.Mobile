using ExpressBase.Mobile.Models;
using System.Collections.Generic;

namespace ExpressBase.Mobile.Configuration
{
    /// <summary>
    /// Core settings class
    /// Contains App Constats like icon,theme,label etc
    /// </summary>
    public class EbBuildConfig
    {
        public static string VendorName = MoveOn.VendorName;

        public const string AppIcon = MoveOn.AppIcon;

        public const string AppLabel = MoveOn.AppLabel;

        public const string StatusBarColor = MoveOn.StatusBar;

        public const string SplashTheme = MoveOn.SplashTheme;

        public static bool NFEnabled = MoveOn.NFEnabled;

        public const string GMapAndroidKey = MoveOn.GMapAndroidKey;

        public const string GMapiOSKey = MoveOn.GMapiOSKey;

        public Dictionary<string, AppVendor> Vendors { set; get; }

        public AppVendor Current => Vendors[VendorName];

        public static EbAppVendors GetVendor()
        {
            if (VendorName == Expressbase.VendorName)
                return EbAppVendors.ExpressBase;
            else if (VendorName == MoveOn.VendorName)
                return EbAppVendors.MoveOn;
            else
                return EbAppVendors.kudumbaShree;
        }
    }
}
