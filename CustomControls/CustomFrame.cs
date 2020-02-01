using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls
{
    public class CustomFrame : Frame
    {
        public bool IsHeader { set; get; }

        public ColumnColletion Columns { set; get; }

        public EbDataRow DataRow { set; get; }

        public CustomFrame() { }

        public CustomFrame(EbDataRow Row, ColumnColletion Columns, EbMobileVisualization Visualization, bool IsHeader = false)
        {
            this.IsHeader = IsHeader;
            this.DataRow = Row;
            this.Columns = Columns;
            var grid = this.CreateGrid(Visualization.DataLayout.CellCollection, Visualization.DataLayout.RowCount, Visualization.DataLayout.RowCount);

            if (!IsHeader)
            {
                this.Padding = new Thickness(10);
                this.RenderSyncFlag(grid);
            }
        }

        Grid CreateGrid(List<EbMobileTableCell> CellCollection, int RowCount, int ColumCount)
        {
            Grid G = new Grid { BackgroundColor = Color.Transparent };

            for (int r = 0; r < RowCount; r++)
            {
                G.RowDefinitions.Add(new RowDefinition());
            }
            for (int c = 0; c < ColumCount; c++)
            {
                EbMobileTableCell current = CellCollection.Find(li => li.ColIndex == c && li.RowIndex == 0);

                G.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(current.Width, GridUnitType.Star) });
            }

            FillData(CellCollection, G);
            this.Content = G;
            return G;
        }

        private void FillData(List<EbMobileTableCell> CellCollection, Grid OuterGrid)
        {
            foreach (EbMobileTableCell _Cell in CellCollection)
            {
                if (_Cell.ControlCollection.Count > 0)
                {
                    EbMobileDataColumn _col = _Cell.ControlCollection[0] as EbMobileDataColumn;

                    string _text;
                    if (!string.IsNullOrEmpty(_col.TextFormat))
                        _text = _col.TextFormat.Replace("{value}", this.DataRow[_col.ColumnName].ToString());
                    else
                        _text = this.DataRow[_col.ColumnName].ToString();

                    Label _label = new Label { Text = _text };

                    this.ApplyLabelStyle(_label, _col);

                    OuterGrid.Children.Add(_label, _Cell.ColIndex, _Cell.RowIndex);
                }
            }
        }

        private void ApplyLabelStyle(Label Label, EbMobileDataColumn DataColumn)
        {
            EbFont _font = DataColumn.Font;

            if (_font != null)
            {
                Label.FontSize = (this.IsHeader) ? (_font.Size + 4) : _font.Size;

                if (_font.Style == FontStyle.BOLD)
                    Label.FontAttributes = FontAttributes.Bold;
                else if (_font.Style == FontStyle.ITALIC)
                    Label.FontAttributes = FontAttributes.Italic;
                else
                    Label.FontAttributes = FontAttributes.None;

                Label.TextColor = (this.IsHeader) ? Color.White : Color.FromHex(_font.color.TrimStart('#'));

                if (_font.Caps)
                    Label.Text = Label.Text.ToUpper();

                if (_font.Underline)
                    Label.TextDecorations = TextDecorations.Underline;
                else if (_font.Strikethrough)
                    Label.TextDecorations = TextDecorations.Strikethrough;
            }
            else
            {
                if (this.IsHeader) Label.TextColor = Color.White;
            }
        }

        public void SetBackGroundColor(int RowIndex)
        {
            if (RowIndex % 2 == 0)
                this.BackgroundColor = Color.Default;
            else
                this.BackgroundColor = Color.FromHex("F2F2F2");
        }

        private void RenderSyncFlag(Grid grid)
        {
            EbDataColumn col = Columns.Find(item => item.ColumnName == "eb_synced");
            if (col == null)
                return;

            int synced = Convert.ToInt32(DataRow["eb_synced"]);

            var lbl = new Label
            {
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.End,
                TextColor = (synced == 0) ? Color.FromHex("ff5f5f") : Color.Green,
                Text = (synced == 0) ? "\uf06a" : "\uf058",
                FontFamily = (OnPlatform<string>)HelperFunctions.GetResourceValue("FontAwesome")
            };

            int colLength = grid.ColumnDefinitions.Count;

            grid.Children.Add(lbl, colLength - 1, 0);
        }
    }
}
