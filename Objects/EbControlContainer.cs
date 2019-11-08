using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using System.Runtime.Serialization;

namespace ExpressBase.Mobile
{
    public enum EnumOperator
    {
        Equal,
        NotEqual,
        StartsWith,
        Contains,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual
    }

    
    public class EbControlContainer : EbControlUI
    {
        public virtual List<EbControl> Controls { get; set; }

        public override bool Hidden { get; set; }

        public override bool DoNotPersist { get; set; }

        public virtual bool isTableNameFromParent { get; set; }

        public virtual string TableName { get; set; }

        public virtual int TableRowId { get; set; }

    }
}
