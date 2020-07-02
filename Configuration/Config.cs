using ExpressBase.Mobile.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Configuration
{
    public class AppContent
    {
        public Dictionary<string, string> NewSolution { set; get; }

        public Dictionary<string, string> MySolutions { set; get; }
    }

    public class AppVendor
    {
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

    public class Config
    {
        private const string vendor = "ExpressBase";

        public const string StatusBarColor = "#068433";

        public Dictionary<string, AppVendor> Vendors { set; get; }

        public AppVendor Current => Vendors[vendor];

        public static T PopulateData<T>(string fileName)
        {
            Assembly assembly = typeof(App).GetTypeInfo().Assembly;

            T obj = default;
            try
            {
                Stream stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.Configuration.{fileName}");

                using (var reader = new StreamReader(stream))
                {
                    string jsonString = reader.ReadToEnd();
                    obj = JsonConvert.DeserializeObject<T>(jsonString);
                }
            }
            catch (Exception ex)
            {
                EbLog.Write("Failed to deserialize config" + ex.Message);
            }

            return obj;
        }
    }
}
