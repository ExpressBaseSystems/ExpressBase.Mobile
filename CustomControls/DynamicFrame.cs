using ExpressBase.Mobile.Configuration;
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
    public class DynamicFrame : Frame
    {
        public EbDataRow DataRow { set; get; }

        private readonly bool isHeader;

        private Grid grid;

        public DynamicFrame() { }

        public DynamicFrame(EbDataRow row, EbMobileVisualization visualization, bool isHeader = false)
        {
            this.isHeader = isHeader;
            this.DataRow = row;

            this.SetFrameStyle(visualization);

            this.CreateGrid(visualization.DataLayout.CellCollection, visualization.DataLayout.RowCount, visualization.DataLayout.ColumCount);
            this.FillData(visualization.DataLayout.CellCollection);

            if (visualization.HasLink() && !isHeader)
            {
                this.ShowLinkIcon();
            }
            this.Content = new StackLayout { Children = { grid } };
        }

        //for data grid
        public DynamicFrame(MobileTableRow row, List<EbMobileTableCell> cellCollection, int rowCount, int columCount, bool isHeader = false)
        {
            this.isHeader = isHeader;
            this.CreateGrid(cellCollection, rowCount, columCount);
            this.FillTableColums(row, cellCollection);
            this.Content = grid;
        }

        private void SetFrameStyle(EbMobileVisualization viz)
        {
            if (!isHeader)
            {
                EbThickness pd = viz.Padding ?? new EbThickness(10);
                EbThickness mr = viz.Margin ?? new EbThickness();

                this.Padding = new Thickness(pd.Left, pd.Top, pd.Right, pd.Bottom);
                this.Margin = new Thickness(mr.Left, mr.Top, mr.Right, mr.Bottom);
            }

            try
            {
                if (viz.Style == RenderStyle.Tile)
                {
                    this.BackgroundColor = Color.FromHex(viz.BackgroundColor ?? "#ffffff");
                    this.BorderColor = Color.FromHex(viz.BorderColor ?? "#ffffff");
                    this.CornerRadius = viz.BorderRadius;
                    this.HasShadow = viz.BoxShadow;
                }
            }
            catch (Exception ex)
            {
                EbLog.Write("Frame style issue :: " + ex.Message);
            }
        }



        private void CreateGrid(List<EbMobileTableCell> CellCollection, int RowCount, int ColumCount)
        {
            grid = new Grid { BackgroundColor = Color.Transparent };

            for (int r = 0; r < RowCount; r++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }
            for (int c = 0; c < ColumCount; c++)
            {
                EbMobileTableCell current = CellCollection.Find(li => li.ColIndex == c && li.RowIndex == 0);

                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(current.Width, GridUnitType.Star) });
            }
        }

        private void FillData(List<EbMobileTableCell> CellCollection)
        {
            try
            {
                foreach (EbMobileTableCell cell in CellCollection)
                {
                    if (cell.ControlCollection.Count > 0)
                    {
                        EbMobileDataColumn datacol = (EbMobileDataColumn)cell.ControlCollection[0];

                        object data = this.DataRow[datacol.ColumnName];

                        View view = ResolveContentType(datacol, data);

                        if (view == null) continue;

                        grid.Children.Add(view, cell.ColIndex, cell.RowIndex);

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
            foreach (EbMobileTableCell cell in CellCollection)
            {
                if (cell.ControlCollection.Count > 0)
                {
                    EbMobileDataColumn column = (EbMobileDataColumn)cell.ControlCollection[0];

                    string text = string.Empty;
                    MobileTableColumn tableColumn = row[column.ColumnName];

                    if (tableColumn != null)
                        text = tableColumn.Value.ToString();

                    Label label = new Label { Text = text, VerticalTextAlignment = TextAlignment.Center, VerticalOptions = LayoutOptions.Center };

                    if (isHeader)
                        label.FontFamily = (OnPlatform<string>)HelperFunctions.GetResourceValue("Roboto-Medium");
                    else
                        this.ApplyLabelStyle(label, column);

                    grid.Children.Add(label, cell.ColIndex, cell.RowIndex);
                }
            }
        }

        private void ApplyLabelStyle(Label label, EbMobileDataColumn dc)
        {
            EbFont font = dc.Font;

            SetTextAlign(dc.TextAlign, label);

            if (font != null)
            {
                label.FontSize = font.Size;

                if (font.Style == FontStyle.BOLD)
                    label.FontAttributes = FontAttributes.Bold;
                else if (font.Style == FontStyle.ITALIC)
                    label.FontAttributes = FontAttributes.Italic;
                else
                    label.FontAttributes = FontAttributes.None;

                label.TextColor = (this.isHeader) ? Color.White : Color.FromHex(font.Color);

                if (font.Caps)
                    label.Text = label.Text.ToUpper();

                if (font.Underline)
                    label.TextDecorations = TextDecorations.Underline;
                else if (font.Strikethrough)
                    label.TextDecorations = TextDecorations.Strikethrough;
            }
            else
            {
                if (this.isHeader) label.TextColor = Color.White;
            }
        }

        private void SetTextAlign(MobileTextAlign align, Label label)
        {
            switch (align)
            {
                case MobileTextAlign.Center:
                    label.HorizontalTextAlignment = TextAlignment.Center;
                    break;
                case MobileTextAlign.Right:
                    label.HorizontalTextAlignment = TextAlignment.End;
                    break;
                default:
                    label.HorizontalTextAlignment = TextAlignment.Start;
                    break;
            }
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

            int colLength = grid.ColumnDefinitions.Count;

            grid.Children.Add(lbl, colLength - 1, 0);
        }

        private void ShowLinkIcon()
        {
            Label lbl = new Label
            {
                Style = (Style)HelperFunctions.GetResourceValue("ListViewLinkIconStyle")
            };

            grid.Children.Add(lbl);
            Grid.SetRowSpan(lbl, grid.RowDefinitions.Count);
            Grid.SetColumnSpan(lbl, grid.ColumnDefinitions.Count);
        }

        private View DC2Image(object value)
        {
            LSImageButton image = new LSImageButton
            {
                Style = (Style)HelperFunctions.GetResourceValue("ListViewImage")
            };
            image.SizeChanged += Image_SizeChanged;

            this.RenderImage(image, value);
            return image;
        }

        private void Image_SizeChanged(object sender, EventArgs e)
        {
            LSImageButton item = (LSImageButton)sender;

            if (item.InitialWidth == 0)
                item.InitialWidth = item.Width;

            if (item.Width != item.InitialWidth)
                item.WidthRequest = item.InitialWidth;

            item.HeightRequest = item.InitialWidth;
        }

        public async void RenderImage(LSImageButton image, object filerefs)
        {
            if (filerefs == null) return;

            string refid = filerefs.ToString().Split(CharConstants.COMMA)[0];
            try
            {
                byte[] file = await DataService.Instance.GetLocalFile($"{refid}.jpg");

                if (file == null)
                {
                    ApiFileResponse resp = await DataService.Instance.GetFile(EbFileCategory.Images, $"{refid}.jpg");
                    if (resp.HasContent)
                    {
                        image.Source = ImageSource.FromStream(() => { return new MemoryStream(resp.Bytea); });
                        this.CacheImage(refid, resp.Bytea);
                    }
                }
                else
                    image.Source = ImageSource.FromStream(() => { return new MemoryStream(file); });
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
            catch (ArgumentNullException) { toast.Show("no number"); }
            catch (FeatureNotSupportedException) { toast.Show("Feature unsuported"); }
            catch (Exception) { toast.Show("Something went wrong"); }
        }

        private void CacheImage(string filename, byte[] fileBytea)
        {
            HelperFunctions.WriteFilesLocal(filename + ".jpg", fileBytea);
        }
    }
}
