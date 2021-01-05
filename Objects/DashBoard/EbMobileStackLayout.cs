using ExpressBase.Mobile.Data;
using System.Collections.Generic;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbMobileStackLayout : EbMobileDashBoardControl
    {
        public StackOrientation Orientation { set; get; }

        public bool AllowScrolling { set; get; }

        public int Spacing { set; get; }

        public List<EbMobileDashBoardControl> ChildControls { set; get; }

        View container;

        public override View Draw()
        {
            if (ChildControls == null) return null;

            container = GetContainer();

            for (int i = 0; i < ChildControls.Count; i++)
            {
                View view = ChildControls[i].Draw();
                view.HorizontalOptions = LayoutOptions.FillAndExpand;

                if (view != null)
                {
                    AddContainerItems(view, i);
                }
            }

            return container;
        }

        public override void SetBindingValue(EbDataSet dataSet)
        {
            foreach (EbMobileDashBoardControl ctrl in ChildControls)
            {
                ctrl.SetBindingValue(dataSet);
            }
        }

        View GetContainer()
        {
            if (Orientation == StackOrientation.Vertical)
            {
                return new StackLayout
                {
                    Orientation = Xamarin.Forms.StackOrientation.Vertical,
                    Spacing = Spacing,
                    Padding = this.Padding == null ? 0 : this.Padding.ConvertToXValue(),
                    Margin = this.Margin == null ? 0 : this.Margin.ConvertToXValue(),
                };
            }
            else
            {
                Grid grid = new Grid
                {
                    ColumnSpacing = Spacing,
                    Padding = this.Padding == null ? 0 : this.Padding.ConvertToXValue(),
                    Margin = this.Margin == null ? 0 : this.Margin.ConvertToXValue(),
                };

                for (int i = 0; i < ChildControls.Count; i++)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition());
                }

                return grid;
            }
        }

        void AddContainerItems(View view, int index)
        {
            if (container is StackLayout sl)
            {
                sl.Children.Add(view);
            }
            else if (container is Grid grid)
            {
                grid.Children.Add(view);
                Grid.SetRow(view, 0);
                Grid.SetColumn(view, index);
            }
        }
    }
}
