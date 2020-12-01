using System.Collections.Generic;

namespace ExpressBase.Mobile.Configuration
{
    public class EbBuildConfig
    {
        public static string VendorName = Tiwi.VendorName;

        public const string AppIcon = Tiwi.AppIcon;

        public const string AppLabel = Tiwi.AppLabel;

        public const string StatusBarColor = Tiwi.StatusBar;

        public const string SplashTheme = Tiwi.SplashTheme;

        public static bool NFEnabled = Tiwi.NFEnabled;

        public const string GMapAndroidKey = Tiwi.GMapAndroidKey;

        public const string GMapiOSKey = Tiwi.GMapiOSKey;

        public Dictionary<string, AppVendor> Vendors { set; get; }

        public AppVendor Current => Vendors[VendorName];
    }
}
