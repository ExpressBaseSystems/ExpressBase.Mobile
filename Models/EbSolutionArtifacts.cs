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
            bool exist = false;
            refid = null;

            if (SolutionSettings != null && SolutionSettings.MobileAppSettings != null)
            {
                refid = SolutionSettings.MobileAppSettings.SignUpPageRefId;
                exist = SolutionSettings.MobileAppSettings.IsSignupEnabled();
            }
            return exist;
        }
    }

    public class SolutionSettings
    {
        public MobileAppSettings MobileAppSettings { get; set; }
    }

    public class MobileAppSettings
    {
        public string SignUpPageRefId { set; get; }

        public List<string> ProfileSetupPages { get; set; }

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
}
