using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls
{
    public class DynamicFrame : Frame
    {
        public EbDataRow DataRow { set; get; }

        protected bool IsHeader { set; get; }

        protected Grid ContentGrid { set; get; }

        private readonly EbMobileVisualization context;

        public DynamicFrame() { }

        public DynamicFrame(EbDataRow row, EbMobileVisualization visualization, bool isHeader = false)
        {
            this.IsHeader = isHeader;
            this.DataRow = row;
            this.context = visualization;

            this.SetFrameStyle(visualization);

            this.CreateGrid(visualization.DataLayout.CellCollection, visualization.DataLayout.RowCount, visualization.DataLayout.ColumCount);
            this.FillData(visualization.DataLayout.CellCollection);

            if (visualization.ShowLinkIcon && !isHeader)
            {
                this.ShowLinkIcon();
            }
            this.Content = new StackLayout { Children = { ContentGrid } };
        }

        private void SetFrameStyle(EbMobileVisualization viz)
        {
            if (!IsHeader)
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
                EbLog.Error("Frame style issue :: " + ex.Message);
            }
        }

        public void CreateGrid(List<EbMobileTableCell> CellCollection, int RowCount, int ColumCount)
        {
            ContentGrid = new Grid { BackgroundColor = Color.Transparent };

            for (int r = 0; r < RowCount; r++)
            {
                ContentGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }
            for (int c = 0; c < ColumCount; c++)
            {
                EbMobileTableCell current = CellCollection.Find(li => li.ColIndex == c && li.RowIndex == 0);

                ContentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(current.Width, GridUnitType.Star) });
            }
        }

        private void FillData(List<EbMobileTableCell> CellCollection)
        {
            foreach (EbMobileTableCell cell in CellCollection)
            {
                if (cell.IsEmpty())
                    continue;

                foreach (var ctrl in cell.ControlCollection)
                {
                    this.ResolveControlType(ctrl, cell);
                }
            }
        }

        private void ResolveControlType(EbMobileControl ctrl, EbMobileTableCell cell)
        {
            View view = null;
            int rowSpan = 0;
            int colSpan = 0;
            bool hideInContext = false;

            if (ctrl is EbMobileButton button)
            {
                var btn = button.CreateView();
                btn.Clicked += async (sender, args) => await ButtonControlClick(button.LinkRefId);

                SetHorrizontalAlign(button.HorrizontalAlign, btn);
                SetVerticalAlign(button.VerticalAlign, btn);

                rowSpan = button.RowSpan;
                colSpan = button.ColumnSpan;
                hideInContext = button.HideInContext;

                view = btn;
            }
            else if (ctrl is EbMobileDataColumn dc)
            {
                rowSpan = dc.RowSpan;
                colSpan = dc.ColumnSpan;
                hideInContext = dc.HideInContext;

                object data = this.DataRow[dc.ColumnName];
                view = ResolveContentType(dc, data);
            }

            if (view != null)
            {
                view.IsVisible = !IsHeader || !hideInContext;
                SetGrid(view, cell.RowIndex, cell.ColIndex, rowSpan, colSpan);
            }
        }

        private void Btn_Clicked(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void SetGrid(View view, int row, int col, int rowspan, int colspan)
        {
            ContentGrid.Children.Add(view, col, row);

            if (rowspan > 0) Grid.SetRowSpan(view, rowspan);

            if (colspan > 0) Grid.SetColumnSpan(view, colspan);
        }

        private View ResolveContentType(EbMobileDataColumn dc, object value)
        {
            View view;

            switch (dc.RenderAs)
            {
                case DataColumnRenderType.Image:
                    view = this.DC2Image(value);
                    break;
                case DataColumnRenderType.MobileNumber:
                    view = this.DC2PhoneNumber(dc, value);
                    break;
                case DataColumnRenderType.Map:
                    view = this.DC2Map(value);
                    break;
                case DataColumnRenderType.Email:
                    view = this.DC2Email(dc, value);
                    break;
                default:
                    Label label = new Label { Text = dc.GetContent(value) };
                    this.ApplyLabelStyle(label, dc);
                    view = label;
                    break;
            }
            return view;
        }

        protected void ApplyLabelStyle(Label label, EbMobileDataColumn dc)
        {
            EbFont font = dc.Font;

            SetHorrizontalAlign(dc.HorrizontalAlign, label);
            SetVerticalAlign(dc.VerticalAlign, label);

            if (font != null)
            {
                label.FontSize = font.Size;
                label.TextColor = Color.FromHex(font.Color);

                SetFontStyle(font.Style, label);

                if (font.Caps)
                    label.Text = label.Text.ToUpper();

                SetFontDecoration(font, label);
            }

            if (this.IsHeader)
                label.TextColor = Color.White;
        }

        private void SetFontStyle(FontStyle style, Label label)
        {
            switch (style)
            {
                case FontStyle.BOLD:
                    label.FontAttributes = FontAttributes.Bold;
                    break;
                case FontStyle.ITALIC:
                    label.FontAttributes = FontAttributes.Italic;
                    break;
                case FontStyle.BOLDITALIC:
                    label.FontAttributes = FontAttributes.Italic;
                    break;
                default:
                    label.FontAttributes = FontAttributes.None;
                    break;
            }
        }

        private void SetFontDecoration(EbFont font, Label label)
        {
            if (font.Underline)
            {
                label.TextDecorations = TextDecorations.Underline;
            }
            else if (font.Strikethrough)
            {
                label.TextDecorations = TextDecorations.Strikethrough;
            }
            else
            {
                label.TextDecorations = TextDecorations.None;
            }
        }

        private void SetHorrizontalAlign(MobileHorrizontalAlign align, View label)
        {
            switch (align)
            {
                case MobileHorrizontalAlign.Center:
                    label.HorizontalOptions = LayoutOptions.Center;
                    break;
                case MobileHorrizontalAlign.Right:
                    label.HorizontalOptions = LayoutOptions.End;
                    break;
                case MobileHorrizontalAlign.Left:
                    label.HorizontalOptions = LayoutOptions.Start;
                    break;
                case MobileHorrizontalAlign.Fill:
                    label.HorizontalOptions = LayoutOptions.FillAndExpand;
                    break;
                default:
                    break;
            }
        }

        private void SetVerticalAlign(MobileVerticalAlign align, View label)
        {
            switch (align)
            {
                case MobileVerticalAlign.Center:
                    label.VerticalOptions = LayoutOptions.Center;
                    break;
                case MobileVerticalAlign.Bottom:
                    label.VerticalOptions = LayoutOptions.End;
                    break;
                case MobileVerticalAlign.Top:
                    label.VerticalOptions = LayoutOptions.Start;
                    break;
                case MobileVerticalAlign.Fill:
                    label.VerticalOptions = LayoutOptions.FillAndExpand;
                    break;
                default:
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

            int colLength = ContentGrid.ColumnDefinitions.Count;

            ContentGrid.Children.Add(lbl, colLength - 1, 0);
        }

        private void ShowLinkIcon()
        {
            Label lbl = new Label
            {
                Style = (Style)HelperFunctions.GetResourceValue("ListViewLinkIconStyle")
            };

            ContentGrid.Children.Add(lbl);
            Grid.SetRowSpan(lbl, ContentGrid.RowDefinitions.Count);
            Grid.SetColumnSpan(lbl, ContentGrid.ColumnDefinitions.Count);
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
                EbLog.Error("failed to load image ,getfile api error");
            }
        }

        private void CacheImage(string filename, byte[] fileBytea)
        {
            HelperFunctions.WriteFilesLocal(filename + ".jpg", fileBytea);
        }

        private View DC2PhoneNumber(EbMobileDataColumn dc, object value)
        {
            Label label = new Label
            {
                Text = dc.GetContent(value),
                TextColor = Color.FromHex("315eff")
            };

            ApplyLabelStyle(label, dc);

            var gesture = new TapGestureRecognizer();
            gesture.Tapped += (sender, args) => NativeLauncher.OpenDialerAsync(label.Text);
            label.GestureRecognizers.Add(gesture);

            return label;
        }

        private View DC2Map(object value)
        {
            Button mapbtn = new Button
            {
                Style = (Style)HelperFunctions.GetResourceValue("ListViewGoogleMap")
            };
            mapbtn.Clicked += async (sender, args) => await NativeLauncher.OpenMapAsync(value?.ToString());
            return mapbtn;
        }

        private View DC2Email(EbMobileDataColumn dc, object value)
        {
            Label label = new Label
            {
                Text = dc.GetContent(value),
                TextColor = Color.FromHex("315eff")
            };
            ApplyLabelStyle(label, dc);

            var gesture = new TapGestureRecognizer();
            gesture.Tapped += async (sender, args) => await NativeLauncher.OpenEmailAsync(value?.ToString());
            label.GestureRecognizers.Add(gesture);

            return label;
        }

        public async Task ButtonControlClick(string linkRefId)
        {
            if (string.IsNullOrEmpty(linkRefId))
                return;

            EbMobilePage page = HelperFunctions.GetPage(linkRefId);
            if (page != null)
            {
                await NavigationService.GetButtonLinkPage(context, this.DataRow, page);
            }
        }
    }
}
