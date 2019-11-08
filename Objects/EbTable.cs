using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace ExpressBase.Mobile
{
    public enum VerticalAlign
    {
        Top = 0,
        Middle = 1,
        Bottom = 2
    }

    public class EbTableLayout : EbControlContainer
    {

        public override List<EbControl> Controls { get; set; }
    }

    public class EbTableTd : EbControlContainer
    {
        public float WidthPercentage { get; set; }

        public VerticalAlign VerticalAlign { get; set; }

        public override List<EbControl> Controls { get; set; }
    }
}
