using ExpressBase.Mobile.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls
{
    public class DynamicGrid : Grid
    {
        private readonly EbMobileTableLayout layout;

        private Dictionary<int, int> widthMap;

        public DynamicGrid(EbMobileTableLayout tableLayout)
        {
            this.layout = tableLayout;
            this.Initialize();
        }

        public void Initialize()
        {
            InitializeWidthMap();

            for (int r = 0; r < layout.RowCount; r++)
            {
                this.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            for (int i = 0; i < layout.ColumCount; i++)
            {
                this.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(widthMap[i], GridUnitType.Star)
                });
            }
        }

        private void InitializeWidthMap()
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

            view.SizeChanged += View_SizeChanged;
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