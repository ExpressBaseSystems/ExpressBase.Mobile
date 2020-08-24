using System.Collections.Generic;

namespace ExpressBase.Mobile.Configuration
{
    public class EbBuildConfig
    {
        public static string VendorName = MoveOn.VendorName;

        public const string AppIcon = MoveOn.AppIcon;

        public const string AppLabel = MoveOn.AppLabel;

        public const string StatusBarColor = MoveOn.StatusBar;

        public const string SplashTheme = MoveOn.SplashTheme;

        public static bool NFEnabled = MoveOn.NFEnabled;

        public Dictionary<string, AppVendor> Vendors { set; get; }

        public AppVendor Current => Vendors[VendorName];
    }
}
