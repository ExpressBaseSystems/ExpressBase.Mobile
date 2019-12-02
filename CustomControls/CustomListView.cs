using ExpressBase.Mobile.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls
{
    public class CustomListView : StackLayout
    {
        public EbDataTable Table { set; get; }

        public EbMobileTableLayout TableLayout { set; get; }

        public CustomListView()
        {

        }

        public CustomListView(EbDataTable DT, EbMobileTableLayout TL)
        {
            Table = DT;
            TableLayout = TL;
            this.BuildUI();
        }

        void BuildUI()
        {
            foreach (EbDataRow _row in this.Table.Rows)
            {
                CustomFrame _Frame = new CustomFrame();
                Grid _G = this.CreateGrid(this.TableLayout.CellCollection);

                foreach (EbMobileTableCell _Cell in this.TableLayout.CellCollection)
                {
                    if (_Cell.ControlCollection.Count > 0)
                    {
                        EbMobileDataColumn _col = _Cell.ControlCollection[0] as EbMobileDataColumn;

                        _G.Children.Add(new Label { Text = _row[_col.ColumnName].ToString() }, _Cell.ColIndex, _Cell.RowIndex);
                    }
                }

                _Frame.Content = _G;
                this.Children.Add(_Frame);
            }
        }

        Grid CreateGrid(List<EbMobileTableCell> CellCollection)
        {
            Grid G = new Grid();

            for (int r = 0; r < TableLayout.RowCount; r++)
            {
                G.RowDefinitions.Add(new RowDefinition());
            }

            for (int c = 0; c < TableLayout.ColumCount; c++)
            {
                EbMobileTableCell current = CellCollection.Find(li => li.ColIndex == c && li.RowIndex == 0);

                G.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(current.Width,GridUnitType.Star) });
            }

            return G;
        }
    }
}
