using System.Collections.Generic;

namespace ExpressBase.Mobile.Configuration
{
    public class EbBuildConfig
    {
        public static string VendorName = Expressbase.VendorName;

        public const string AppIcon = Expressbase.AppIcon;

        public const string AppLabel = Expressbase.AppLabel;

        public const string StatusBarColor = Expressbase.StatusBar;

        public const string SplashTheme = Expressbase.SplashTheme;

        public static bool NFEnabled = Expressbase.NFEnabled;

        public const string GMapAndroidKey = Expressbase.GMapAndroidKey;

        public const string GMapiOSKey = Expressbase.GMapiOSKey;

        public Dictionary<string, AppVendor> Vendors { set; get; }

        public AppVendor Current => Vendors[VendorName];
    }
}
