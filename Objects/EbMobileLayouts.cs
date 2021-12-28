using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

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

        public Grid GetGridObject(string parent, FormMode FormMode, NetworkMode NetWorkType, EbDataRow Context)
        {
            Grid grid = new Grid() { ColumnSpacing = 0 };

            List<EbMobileTableCell> tr0 = this.CellCollection.FindAll(tr => tr.RowIndex == 0);
            Dictionary<int, int> widthMap = tr0.Distinct().ToDictionary(item => item.ColIndex, item => item.Width);

            for (int r = 0; r < this.RowCount; r++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            for (int i = 0; i < this.ColumCount; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(widthMap[i], GridUnitType.Star)
                });
            }

            for (int i = 0; i < this.CellCollection.Count; i++)
            {
                EbMobileTableCell cell = this.CellCollection[i];

                if (cell.ControlCollection.Count > 0)
                {
                    EbMobileControl tbctrl = cell.ControlCollection[0];
                    tbctrl.Parent = parent;
                    View controlView;
                    if (Context == null)
                        controlView = tbctrl.Draw(FormMode, NetWorkType);
                    else
                        controlView = tbctrl.Draw(FormMode, NetWorkType, Context);

                    grid.Children.Add(controlView, i % this.ColumCount, i / this.ColumCount);
                }
            }
            return grid;
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
