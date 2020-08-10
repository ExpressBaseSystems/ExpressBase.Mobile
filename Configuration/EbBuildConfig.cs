using ExpressBase.Mobile.Enums;
using System.Collections.Generic;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Configuration
{
    public class AppContent
    {
        public Dictionary<string, string> NewSolution { set; get; }

        public Dictionary<string, string> MySolutions { set; get; }

        public Dictionary<string, string> Login { set; get; }

        public Dictionary<string, string> About { set; get; }
    }

    public class AppVendor
    {
        public AppBuildType BuildType { set; get; }

        public string SolutionURL { set; get; }

        public bool AllowNotifications { set; get; }

        public string PrimaryColor { set; get; }

        public bool HasOfflineFeature { set; get; }

        public bool HasActions { set; get; }

        public bool HasAppSwitcher { set; get; }

        public bool HasLocationswitcher { set; get; }

        public bool HasSolutionSwitcher { set; get; }

        public string Logo { set; get; }

        public string PoweredBy { set; get; }

        public LoginType DefaultLoginType { set; get; }

        public AppContent Content { set; get; }

        public Color GetPrimaryColor()
        {
            return Color.FromHex(PrimaryColor);
        }
    }

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
