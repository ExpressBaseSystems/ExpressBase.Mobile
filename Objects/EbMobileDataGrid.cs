using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbMobileDataGrid : EbMobileControl, ILinesEnabled
    {
        public List<EbMobileControl> ChildControls { set; get; }

        public EbMobileTableLayout DataLayout { set; get; }

        public string TableName { set; get; }

        private StackLayout RowContainer { set; get; }

        public View GridForm { set; get; }

        private readonly Style ButtonStyles = new Style(typeof(Button))
        {
            Setters =
            {
                new Setter{ Property = Button.VerticalOptionsProperty,Value = LayoutOptions.Center },
                new Setter{ Property = Button.PaddingProperty,Value = 0 },
                new Setter{ Property = Button.HeightRequestProperty,Value = 30 },
                new Setter{ Property = Button.WidthRequestProperty,Value = 30 },
                new Setter{ Property = Button.CornerRadiusProperty,Value = 4 },
                new Setter{ Property = Button.FontFamilyProperty,Value = (OnPlatform<string>)HelperFunctions.GetResourceValue("FontAwesome") },
            }
        };

        public EbMobileDataGrid()
        {
            RowContainer = new StackLayout();
        }

        private Grid GridLayout()
        {
            return new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition{ Width=GridLength.Star },
                    new ColumnDefinition{ Width=GridLength.Auto }
                }
            };
        }

        public override void InitXControl(FormMode Mode)
        {
            Frame frame = new Frame { Padding = 5, BackgroundColor = Color.FromHex("#eeeeee"), CornerRadius = 4 };
            Grid grid = GridLayout();

            Button addRowBtn = new Button
            {
                Text = "\uf055",
                Style = ButtonStyles,
                TextColor = Color.White,
                BackgroundColor = Color.FromHex("#33b762")
            };
            addRowBtn.Clicked += AddRowBtn_Clicked;
            grid.Children.Add(addRowBtn, 1, 0);

            var row = this.GetTableRow(true);
            grid.Children.Add(new CustomFrame(row, DataLayout.CellCollection, DataLayout.RowCount, DataLayout.ColumCount)
            {
                BackgroundColor = Color.Transparent,
                Padding = 0,
                Margin = 0
            }, 0, 0);
            frame.Content = grid;
            RowContainer.Children.Add(frame);
            this.XControl = RowContainer;
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
            CreateRow();
        }

        private void CreateRow()
        {
            Frame frame = new Frame { Padding = 5 };
            Grid grid = GridLayout();

            Button rowOptions = new Button
            {
                Text = "\uf142",
                Style = ButtonStyles,
                BackgroundColor = Color.Transparent
            };

            rowOptions.Clicked += RowOptions_Clicked;
            grid.Children.Add(rowOptions, 1, 0);

            grid.Children.Add(new CustomFrame(this.GetTableRow(true), DataLayout.CellCollection, DataLayout.RowCount, DataLayout.ColumCount)
            {
                BackgroundColor = Color.Transparent,
                Padding = 0,
                Margin = 0
            }, 0, 0);
            frame.Content = grid;
            RowContainer.Children.Add(frame);
        }

        private void RowOptions_Clicked(object sender, EventArgs e)
        {
            
        }
    }
}
