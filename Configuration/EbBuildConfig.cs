using System.Collections.Generic;

namespace ExpressBase.Mobile.Configuration
{
    /// <summary>
    /// Core settings class
    /// Contains App Constats like icon,theme,label etc
    /// </summary>
    public class EbBuildConfig
    {
        public static string VendorName = KudumbaShree.VendorName;

        public const string AppIcon = KudumbaShree.AppIcon;

        public const string AppLabel = KudumbaShree.AppLabel;

        public const string StatusBarColor = KudumbaShree.StatusBar;

        public const string SplashTheme = KudumbaShree.SplashTheme;

        public static bool NFEnabled = KudumbaShree.NFEnabled;

        public Dictionary<string, AppVendor> Vendors { set; get; }

        public AppVendor Current => Vendors[VendorName];
    }
}
