using ExpressBase.Mobile.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls.XControls
{
    public class XDataGrid : Grid
    {
        public static readonly BindableProperty DataSourceProperty =
            BindableProperty.Create(nameof(DataSource), typeof(EbDataTable), typeof(XDataGrid));

        public EbDataTable DataSource
        {
            get { return (EbDataTable)GetValue(DataSourceProperty); }
            set { SetValue(DataSourceProperty, value); }
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if (DataSource != null)
            {
                SetDeffenitions(DataSource.Rows.Count, DataSource.Columns.Count);
            }
        }

        private void SetDeffenitions(int rowCount, int columnCount)
        {
            for (int i = 0; i < columnCount; i++)
            {
                this.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int i = 0; i < rowCount; i++)
            {
                this.RowDefinitions.Add(new RowDefinition
                {
                    Height = i == 0 ? GridLength.Star : GridLength.Auto
                });
            }
        }
    }
}
