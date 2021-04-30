using ExpressBase.Mobile.CustomControls.XControls;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using System.Collections.Generic;
using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls
{
    public class DGDynamicFrame : DynamicFrame
    {
        public DGDynamicFrame() { }

        public DGDynamicFrame(MobileTableRow row, EbMobileTableLayout layout, bool isHeader = false)
        {
            IsHeader = isHeader;

            DynamicGrid = new DynamicGrid(layout);
            this.FillTableColums(row, layout.CellCollection);

            this.Content = DynamicGrid;
        }

        private void FillTableColums(MobileTableRow row, List<EbMobileTableCell> CellCollection)
        {
            foreach (EbMobileTableCell cell in CellCollection)
            {
                if (cell.ControlCollection.Count > 0)
                {
                    EbMobileDataColumn column = (EbMobileDataColumn)cell.ControlCollection[0];

                    MobileTableColumn tableColumn = row[column.ColumnName];

                    if (tableColumn != null)
                    {
                        var value = tableColumn.DisplayValue ?? tableColumn.Value;

                        EbXLabel label = new EbXLabel(column)
                        {
                            Text = value?.ToString(),
                            VerticalOptions = LayoutOptions.FillAndExpand,
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            XBackgroundColor = Color.Transparent
                        };

                        if (IsHeader)
                        {
                            label.FontFamily = (OnPlatform<string>)HelperFunctions.GetResourceValue("Roboto-Medium");
                            label.LineBreakMode = LineBreakMode.WordWrap;
                        }
                        else
                        {
                            label.SetFont(column.Font, this.IsHeader);
                            label.SetTextWrap(column.TextWrap);
                        }

                        DynamicGrid.SetPosition(label, cell.RowIndex, cell.ColIndex, column.RowSpan, column.ColumnSpan);
                    }
                }
            }
        }
    }
}
