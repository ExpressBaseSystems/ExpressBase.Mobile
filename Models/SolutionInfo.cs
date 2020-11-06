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

        public string SignUpPage { set; get; }

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
            if (SolutionObject == null)
                return false;
            return SolutionObject.IsMobileSignupEnabled(out _);
        }

        public EbMobilePage GetSignUpPage()
        {
            if (string.IsNullOrEmpty(SignUpPage))
                return null;
            string regexed = EbSerializers.JsonToNETSTD(SignUpPage);
            return EbSerializers.Json_Deserialize<EbMobilePage>(regexed);
        }
    }
}
