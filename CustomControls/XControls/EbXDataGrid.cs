using ExpressBase.Mobile.Data;
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

        private void DrawColumns(ColumnColletion columns)
        {
            for (int i = 0; i < columns.Count; i++)
            {
                this.ColumnDefinitions.Add(new ColumnDefinition());

                Label label = new Label
                {
                    FontAttributes = FontAttributes.Bold,
                    HorizontalTextAlignment = TextAlignment.Center,
                    Text = columns[i].ColumnName
                };

                this.Children.Add(label, i, 0);
            }
        }

        private static void OnDataSourcePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            EbXDataGrid instance = bindable as EbXDataGrid;

            if (instance.DataSource != null)
            {
                instance.RowDefinitions.Add(new RowDefinition());

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
                        HorizontalTextAlignment = TextAlignment.Center,
                        Text = rows[rowIndex][columnIndex]?.ToString()
                    };
                    this.Children.Add(label, columnIndex, rowIndex + 1);
                }
            }
        }
    }
}
