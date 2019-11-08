using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

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

    public interface IEBRootObject
    {

    }
}