using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbMobileDataLink : EbMobileDashBoardControl
    {
        public int RowCount { set; get; }

        public int ColumCount { set; get; }

        public int RowSpacing { set; get; }

        public int ColumnSpacing { set; get; }

        public string LinkRefId { get; set; }

        public List<EbMobileDataCell> CellCollection { set; get; }

        readonly List<EbMobileDashBoardControl> controls = new List<EbMobileDashBoardControl>();

        public override View Draw()
        {
            if (CellCollection == null) return null;

            EbXFrame frame = GetFrame();

            if (!string.IsNullOrEmpty(LinkRefId))
            {
                TapGestureRecognizer recognizer = new TapGestureRecognizer();
                recognizer.Tapped += OnClick;
                frame.GestureRecognizers.Add(recognizer);
            }

            DLDynamicGrid grid = new DLDynamicGrid(this);
            frame.Content = grid;

            foreach (EbMobileDataCell cell in CellCollection)
            {
                if (cell.ControlCollection == null || cell.ControlCollection.Count <= 0)
                    continue;

                foreach (EbMobileDataLabel control in cell.ControlCollection)
                {
                    if (control is IGridAlignment gridAlign)
                    {
                        controls.Add(control);
                        var view = control.Draw();

                        grid.SetPosition(view, cell.RowIndex, cell.ColIndex, gridAlign.RowSpan, gridAlign.ColumnSpan);
                    }
                }
            }
            return frame;
        }

        private bool isTapped;

        private async void OnClick(object sender, EventArgs e)
        {
            if (isTapped) return;

            try
            {
                isTapped = true;

                EbMobilePage page = EbPageHelper.GetPage(LinkRefId);

                if (page != null)
                {
                    EbPageRenderer renderer = await EbPageHelper.GetRenderer(page);

                    if (renderer.IsReady)
                    {
                        EbLog.Info(renderer.Message);
                        await App.Navigation.NavigateMasterAsync(renderer.Renderer);
                    }
                    else
                    {
                        EbLog.Error("unable to create renderer, [DataLink] " + renderer.Message);
                    }
                }
                else
                {
                    EbLog.Warning("page not found in [DataLink], Check permission");
                }
            }
            catch (Exception ex)
            {
                EbLog.Info("[EbMobileDashboard] click navigation error");
                EbLog.Error(ex.Message);
            }

            isTapped = false;
        }

        public override void SetBindingValue(EbDataSet dataSet)
        {
            foreach (var ctrl in controls)
            {
                ctrl.SetBindingValue(dataSet);
            }
        }
    }

    public class EbMobileDataCell : EbMobilePage
    {
        public int RowIndex { set; get; }

        public int ColIndex { set; get; }

        public int Width { set; get; }

        public List<EbMobileDashBoardControl> ControlCollection { set; get; }
    }
}
