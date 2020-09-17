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
        public static string VendorName = Expressbase.VendorName;

        public const string AppIcon = Expressbase.AppIcon;

        public const string AppLabel = Expressbase.AppLabel;

        public const string StatusBarColor = Expressbase.StatusBar;

        public const string SplashTheme = Expressbase.SplashTheme;

        public static bool NFEnabled = Expressbase.NFEnabled;

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
