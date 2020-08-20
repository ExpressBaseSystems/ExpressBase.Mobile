using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using System.Collections.Generic;
using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls
{
    public class DGDynamicFrame : DynamicFrame
    {
        public DGDynamicFrame() { }

        public DGDynamicFrame(MobileTableRow row, List<EbMobileTableCell> cells, int rows, int colums, bool isHeader = false)
        {
            IsHeader = isHeader;

            this.CreateGrid(cells, rows, colums);
            this.FillTableColums(row, cells);

            this.Content = ContentGrid;
        }

        private void FillTableColums(MobileTableRow row, List<EbMobileTableCell> CellCollection)
        {
            foreach (EbMobileTableCell cell in CellCollection)
            {
                if (cell.ControlCollection.Count > 0)
                {
                    EbMobileDataColumn column = (EbMobileDataColumn)cell.ControlCollection[0];

                    string text = string.Empty;
                    MobileTableColumn tableColumn = row[column.ColumnName];

                    if (tableColumn != null)
                        text = tableColumn.Value.ToString();

                    Label label = new Label
                    {
                        Text = text,
                        VerticalTextAlignment = TextAlignment.Center,
                        VerticalOptions = LayoutOptions.Center
                    };

                    if (IsHeader)
                        label.FontFamily = (OnPlatform<string>)HelperFunctions.GetResourceValue("Roboto-Medium");
                    else
                        this.ApplyLabelStyle(label, column);

                    ContentGrid.Children.Add(label, cell.ColIndex, cell.RowIndex);
                }
            }
        }
    }
}
