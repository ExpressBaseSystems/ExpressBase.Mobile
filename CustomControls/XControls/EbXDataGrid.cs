using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls
{
    public class EbXDataGrid : Grid
    {
        public static readonly BindableProperty DataSourceProperty =
            BindableProperty.Create(nameof(DataSource), typeof(EbDataTable), typeof(EbXDataGrid), propertyChanged: OnDataSourcePropertyChanged);

        public EbDataTable DataSource
        {
            get { return (EbDataTable)GetValue(DataSourceProperty); }
            set { SetValue(DataSourceProperty, value); }
        }

        public EbXDataGrid()
        {
            this.BackgroundColor = Color.FromHex("cccccc");
            this.RowSpacing = 1;
            this.ColumnSpacing = 1;
        }

        private void DrawColumns(ColumnColletion columns)
        {
            for (int i = 0; i < columns.Count; i++)
            {
                this.ColumnDefinitions.Add(new ColumnDefinition());

                Label label = new Label
                {
                    FontAttributes = FontAttributes.Bold,
                    FontFamily = (OnPlatform<string>)HelperFunctions.GetResourceValue("Roboto-Medium"),
                    Text = columns[i].ColumnName,
                    VerticalTextAlignment = TextAlignment.Center,
                    BackgroundColor = Color.FromHex("eeeeee"),
                    Padding = new Thickness(5, 0)
                };

                this.Children.Add(label, i, 0);
            }
        }

        private static void OnDataSourcePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            EbXDataGrid instance = bindable as EbXDataGrid;

            if (instance.DataSource != null)
            {
                instance.RowDefinitions.Add(new RowDefinition { Height = 40 });

                instance.DrawColumns(instance.DataSource.Columns);

                instance.DrawRows(instance.DataSource.Rows);
            }
        }

        private void DrawRows(RowColletion rows)
        {
            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                this.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                for (int columnIndex = 0; columnIndex < rows[rowIndex].Count; columnIndex++)
                {
                    Label label = new Label
                    {
                        Padding = 5,
                        BackgroundColor = Color.White,
                        Text = rows[rowIndex][columnIndex]?.ToString(),
                        VerticalOptions = LayoutOptions.FillAndExpand,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        LineBreakMode = LineBreakMode.WordWrap,
                        FontSize = 13
                    };
                    this.Children.Add(label, columnIndex, rowIndex + 1);
                }
            }
        }
    }
}
