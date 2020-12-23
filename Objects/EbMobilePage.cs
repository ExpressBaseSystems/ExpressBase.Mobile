
namespace ExpressBase.Mobile
{
    public class EbObject
    {
        public virtual string Name { get; set; }

        public EbObject() { }

        public virtual string RefId { get; set; }

        public virtual string DisplayName { get; set; }

        public virtual string Description { get; set; }

        public virtual string VersionNumber { get; set; }

        public virtual string Status { get; set; }
    }

    public abstract class EbMobilePageBase : EbObject { }

    public class EbMobilePage : EbMobilePageBase
    {
        public EbMobileContainer Container { set; get; }

        public string Category { get; set; }

        public NetworkMode NetworkMode { get; set; }

        public string BackgroundColor { get; set; }

        public bool HideFromMenu { set; get; }

        public string Icon { set; get; }

        public string IconColor { get; set; }

        public string IconBackground { get; set; }
    }
}
