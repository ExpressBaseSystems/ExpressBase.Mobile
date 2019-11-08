using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Data.Common;
using ExpressBase.Mobile.Structures;

namespace ExpressBase.Mobile
{
    public class EbControl : EbObject
    {
        public virtual string EbSid { get; set; }

        public virtual string ToolNameAlias { get; set; }

        public virtual string ToolIconHtml { get; set; }

        public virtual string ToolHelpText { get { return string.Empty; } set { } }

        public string DBareHtml { get; set; }

        public virtual string ChildOf { get; set; }

        public virtual bool Unique { get; set; }

        public virtual string HelpText { get; set; }

        public virtual string UIchangeFns { get; set; }

        public virtual bool Required { get; set; }

        public virtual string DesignHtml4Bot { get; set; }

        public virtual bool isFullViewContol { get; set; }

        public virtual bool DoNotPersist { get; set; }

        public virtual bool IsReadOnly { get; set; }

        public virtual bool ReadOnly { get; set; }

        public virtual bool Hidden { get; set; }

        public virtual bool SkipPersist { get; set; }

        public virtual int TabIndex { get; set; }

        public virtual string Label { get; set; }

        public virtual EbDbTypes EbDbType { get; set; }
    }
}