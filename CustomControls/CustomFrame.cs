using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls
{
    public class CustomFrame : Frame
    {
        private readonly bool isHeader;

        public EbDataRow DataRow { set; get; }

        private Grid contentGrid;

        public CustomFrame() { }

        public CustomFrame(EbDataRow Row, EbMobileVisualization Visualization, bool IsHeader = false)
        {
            this.isHeader = IsHeader;
            this.DataRow = Row;

            this.CreateGrid(Visualization.DataLayout.CellCollection, Visualization.DataLayout.RowCount, Visualization.DataLayout.ColumCount);
            FillData(Visualization.DataLayout.CellCollection);
            if (!string.IsNullOrEmpty(Visualization.LinkRefId) && !IsHeader) this.ShowLinkIcon();
            this.Content = contentGrid;

            if (!IsHeader)
                this.Padding = new Thickness(10);
        }

        //for data grid
        public CustomFrame(MobileTableRow row, List<EbMobileTableCell> cellCollection, int rowCount, int columCount, bool isHeader = false)
        {
            this.isHeader = isHeader;
            this.CreateGrid(cellCollection, rowCount, columCount);
            this.FillTableColums(row, cellCollection);
            this.Content = contentGrid;
        }

        private void CreateGrid(List<EbMobileTableCell> CellCollection, int RowCount, int ColumCount)
        {
            contentGrid = new Grid { BackgroundColor = Color.Transparent };

            for (int r = 0; r < RowCount; r++)
            {
                contentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }
            for (int c = 0; c < ColumCount; c++)
            {
                EbMobileTableCell current = CellCollection.Find(li => li.ColIndex == c && li.RowIndex == 0);

                contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(current.Width, GridUnitType.Star) });
            }
        }

        private void FillData(List<EbMobileTableCell> CellCollection)
        {
            try
            {
                foreach (EbMobileTableCell _Cell in CellCollection)
                {
                    if (_Cell.ControlCollection.Count > 0)
                    {
                        EbMobileDataColumn _col = (EbMobileDataColumn)_Cell.ControlCollection[0];

                        string _text;
                        var data = this.DataRow[_col.ColumnName];

                        if (!string.IsNullOrEmpty(_col.TextFormat))
                            _text = _col.TextFormat.Replace("{value}", data?.ToString());
                        else
                            _text = data?.ToString();

                        Label _label = new Label { Text = _text };

                        this.ApplyLabelStyle(_label, _col);

                        contentGrid.Children.Add(_label, _Cell.ColIndex, _Cell.RowIndex);

                        if (_col.RowSpan > 0)
                            Grid.SetRowSpan(_label, _col.RowSpan);

                        if (_col.ColumnSpan > 0)
                            Grid.SetColumnSpan(_label, _col.ColumnSpan);
                    }
                }
            }
            catch (Exception ex)
            {
                EbLog.Write(ex.Message);
            }
        }

        //for data grid
        private void FillTableColums(MobileTableRow row, List<EbMobileTableCell> CellCollection)
        {
            foreach (EbMobileTableCell _Cell in CellCollection)
            {
                if (_Cell.ControlCollection.Count > 0)
                {
                    EbMobileDataColumn _col = (EbMobileDataColumn)_Cell.ControlCollection[0];

                    string _text = string.Empty;
                    MobileTableColumn tableColumn = row[_col.ColumnName];

                    if (tableColumn != null)
                        _text = tableColumn.Value.ToString();

                    Label _label = new Label { Text = _text, VerticalTextAlignment = TextAlignment.Center, VerticalOptions = LayoutOptions.Center };

                    if (isHeader)
                        _label.FontFamily = (OnPlatform<string>)HelperFunctions.GetResourceValue("Roboto-Medium");
                    else
                        this.ApplyLabelStyle(_label, _col);

                    contentGrid.Children.Add(_label, _Cell.ColIndex, _Cell.RowIndex);
                }
            }
        }

        private void ApplyLabelStyle(Label Label, EbMobileDataColumn DataColumn)
        {
            EbFont _font = DataColumn.Font;

            if (_font != null)
            {
                Label.FontSize = (this.isHeader) ? (_font.Size + 4) : _font.Size;

                if (_font.Style == FontStyle.BOLD)
                    Label.FontAttributes = FontAttributes.Bold;
                else if (_font.Style == FontStyle.ITALIC)
                    Label.FontAttributes = FontAttributes.Italic;
                else
                    Label.FontAttributes = FontAttributes.None;

                Label.TextColor = (this.isHeader) ? Color.White : Color.FromHex(_font.Color.TrimStart('#'));

                if (_font.Caps)
                    Label.Text = Label.Text.ToUpper();

                if (_font.Underline)
                    Label.TextDecorations = TextDecorations.Underline;
                else if (_font.Strikethrough)
                    Label.TextDecorations = TextDecorations.Strikethrough;
            }
            else
            {
                if (this.isHeader) Label.TextColor = Color.White;
            }
        }

        public void SetBackGroundColor(int RowIndex)
        {
            if (RowIndex % 2 == 0)
                this.BackgroundColor = Color.Default;
            else
                this.BackgroundColor = Color.FromHex("F2F2F2");
        }

        public void ShowSyncFlag(ColumnColletion columns)
        {
            EbDataColumn col = columns.Find(item => item.ColumnName == "eb_synced");
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

            int colLength = contentGrid.ColumnDefinitions.Count;

            contentGrid.Children.Add(lbl, colLength - 1, 0);
        }

        public void ShowLinkIcon()
        {
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var lbl = new Label
            {
                FontSize = 18,
                VerticalOptions = LayoutOptions.Center,
                TextColor = Color.FromHex("aba8a8"),
                Text = "\uf105",
                FontFamily = (OnPlatform<string>)HelperFunctions.GetResourceValue("FontAwesome")
            };

            contentGrid.Children.Add(lbl);
            Grid.SetColumn(lbl, contentGrid.ColumnDefinitions.Count - 1);
            Grid.SetRowSpan(lbl, contentGrid.RowDefinitions.Count);
        }
    }
}
