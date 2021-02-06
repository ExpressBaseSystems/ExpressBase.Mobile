using ExpressBase.Mobile.CustomControls.XControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Helpers.Script;
using ExpressBase.Mobile.Views.Base;
using System;
using System.Collections.Generic;
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

            this.Initialize(viz);
        }

        protected void Initialize(EbMobileVisualization viz)
        {
            DynamicGrid = new DynamicGrid(viz.DataLayout);
            DynamicGrid.SetSpacing(viz.RowSpacing, viz.ColumnSpacing);

            this.SetFrameStyle(viz);

            this.FillData(viz.DataLayout.CellCollection);

            if (viz.ShowLinkIcon && !this.IsHeader)
            {
                DynamicGrid.ShowLinkIcon();
            }
            this.Content = DynamicGrid;
        }

        protected void SetFrameStyle(EbMobileVisualization viz)
        {
            if (!IsHeader)
            {
                EbThickness pd = viz.Padding ?? new EbThickness(10);
                EbThickness mr = viz.Margin ?? new EbThickness();

                this.Padding = new Thickness(pd.Left, pd.Top, pd.Right, pd.Bottom);
                this.Margin = new Thickness(mr.Left, mr.Top, mr.Right, mr.Bottom);

                DynamicGrid.XAllocated = App.ScreenX - (pd.Left + pd.Right + mr.Left + mr.Right);
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
                        View view = GetViewByRenderType(ctrl);

                        if (view != null && ctrl is IGridAlignment gridAlign)
                        {
                            view.SetHorrizontalAlign(gridAlign.HorrizontalAlign);
                            view.SetVerticalAlign(gridAlign.VerticalAlign);

                            DynamicGrid.SetPosition(view, cell.RowIndex, cell.ColIndex, gridAlign.RowSpan, gridAlign.ColumnSpan);
                            EvaluateExpression(ctrl, view);
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

        private void EvaluateExpression(EbMobileControl ctrl, View view)
        {
            if (ctrl.HasExpression(ExprType.HideExpr))
            {
                string script = ctrl.HiddenExpr.GetCode();
                EbListHelper.SetDataRow(DataRow);
                view.IsVisible = !EbListHelper.EvaluateHideExpr(script);
            }
            else if (ctrl.HasExpression(ExprType.ValueExpr))
            {
                string script = ctrl.ValueExpr.GetCode();
                EbListHelper.SetDataRow(DataRow);
                EbListHelper.EvaluateValueExpr(view, script);
            }
        }

        protected virtual View GetViewByRenderType(EbMobileControl ctrl)
        {
            View view = null;

            if (ctrl is EbMobileButton button)
            {
                if (button.HideInContext && IsHeader) return null;

                Button btn = (Button)button.Draw(DataRow);
                btn.Clicked += async (sender, args) => await ButtonControlClick(button);
                view = btn;
            }
            else if (ctrl is EbMobileDataColumn dc)
            {
                if (dc.HideInContext && IsHeader) return null;

                object data = this.DataRow[dc.ColumnName];
                view = ResolveRenderType(dc, data);
            }
            else if (ctrl is EbMobileLabel label)
            {
                EbXLabel lbl = (EbXLabel)label.Draw();
                lbl.SetFont(label.Font, this.IsHeader);
                lbl.SetTextWrap(label.TextWrap);
                view = lbl;
            }
            return view;
        }

        private View ResolveRenderType(EbMobileDataColumn dc, object value)
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
                    view = this.DC2Map(dc, value);
                    break;
                case DataColumnRenderType.Email:
                    view = this.DC2Email(dc, value);
                    break;
                case DataColumnRenderType.Audio:
                    view = this.DC2Audio(dc, value);
                    break;
                default:
                    EbXLabel label = new EbXLabel(dc)
                    {
                        Text = dc.GetContent(value)
                    };
                    label.SetFont(dc.Font, this.IsHeader);
                    label.SetTextWrap(dc.TextWrap);
                    view = label;
                    break;
            }
            return view;
        }

        private View DC2Image(EbMobileDataColumn dc, object value)
        {
            EbListViewImage image = new EbListViewImage(dc)
            {
                Style = (Style)HelperFunctions.GetResourceValue("ListViewImage")
            };
            image.SetValue(value);

            image.Clicked += (sender, args) =>
            {
                if (App.Navigation.GetCurrentPage() is IListRenderer rendrer)
                {
                    rendrer.ShowFullScreenImage(image.Source);
                }
            };
            return image;
        }

        private View DC2PhoneNumber(EbMobileDataColumn dc, object value)
        {
            EbXLabel label = new EbXLabel(dc)
            {
                Text = dc.GetContent(value)
            };

            label.SetFont(dc.Font, this.IsHeader);
            label.SetTextWrap(dc.TextWrap);

            TapGestureRecognizer gesture = new TapGestureRecognizer();
            gesture.Tapped += (sender, args) => NativeLauncher.OpenDialerAsync(label.Text);
            label.GestureRecognizers.Add(gesture);

            return label;
        }

        private View DC2Map(EbMobileDataColumn dc, object value)
        {
            EbListViewButton mapbtn = new EbListViewButton(dc);

            mapbtn.Clicked += async (sender, args) =>
            {
                if (value == null)
                {
                    Utils.Toast("location info empty");
                    return;
                }
                await NativeLauncher.OpenMapAsync(value?.ToString());
            };
            return mapbtn;
        }

        private View DC2Email(EbMobileDataColumn dc, object value)
        {
            EbXLabel label = new EbXLabel(dc)
            {
                Text = dc.GetContent(value)
            };
            label.SetFont(dc.Font, this.IsHeader);
            label.SetTextWrap(dc.TextWrap);

            TapGestureRecognizer gesture = new TapGestureRecognizer();
            gesture.Tapped += async (sender, args) => await NativeLauncher.OpenEmailAsync(value?.ToString());
            label.GestureRecognizers.Add(gesture);

            return label;
        }

        public async Task ButtonControlClick(EbMobileButton button)
        {
            await button.OnControlAction(this.DataRow);
        }

        private View DC2Audio(EbMobileDataColumn dc, object value)
        {
            EbPlayButton audioButton = new EbPlayButton(dc)
            {
                Style = (Style)HelperFunctions.GetResourceValue("ListViewAudioButton"),
                IsEnabled = !(value == null)
            };

            audioButton.SetDimensions(dc);

            if (value != null)
            {
                audioButton.SetValue(value);

                audioButton.Clicked += (sender, e) =>
                {
                    Page current = App.Navigation.GetCurrentPage();
                    if (current is IListRenderer ls)
                    {
                        ls.ShowAudioFiles((EbPlayButton)sender);
                    }
                };
            }

            return audioButton;
        }
    }
}
