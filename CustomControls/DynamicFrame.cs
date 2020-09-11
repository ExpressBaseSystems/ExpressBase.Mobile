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

        protected DynamicGrid DynamicGrid { set; get; }

        public DynamicFrame() { }

        public DynamicFrame(EbDataRow row, EbMobileVisualization viz, bool isHeader = false)
        {
            this.IsHeader = isHeader;
            this.DataRow = row;

            this.SetFrameStyle(viz);

            DynamicGrid = new DynamicGrid(viz.DataLayout);
            DynamicGrid.SetSpacing(viz.RowSpacing, viz.ColumnSpacing);

            this.FillData(viz.DataLayout.CellCollection);

            if (viz.ShowLinkIcon && !isHeader)
            {
                DynamicGrid.ShowLinkIcon();
            }
            this.Content = DynamicGrid;
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
                this.BackgroundColor = Color.FromHex(viz.BackgroundColor ?? "#ffffff");
                this.CornerRadius = viz.BorderRadius;
                if (!IsHeader)
                {
                    this.BorderColor = Color.FromHex(viz.BorderColor ?? "#ffffff");
                    this.HasShadow = viz.BoxShadow;
                }
            }
            catch (Exception ex)
            {
                EbLog.Info("Frame style issue");
                EbLog.Error(ex.Message);
            }
        }

        private void FillData(List<EbMobileTableCell> CellCollection)
        {
            foreach (EbMobileTableCell cell in CellCollection)
            {
                if (cell.IsEmpty())
                    continue;

                foreach (EbMobileControl ctrl in cell.ControlCollection)
                {
                    try
                    {
                        View view = this.ResolveControlType(ctrl);

                        if (view != null)
                        {
                            IMobileAlignment algn = (IMobileAlignment)ctrl;

                            SetHorrizontalAlign(algn.HorrizontalAlign, view);
                            SetVerticalAlign(algn.VerticalAlign, view);

                            IGridSpan span = (IGridSpan)ctrl;

                            DynamicGrid.SetPosition(view, cell.RowIndex, cell.ColIndex, span.RowSpan, span.ColumnSpan);
                        }
                    }
                    catch (Exception ex)
                    {
                        EbLog.Info("Failed to resolve grid content type in dynamic frame");
                        EbLog.Error(ex.Message);
                    }
                }
            }
        }

        private View ResolveControlType(EbMobileControl ctrl)
        {
            View view = null;

            if (ctrl is EbMobileButton button)
            {
                if (button.HideInContext && IsHeader)
                    return null;

                var btn = button.CreateView();
                btn.Clicked += async (sender, args) => await ButtonControlClick(button);
                view = btn;
            }
            else if (ctrl is EbMobileDataColumn dc)
            {
                if (dc.HideInContext && IsHeader)
                    return null;

                object data = this.DataRow[dc.ColumnName];
                view = this.ResolveContentType(dc, data);
            }
            return view;
        }

        private View ResolveContentType(EbMobileDataColumn dc, object value)
        {
            View view;

            switch (dc.RenderAs)
            {
                case DataColumnRenderType.Image:
                    view = this.DC2Image(dc, value);
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

        private void SetHorrizontalAlign(MobileHorrizontalAlign align, View view)
        {
            switch (align)
            {
                case MobileHorrizontalAlign.Center:
                    view.HorizontalOptions = LayoutOptions.Center;
                    break;
                case MobileHorrizontalAlign.Right:
                    view.HorizontalOptions = LayoutOptions.End;
                    break;
                case MobileHorrizontalAlign.Left:
                    view.HorizontalOptions = LayoutOptions.Start;
                    break;
                case MobileHorrizontalAlign.Fill:
                    view.HorizontalOptions = LayoutOptions.FillAndExpand;
                    break;
                default:
                    break;
            }
        }

        private void SetVerticalAlign(MobileVerticalAlign align, View view)
        {
            switch (align)
            {
                case MobileVerticalAlign.Center:
                    view.VerticalOptions = LayoutOptions.Center;
                    break;
                case MobileVerticalAlign.Bottom:
                    view.VerticalOptions = LayoutOptions.End;
                    break;
                case MobileVerticalAlign.Top:
                    view.VerticalOptions = LayoutOptions.Start;
                    break;
                case MobileVerticalAlign.Fill:
                    view.VerticalOptions = LayoutOptions.FillAndExpand;
                    break;
                default:
                    break;
            }
        }

        private View DC2Image(EbMobileDataColumn dc, object value)
        {
            LSImage image = new LSImage
            {
                Style = (Style)HelperFunctions.GetResourceValue("ListViewImage")
            };

            if (dc.VerticalAlign == MobileVerticalAlign.Fill)
                image.CalcHeight = true;
            else
                image.HeightRequest = dc.Height;

            if (dc.HorrizontalAlign != MobileHorrizontalAlign.Fill)
                image.WidthRequest = dc.Width;

            this.RenderImage(image, value);
            return image;
        }

        private void Image_SizeChanged(object sender, EventArgs e)
        {
            LSImage item = (LSImage)sender;

            if (item.InitialWidth == 0)
                item.InitialWidth = item.Width;

            if (item.Width != item.InitialWidth)
                item.WidthRequest = item.InitialWidth;

            item.HeightRequest = item.InitialWidth;
        }

        public async void RenderImage(LSImage image, object filerefs)
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

        public async Task ButtonControlClick(EbMobileButton button)
        {
            if (string.IsNullOrEmpty(button.LinkRefId))
                return;

            EbMobilePage page = EbPageFinder.GetPage(button.LinkRefId);
            if (page != null)
            {
                await NavigationService.NavigateButtonLinkPage(button, this.DataRow, page);
            }
        }
    }
}
