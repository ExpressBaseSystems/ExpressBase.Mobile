using System.Collections.Generic;

namespace ExpressBase.Mobile.Models
{
    public class Eb_Solution
    {
        public string SolutionID { get; set; }

        public string ExtSolutionID { get; set; }

        public string SolutionName { get; set; }

        public string Description { get; set; }

        public string DateCreated { get; set; }

        public SolutionSettings SolutionSettings { get; set; }

        public SolutionType SolutionType { get; set; }

        public bool IsMobileSignupEnabled(out string refid)
        {
            refid = null;

            if (GetMobileSettings(out MobileAppSettings settings))
            {
                refid = settings.SignUpPageRefId;
                return settings.IsSignupEnabled();
            }
            return false;
        }

        public bool GetMobileSettings(out MobileAppSettings settings)
        {
            settings = null;

            if (SolutionSettings != null && SolutionSettings.MobileAppSettings != null)
            {
                settings = SolutionSettings.MobileAppSettings;
                return true;
            }
            return false;
        }

        public List<EbProfileUserType> GetUserTypeProfile()
        {
            if (GetMobileSettings(out MobileAppSettings settings))
            {
                return settings.UserTypeForms;
            }
            return null;
        }
    }

    public class SolutionSettings
    {
        public MobileAppSettings MobileAppSettings { get; set; }
    }

    public class MobileAppSettings
    {
        public string SignUpPageRefId { set; get; }

        public List<EbProfileUserType> UserTypeForms { get; set; }

        public bool IsSignupEnabled()
        {
            return !string.IsNullOrEmpty(SignUpPageRefId);
        }
    }

    public enum SolutionType
    {
        NORMAL = 1,
        PRIMARY = 2,
        REPLICA = 3
    }

    public class EbProfileUserType
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string RefId { get; set; }

        public bool HasUserTypeForm()
        {
            return !string.IsNullOrEmpty(RefId);
        }
    }
}
