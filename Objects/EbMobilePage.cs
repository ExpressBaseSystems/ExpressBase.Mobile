using ExpressBase.Mobile.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

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

    public abstract class EbMobilePageBase : EbObject
    {

    }

    public class EbMobilePage : EbMobilePageBase
    {
        public override string RefId { get; set; }

        public override string DisplayName { get; set; }

        public override string Description { get; set; }

        public override string VersionNumber { get; set; }

        public override string Status { get; set; }

        public EbMobileContainer Container { set; get; }
    }

    public class EbScript
    {
        public EbScript()
        {
            Code = Code ?? string.Empty;
        }

        public string Code { get; set; }

        public ScriptingLanguage Lang { get; set; }
    }
}
