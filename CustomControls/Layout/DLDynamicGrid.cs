using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExpressBase.Mobile.CustomControls
{
    public class DLDynamicGrid : DynamicGrid
    {
        readonly EbMobileDataLink dataLink;

        public DLDynamicGrid() { }

        public DLDynamicGrid(EbMobileDataLink dataLink)
        {
            this.dataLink = dataLink;
            Initialize(dataLink.RowCount, dataLink.ColumCount);
        }

        protected override void InitializeWidthMap()
        {
            List<EbMobileDataCell> tr0 = dataLink.CellCollection.FindAll(tr => tr.RowIndex == 0);
            widthMap = tr0.Distinct().ToDictionary(item => item.ColIndex, item => item.Width);
        }
    }
}
