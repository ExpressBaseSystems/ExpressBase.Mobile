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
        private Color OddColor = Color.FromHex("F2F2F2");

        private Color EvenColor = Color.Default;

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
            StackLayout StackL = new StackLayout { Spacing = 0 };

            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += VisNodeCommand;
            int _rowColCount = 1;

            foreach (EbDataRow _row in this.DataTable.Rows)
            {
                Color _color = ((_rowColCount % 2) == 0) ? EvenColor : OddColor;
                CustomFrame _Frame = new CustomFrame(_row, _color);

                _Frame.GestureRecognizers.Add(tapGestureRecognizer);

                Grid _G = this.CreateGrid(this.Visualization.DataLayout.CellCollection);

                foreach (EbMobileTableCell _Cell in this.Visualization.DataLayout.CellCollection)
                {
                    if (_Cell.ControlCollection.Count > 0)
                    {
                        EbMobileDataColumn _col = _Cell.ControlCollection[0] as EbMobileDataColumn;

                        Label _label = new Label { Text = _row[_col.ColumnName].ToString() };
                        this.ApplyLabelStyle(_label, _col);

                        _G.Children.Add(_label, _Cell.ColIndex, _Cell.RowIndex);
                    }
                }

                _Frame.SetContent(_G);
                StackL.Children.Add(_Frame);
                _rowColCount++;
            }

            this.View = new ScrollView { Content = StackL };
        }

        Grid CreateGrid(List<EbMobileTableCell> CellCollection)
        {
            Grid G = new Grid { BackgroundColor = Color.Transparent };

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
            if (!string.IsNullOrEmpty(this.Visualization.LinkRefId))
            {
                EbMobilePage _page = HelperFunctions.GetPage(Visualization.LinkRefId);

                if (_page.Container is EbMobileForm)
                {
                    FormRender Renderer = new FormRender(_page, (Frame as CustomFrame).DataRow, this.DataTable.Columns);
                    (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(Renderer);
                }
                else if (_page.Container is EbMobileVisualization)
                {
                    VisRender Renderer = new VisRender(_page, true);
                    (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(Renderer);
                }
            }
        }

        private void ApplyLabelStyle(Label Label, EbMobileDataColumn DataColumn)
        {
            EbFont _font = DataColumn.Font;
            if (_font != null)
            {
                Label.FontSize = _font.Size;
                
                if(_font.Style == FontStyle.BOLD)
                {
                    Label.FontAttributes = FontAttributes.Bold;
                    Label.FontFamily = "Montserrat-SemiBold";
                }
                else if (_font.Style == FontStyle.ITALIC)
                    Label.FontAttributes = FontAttributes.Italic;
                else 
                    Label.FontAttributes = FontAttributes.None;

                Label.TextColor = Color.FromHex(_font.color.TrimStart('#'));

                if (_font.Caps)
                    Label.Text = Label.Text.ToUpper();

                if (_font.Underline)
                    Label.TextDecorations = TextDecorations.Underline;
                else if (_font.Strikethrough)
                    Label.TextDecorations = TextDecorations.Strikethrough;
            }
        }
    }
}
