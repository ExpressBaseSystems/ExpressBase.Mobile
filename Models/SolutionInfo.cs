using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using System;
using System.IO;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Models
{
    public class SolutionInfo
    {
        public string RootUrl { set; get; }

        public string SolutionName { set; get; }

        public ImageSource Logo { set; get; }

        public bool IsCurrent { set; get; }

        public string LastUser { set; get; }

        public LoginType LoginType { set; get; }

        public Eb_Solution SolutionObject { set; get; }

        public void SetLogo()
        {
            INativeHelper helper = DependencyService.Get<INativeHelper>();
            try
            {
                byte[] bytes = helper.GetFile($"{App.Settings.AppDirectory}/{this.SolutionName}/logo.png");

                if (bytes != null)
                    this.Logo = ImageSource.FromStream(() => new MemoryStream(bytes));
            }
            catch (Exception ex)
            {
                EbLog.Error("Login_SetLogo" + ex.Message);
            }
        }

        public bool SignupEnabled()
        {
            SolutionSettings settings = SolutionObject?.SolutionSettings;

            if (settings != null && settings.MobileAppSettings != null)
            {
                MobileAppSettings mobs = settings.MobileAppSettings;

                if (mobs.MobileSignUpSettings != null && mobs.MobileSignUpSettings.SignUp)
                {
                    return true;
                }
            }
            return false;
        }

        public MobileSignUpSettings GetSignUpSettings()
        {
            SolutionSettings settings = SolutionObject?.SolutionSettings;

            if (settings != null && settings.MobileAppSettings != null)
            {
                MobileAppSettings mobs = settings.MobileAppSettings;
                return mobs?.MobileSignUpSettings;
            }
            return null;
        }
    }
}
