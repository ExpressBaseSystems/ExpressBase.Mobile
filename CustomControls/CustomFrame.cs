using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls
{
    public class CustomFrame : Frame
    {
        private readonly bool isHeader;

        public EbDataRow DataRow { set; get; }

        private Grid contentGrid;

        public CustomFrame() { }

        public CustomFrame(EbDataRow row, EbMobileVisualization visualization, bool isHeader = false)
        {
            this.isHeader = isHeader;
            this.DataRow = row;

            this.CreateGrid(visualization.DataLayout.CellCollection, visualization.DataLayout.RowCount, visualization.DataLayout.ColumCount);
            this.FillData(visualization.DataLayout.CellCollection);

            if (visualization.HasLink() && !isHeader)
                this.ShowLinkIcon();

            this.Content = contentGrid;

            if (!isHeader)
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
            contentGrid = new Grid { BackgroundColor = Color.Transparent, VerticalOptions = LayoutOptions.StartAndExpand };

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
                        EbMobileDataColumn datacol = (EbMobileDataColumn)_Cell.ControlCollection[0];

                        object data = this.DataRow[datacol.ColumnName];

                        View view = ResolveContentType(datacol, data);

                        if (view == null) continue;

                        contentGrid.Children.Add(view, _Cell.ColIndex, _Cell.RowIndex);

                        if (datacol.RowSpan > 0)
                            Grid.SetRowSpan(view, datacol.RowSpan);

                        if (datacol.ColumnSpan > 0)
                            Grid.SetColumnSpan(view, datacol.ColumnSpan);
                    }
                }
            }
            catch (Exception ex)
            {
                EbLog.Write(ex.Message);
            }
        }

        private View ResolveContentType(EbMobileDataColumn dc, object value)
        {
            if (value == null) return null;

            if (dc.RenderAs == DataColumnRenderType.Image)
            {
                return this.DC2Image(value);
            }
            else if (dc.RenderAs == DataColumnRenderType.MobileNumber)
            {
                return this.DC2PhoneNumber(dc, value);
            }
            else
            {
                Label label = new Label { Text = dc.GetContent(value) };
                this.ApplyLabelStyle(label, dc);
                return label;
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

            Label lbl = new Label
            {
                Style = (Style)HelperFunctions.GetResourceValue("ListViewSyncFlagStyle"),
                TextColor = (synced == 0) ? Color.FromHex("ff5f5f") : Color.Green,
                Text = (synced == 0) ? "\uf06a" : "\uf058",
            };

            int colLength = contentGrid.ColumnDefinitions.Count;

            contentGrid.Children.Add(lbl, colLength - 1, 0);
        }

        public void ShowLinkIcon()
        {
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            Label lbl = new Label
            {
                Style = (Style)HelperFunctions.GetResourceValue("ListViewLinkIconStyle")
            };

            contentGrid.Children.Add(lbl);
            Grid.SetColumn(lbl, contentGrid.ColumnDefinitions.Count - 1);
            Grid.SetRowSpan(lbl, contentGrid.RowDefinitions.Count);
        }

        private View DC2Image(object value)
        {
            Image image = new Image { Style = (Style)HelperFunctions.GetResourceValue("ListViewImage") };
            Frame frame = new Frame
            {
                Content = image,
                Style = (Style)HelperFunctions.GetResourceValue("ListViewImageFrame")
            };
            frame.SizeChanged += Image_SizeChanged;
            this.RenderImage(image, value);
            return frame;
        }

        private void Image_SizeChanged(object sender, EventArgs e)
        {
            Frame item = (Frame)sender;
            item.HeightRequest = item.Width;
        }

        public async void RenderImage(Image image, object filerefs)
        {
            if (filerefs == null && string.IsNullOrEmpty(filerefs.ToString()))
                return;

            string refid = filerefs.ToString().Split(CharConstants.COMMA)[0];

            try
            {
                ApiFileResponse resp = await DataService.Instance.GetFile(EbFileCategory.Images, $"{refid}.jpg");
                if (resp.HasContent)
                    image.Source = ImageSource.FromStream(() => { return new MemoryStream(resp.Bytea); });
            }
            catch (Exception)
            {
                EbLog.Write("failed to load image ,getfile api error");
            }
        }

        private View DC2PhoneNumber(EbMobileDataColumn dc, object value)
        {
            Label label = new Label { Text = dc.GetContent(value) };
            this.ApplyLabelStyle(label, dc);
            Button callbtn = new Button
            {
                ClassId = label.Text,
                Style = (Style)HelperFunctions.GetResourceValue("ListViewPhoneNumber")
            };
            callbtn.Clicked += DialNumber;

            return new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children = { label, callbtn }
            };
        }

        private void DialNumber(object sender, EventArgs e)
        {
            Button dialler = (Button)sender;
            IToast toast = DependencyService.Get<IToast>();
            try
            {
                PhoneDialer.Open(dialler.ClassId);
            }
            catch (ArgumentNullException)
            {
                toast.Show("no number");
            }
            catch (FeatureNotSupportedException)
            {
                toast.Show("Feature unsuported");
            }
            catch (Exception)
            {
                toast.Show("Something went wrong");
            }
        }
    }
}
