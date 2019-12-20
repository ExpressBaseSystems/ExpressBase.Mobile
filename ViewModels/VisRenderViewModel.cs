using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.ViewModels;
using ExpressBase.Mobile.Views;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ExpressBase.Mobile.DynamicRenders
{
    public class VisRenderViewModel : BaseViewModel
    {
        public EbMobileVisualization Visualization { set; get; }

        public EbDataTable DataTable { set; get; }

        private ScrollView _dyView { set; get; }

        public ScrollView View
        {
            get
            {
                return _dyView;
            }
            set
            {
                _dyView = value;
            }
        }

        public VisRenderViewModel(EbMobilePage Page)
        {
            PageTitle = Page.DisplayName;
            this.Visualization = (Page.Container as EbMobileVisualization);
            this.GetData();
            this.CreateView();
        }

        private void GetData()
        {
            byte[] b = Convert.FromBase64String(this.Visualization.OfflineQuery.Code);
            string sql = HelperFunctions.WrapSelectQuery(System.Text.Encoding.UTF8.GetString(b));
            try
            {
                DataTable = App.DataDB.DoQuery(sql);
            }
            catch (Exception e)
            {
                DataTable = new EbDataTable();
                Console.WriteLine(e.Message);
            }
        }

        private void CreateView()
        {
            StackLayout StackL = new StackLayout();

            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += VisNodeCommand;

            foreach (EbDataRow _row in this.DataTable.Rows)
            {
                CustomFrame _Frame = new CustomFrame(_row);

                _Frame.GestureRecognizers.Add(tapGestureRecognizer);

                Grid _G = this.CreateGrid(this.Visualization.DataLayout.CellCollection);

                foreach (EbMobileTableCell _Cell in this.Visualization.DataLayout.CellCollection)
                {
                    if (_Cell.ControlCollection.Count > 0)
                    {
                        EbMobileDataColumn _col = _Cell.ControlCollection[0] as EbMobileDataColumn;

                        _G.Children.Add(new Label { Text = _row[_col.ColumnName].ToString() }, _Cell.ColIndex, _Cell.RowIndex);
                    }
                }

                _Frame.Content = _G;
                StackL.Children.Add(_Frame);
            }

            this.View = new ScrollView { Content = StackL };
        }

        Grid CreateGrid(List<EbMobileTableCell> CellCollection)
        {
            Grid G = new Grid();

            for (int r = 0; r < this.Visualization.DataLayout.RowCount; r++)
            {
                G.RowDefinitions.Add(new RowDefinition());
            }

            for (int c = 0; c < this.Visualization.DataLayout.ColumCount; c++)
            {
                EbMobileTableCell current = CellCollection.Find(li => li.ColIndex == c && li.RowIndex == 0);

                G.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(current.Width, GridUnitType.Star) });
            }

            return G;
        }

        void VisNodeCommand(object Frame, EventArgs args)
        {
            if(this.Visualization.RenderAs != MobVisRenderType.Info)
            {
                FormRender Renderer = new FormRender(this.Visualization, (Frame as CustomFrame).DataRow,this.DataTable.Columns);
                (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(Renderer);
            }
            else
            {

            }
        }
    }
}
