using System.Collections.Generic;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Configuration
{
    public class AppContent
    {
        public Dictionary<string, string> NewSolution { set; get; }

        public Dictionary<string, string> MySolutions { set; get; }

        public Dictionary<string, string> Login { set; get; }
    }

    public class AppVendor
    {
        public bool AllowNotifications { set; get; }

        public string PrimaryColor { set; get; }

        public bool HasOfflineFeature { set; get; }

        public bool HasActions { set; get; }

        public bool HasAppSwitcher { set; get; }

        public bool HasLocationswitcher { set; get; }

        public bool HasSolutionSwitcher { set; get; }

        public string Logo { set; get; }

        public bool AllowMenuRefresh { set; get; }

        public string PoweredBy { set; get; }

        public AppContent Content { set; get; }

        public Color GetPrimaryColor()
        {
            return Color.FromHex(PrimaryColor);
        }
    }

    public static class EbSettings
    {
        public const string VendorName = "ExpressBase";

        public const string AppIcon = "@mipmap/icon_eb";

        public const string AppLabel = "EXPRESSbase";

        public const string StatusBar = "#0046bb";

        public const string SplashTheme = "@style/Theme.Splash";
    }

    public static class MoSettings
    {
        public const string VendorName = "MoveOn";

        public const string AppIcon = "@mipmap/icon_mo";

        public const string AppLabel = "Move On";

        public const string StatusBar = "#283b97";

        public const string SplashTheme = "@style/Theme.SplashMO";
    }

    public class Config
    {
        private const string vendor = MoSettings.VendorName;

        public const string AppIcon = MoSettings.AppIcon;

        public const string AppLabel = MoSettings.AppLabel;

        public const string StatusBarColor = MoSettings.StatusBar;

        public const string SplashTheme = MoSettings.SplashTheme;

        public static bool NFEnabled = false;

        public Dictionary<string, AppVendor> Vendors { set; get; }

        public AppVendor Current => Vendors[vendor];
    }
}
