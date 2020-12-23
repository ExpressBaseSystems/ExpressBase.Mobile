using ExpressBase.Mobile.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls
{
    public class DynamicGrid : Grid
    {
        private readonly EbMobileTableLayout layout;

        protected Dictionary<int, int> widthMap;

        public double XAllocated { set; get; }

        public DynamicGrid() { }

        public DynamicGrid(EbMobileTableLayout tableLayout)
        {
            this.layout = tableLayout;
            this.Initialize(layout.RowCount, layout.ColumCount);
        }

        protected void Initialize(int rowCount, int columnCount)
        {
            InitializeWidthMap();

            for (int r = 0; r < rowCount; r++)
            {
                this.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            for (int i = 0; i < columnCount; i++)
            {
                this.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(widthMap[i], GridUnitType.Star)
                });
            }
        }

        protected virtual void InitializeWidthMap()
        {
            List<EbMobileTableCell> tr0 = layout.CellCollection.FindAll(tr => tr.RowIndex == 0);
            widthMap = tr0.Distinct().ToDictionary(item => item.ColIndex, item => item.Width);
        }

        public void SetSpacing(int rowspace, int colspace)
        {
            this.RowSpacing = rowspace;
            this.ColumnSpacing = colspace;
        }

        public void SetPosition(View view, int rownum, int colnum, int rowspan = 0, int colspan = 0)
        {
            this.Children.Add(view, colnum, rownum);

            if (rowspan > 0)
                Grid.SetRowSpan(view, rowspan);

            if (colspan > 0)
                Grid.SetColumnSpan(view, colspan);

            TriggerSizeChanged(view, colnum);
        }

        private void TriggerSizeChanged(View view, int colnum)
        {
            if (view is IDynamicHeight dynH)
            {
                if (dynH.CalcHeight)
                {
                    view.HeightRequest = this.GetColumnWidth(colnum);
                    return;
                }
            }
            view.SizeChanged += View_SizeChanged;
        }

        private double GetColumnWidth(int colnum)
        {
            if (this.XAllocated != 0)
            {
                return this.XAllocated * widthMap[colnum] / 100;
            }
            return 0;
        }

        private void View_SizeChanged(object sender, EventArgs e)
        {
            var view = (View)sender;
            view.HeightRequest = view.Height;
        }

        public void ShowLinkIcon()
        {
            Label lbl = new Label
            {
                Style = (Style)HelperFunctions.GetResourceValue("ListViewLinkIconStyle")
            };

            this.Children.Add(lbl);
            Grid.SetRowSpan(lbl, this.RowDefinitions.Count);
            Grid.SetColumnSpan(lbl, this.ColumnDefinitions.Count);
        }
    }
}