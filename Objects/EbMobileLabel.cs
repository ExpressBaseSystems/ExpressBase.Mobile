using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressBase.Mobile
{
    public class EbMobileLabel : EbMobileControl, INonPersistControl, IMobileAlignment, IGridSpan
    {
        public int BorderRadius { get; set; }

        public string Text { get; set; }

        public EbFont Font { get; set; }

        public MobileHorrizontalAlign HorrizontalAlign { set; get; }

        public MobileVerticalAlign VerticalAlign { set; get; }

        public int Height { set; get; }

        public int Width { set; get; }

        public int RowSpan { set; get; }

        public int ColumnSpan { set; get; }

        public string BackgroundColor { get; set; }
    }
}
