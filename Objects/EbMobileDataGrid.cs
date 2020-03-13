using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Views.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbMobileDataGrid : EbMobileControl, ILinesEnabled
    {
        public List<EbMobileControl> ChildControls { set; get; }

        public EbMobileTableLayout DataLayout { set; get; }

        public string TableName { set; get; }

        public FormMode Mode { set; get; }

        private Frame Container { set; get; }

        private StackLayout GridHeader { set; get; }

        private StackLayout GridBody { set; get; }

        private StackLayout GridFooter { set; get; }

        private TapGestureRecognizer TapRecognizer { set; get; }

        private readonly Style ButtonStyles = new Style(typeof(Button))
        {
            Setters =
            {
                new Setter{ Property = Button.VerticalOptionsProperty,Value = LayoutOptions.Center },
                new Setter{ Property = Button.PaddingProperty,Value = 0 },
                new Setter{ Property = Button.HeightRequestProperty,Value = 30 },
                new Setter{ Property = Button.WidthRequestProperty,Value = 30 },
                new Setter{ Property = Button.CornerRadiusProperty,Value = 4 },
                new Setter
                {
                    Property = Button.FontFamilyProperty,
                    Value = (OnPlatform<string>)HelperFunctions.GetResourceValue("FontAwesome")
                },
            }
        };

        public EbMobileDataGrid()
        {
            Container = new Frame
            {
                BorderColor = Color.FromHex("cccccc"),
                CornerRadius = 4,
                HasShadow = false,
                Padding = 1
            };

            GridHeader = new StackLayout { BackgroundColor = Color.FromHex("eeeeee") };
            GridBody = new StackLayout { IsVisible = false };
            GridFooter = new StackLayout { IsVisible = false };

            var stackL = new StackLayout
            {
                Spacing = 0,
                Children =
                {
                    GridHeader,
                    GridBody,
                    GridFooter
                }
            };
            Container.Content = stackL;
            TapRecognizer = new TapGestureRecognizer();
            TapRecognizer.Tapped += TapRecognizer_Tapped;
        }

        private Grid CreateGridLayout()
        {
            return new Grid
            {
                ClassId = Guid.NewGuid().ToString("N"),
                VerticalOptions = LayoutOptions.Center,
                Padding = new Thickness(5, 5),
                ColumnDefinitions =
                {
                    new ColumnDefinition{ Width=GridLength.Star },
                    new ColumnDefinition{ Width=GridLength.Auto }
                }
            };
        }

        public override void InitXControl(FormMode mode)
        {
            Mode = mode;
            CreateHeader();//creating grid header
            CreateFooter();//creating grid footer
            this.XControl = Container;
        }

        private void CreateHeader()
        {
            Grid grid = CreateGridLayout();//creating new grid
            Button addRowBtn = new Button
            {
                Text = "\uf055",
                Style = ButtonStyles,
                TextColor = Color.White,
                BackgroundColor = Color.FromHex("#33b762")
            };
            addRowBtn.Clicked += AddRowBtn_Clicked;
            grid.Children.Add(addRowBtn, 1, 0);
            var frame = new CustomFrame(this.GetTableRow(true), DataLayout.CellCollection, DataLayout.RowCount, DataLayout.ColumCount, true)
            {
                BackgroundColor = Color.Transparent,
                Padding = 0
            };
            grid.Children.Add(frame, 0, 0);
            GridHeader.Children.Add(grid);
        }

        private void CreateRow()
        {
            Grid grid = CreateGridLayout();
            Button rowOptions = new Button
            {
                ClassId = grid.ClassId,
                Text = "\uf014",
                Style = ButtonStyles,
                BackgroundColor = Color.Transparent
            };
            rowOptions.Clicked += RowDelete_Clicked;
            grid.Children.Add(rowOptions, 1, 0);
            var row = this.GetTableRow();
            CustomFrame frame = new CustomFrame(row, DataLayout.CellCollection, DataLayout.RowCount, DataLayout.ColumCount)
            {
                ClassId = grid.ClassId,
                BackgroundColor = Color.Transparent,
                Padding = 0
            };
            frame.GestureRecognizers.Add(this.TapRecognizer);
            grid.Children.Add(frame, 0, 0);
            GridBody.Children.Add(grid);
        }

        private void CreateFooter()
        {
            Grid grid = CreateGridLayout();
            grid.Children.Add(new CustomFrame(this.GetTableRow(true), DataLayout.CellCollection, DataLayout.RowCount, DataLayout.ColumCount)
            {
                BackgroundColor = Color.Transparent,
                Padding = 0
            }, 0, 0);
            GridFooter.Children.Add(grid);
        }

        private MobileTableRow GetTableRow(bool isHeader = false)
        {
            MobileTableRow row = new MobileTableRow();
            foreach (var ctrl in this.ChildControls)
            {
                if (ctrl is INonPersistControl || ctrl is ILinesEnabled)
                    continue;
                row.Columns.Add(new MobileTableColumn
                {
                    Name = ctrl.Name,
                    Type = ctrl.EbDbType,
                    Value = isHeader ? ctrl.Label : ctrl.GetValue()
                });
            }
            return row;
        }

        private void AddRowBtn_Clicked(object sender, EventArgs e)
        {
            var gridview = new DataGridView(this);
            (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushModalAsync(gridview);
        }

        private void RowDelete_Clicked(object sender, EventArgs e)
        {
            var button = sender as Button;

            foreach(var el in GridBody.Children)
            {
                if(el.ClassId == button.ClassId)
                {
                    GridBody.Children.Remove(el);
                    break;
                }
            }
        }

        public void RowAddCallBack()
        {
            if (GridBody.Children.Count >= 1)
                GridBody.Children.Add(new BoxView { HeightRequest = 1, Color = Color.FromHex("cccccc") });
            CreateRow();

            if (GridBody.Children.Count > 0)
                GridBody.IsVisible = true;
        }

        private void TapRecognizer_Tapped(object sender, EventArgs e)
        {

        }
    }
}
