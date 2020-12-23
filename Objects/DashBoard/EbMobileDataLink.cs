using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using System.Collections.Generic;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbMobileDataLink : EbMobileDashBoardControl
    {
        public int RowCount { set; get; }

        public int ColumCount { set; get; }

        public string LinkRefId { get; set; }

        public List<EbMobileDataCell> CellCollection { set; get; }

        readonly List<EbMobileDashBoardControl> controls = new List<EbMobileDashBoardControl>();

        public override View Draw()
        {
            if (CellCollection == null) return null;

            EbXFrame frame = GetFrame();

            DLDynamicGrid grid = new DLDynamicGrid(this);
            frame.Content = grid;

            foreach (EbMobileDataCell cell in CellCollection)
            {
                if (cell.ControlCollection == null || cell.ControlCollection.Count <= 0)
                    continue;

                foreach (EbMobileDataLabel control in cell.ControlCollection)
                {
                    if(control is IGridAlignment gridAlign)
                    {
                        controls.Add(control);
                        var view = control.Draw();

                        grid.SetPosition(view, cell.RowIndex, cell.ColIndex, gridAlign.RowSpan, gridAlign.ColumnSpan);
                    }
                }
            }
            return frame;
        }

        public override void SetBindingValue(EbDataRow row)
        {
            foreach(var ctrl in controls)
            {
                ctrl.SetBindingValue(row);
            }
        }
    }

    public class EbMobileDataCell : EbMobilePage
    {
        public int RowIndex { set; get; }

        public int ColIndex { set; get; }

        public int Width { set; get; }

        public List<EbMobileDashBoardControl> ControlCollection { set; get; }
    }
}
