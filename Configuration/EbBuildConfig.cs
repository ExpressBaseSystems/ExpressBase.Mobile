using System.Collections.Generic;

namespace ExpressBase.Mobile.Configuration
{
    public class EbBuildConfig
    {
        public static string VendorName = Hairocraft.VendorName;

        public const string AppIcon = Hairocraft.AppIcon;

        public const string AppLabel = Hairocraft.AppLabel;

        public const string StatusBarColor = Hairocraft.StatusBar;

        public const string SplashTheme = Hairocraft.SplashTheme;

        public static bool NFEnabled = Hairocraft.NFEnabled;

        public const string GMapAndroidKey = Hairocraft.GMapAndroidKey;

        public const string GMapiOSKey = Hairocraft.GMapiOSKey;

        public Dictionary<string, AppVendor> Vendors { set; get; }

        public AppVendor Current => Vendors[VendorName];
    }
}
