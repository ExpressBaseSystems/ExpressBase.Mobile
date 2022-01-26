using System;
using ExpressBase.Mobile.Helpers;
using Xamarin.Forms;
using ExpressBase.Mobile.Enums;
using System.IO;

namespace ExpressBase.Mobile.Services
{
    public class CommonServices
    {
        private static CommonServices instance;

        public static CommonServices Instance => instance ??= new CommonServices();

        public static ImageSource GetLogo(string sid)
        {
            try
            {
                if (App.Settings.Vendor.BuildType == AppBuildType.Embedded)
                {
                    return ImageSource.FromFile(App.Settings.Vendor.Logo);
                }
                else
                {
                    INativeHelper helper = DependencyService.Get<INativeHelper>();

                    byte[] bytes = helper.GetFile($"{App.Settings.AppDirectory}/{sid.ToUpper()}/logo.png");
                    if (bytes != null)
                        return ImageSource.FromStream(() => new MemoryStream(bytes));
                }
            }
            catch (Exception ex)
            {
                EbLog.Error("GetLogo" + ex.Message);
            }
            return null;
        }
    }
}
