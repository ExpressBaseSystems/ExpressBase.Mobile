using System.Collections.Generic;

namespace ExpressBase.Mobile
{
    public class EbMobileTableLayout : EbMobileControl, ILayoutControl
    {
        public int RowCount { set; get; }

        public int ColumCount { set; get; }

        public List<EbMobileTableCell> CellCollection { set; get; }

        public override bool Hidden { set; get; }

        public override bool Unique { get; set; }

        public EbMobileTableLayout()
        {
            this.CellCollection = new List<EbMobileTableCell>();
        }
    }

    public class EbMobileTableCell : EbMobilePageBase
    {
        public int RowIndex { set; get; }

        public int ColIndex { set; get; }

        public int Width { set; get; }

        public List<EbMobileControl> ControlCollection { set; get; }

        public EbMobileTableCell()
        {
            this.ControlCollection = new List<EbMobileControl>();
        }

        public bool IsEmpty()
        {
            return this.ControlCollection.Count <= 0;
        }
    }
}
