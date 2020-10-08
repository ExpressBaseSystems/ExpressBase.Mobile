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
    }

    public class SolutionSettings
    {
        public MobileAppSettings MobileAppSettings { get; set; }
    }

    public class MobileAppSettings
    {
        public MobileSignUpSettings MobileSignUpSettings { get; set; }
    }

    public class MobileSignUpSettings
    {
        public bool SignUp { get; set; }

        public bool Email { get; set; }

        public bool MobileNo { get; set; }

        public bool Password { get; set; }

        public bool Verification { get; set; }

        public List<string> ProfileSetupPages { get; set; }
    }

    public enum SolutionType
    {
        NORMAL = 1,
        PRIMARY = 2,
        REPLICA = 3
    }
}
