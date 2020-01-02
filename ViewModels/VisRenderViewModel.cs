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

        public EbMobileVisualization SourceVisualization { set; get; }

        public EbMobileVisualization Visualization { set; get; }

        public EbDataTable DataTable { set; get; }

        private ScrollView _dyView { set; get; }

        private VisRenderType RenderType { set; get; }

        private CustomFrame HeaderFrame { set; get; }

        public Command AddCommand { set; get; }

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
            RenderType = VisRenderType.OBJ2LIST;
            PageTitle = Page.DisplayName;
            this.Visualization = (Page.Container as EbMobileVisualization);
            this.GetData();
            this.CreateView();
        }

        public VisRenderViewModel(EbMobilePage LinkPage, EbMobileVisualization SourceVis, CustomFrame CustFrame)
        {
            AddCommand = new Command(AddButtonClicked);
            RenderType = VisRenderType.LIST2LIST;
            SourceVisualization = SourceVis;
            this.HeaderFrame = CreateFrame(CustFrame.DataRow, CustFrame.Columns, SourceVis, Color.Transparent, true);

            PageTitle = LinkPage.DisplayName;
            this.Visualization = (LinkPage.Container as EbMobileVisualization);
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

            if (RenderType == VisRenderType.LIST2LIST)
            {
                this.AppendListHeader(StackL);
            }

            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += VisNodeCommand;
            int _rowColCount = 1;

            foreach (EbDataRow _row in this.DataTable.Rows)
            {
                Color _color = ((_rowColCount % 2) == 0) ? EvenColor : OddColor;

                CustomFrame CustFrame = CreateFrame(_row, this.DataTable.Columns, this.Visualization, _color);
                CustFrame.GestureRecognizers.Add(tapGestureRecognizer);
                StackL.Children.Add(CustFrame);
                _rowColCount++;
            }

            this.View = new ScrollView { Content = StackL };
        }

        private CustomFrame CreateFrame(EbDataRow row, ColumnColletion Columns, EbMobileVisualization Vis, Color color, bool IsHeader = false)
        {
            CustomFrame _frame = new CustomFrame(row, Columns, color);

            Grid _grid = this.CreateGrid(Vis.DataLayout.CellCollection, Vis.DataLayout.RowCount, Vis.DataLayout.ColumCount);

            foreach (EbMobileTableCell _Cell in Vis.DataLayout.CellCollection)
            {
                if (_Cell.ControlCollection.Count > 0)
                {
                    EbMobileDataColumn _col = _Cell.ControlCollection[0] as EbMobileDataColumn;

                    string _text = string.Empty;
                    if (!string.IsNullOrEmpty(_col.TextFormat))
                    {
                        _text = _col.TextFormat.Replace("{value}", row[_col.ColumnName].ToString());
                    }
                    else
                    {
                        _text = row[_col.ColumnName].ToString();
                    }

                    Label _label = new Label { Text = _text };
                    this.ApplyLabelStyle(_label, _col, IsHeader);

                    _grid.Children.Add(_label, _Cell.ColIndex, _Cell.RowIndex);
                }
            }
            _frame.SetContent(_grid);
            return _frame;
        }

        Grid CreateGrid(List<EbMobileTableCell> CellCollection, int RowCount, int ColumCount)
        {
            Grid G = new Grid { BackgroundColor = Color.Transparent };

            for (int r = 0; r < RowCount; r++)
            {
                G.RowDefinitions.Add(new RowDefinition());
            }

            for (int c = 0; c < ColumCount; c++)
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
                    VisRender Renderer = new VisRender(_page, this.Visualization, (Frame as CustomFrame));
                    (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(Renderer);
                }
            }
        }

        private void ApplyLabelStyle(Label Label, EbMobileDataColumn DataColumn, bool IsHeader)
        {
            EbFont _font = DataColumn.Font;

            if (_font != null)
            {
                Label.FontSize = (IsHeader) ? (_font.Size + 4) : _font.Size;

                if (_font.Style == FontStyle.BOLD)
                    Label.FontAttributes = FontAttributes.Bold;
                else if (_font.Style == FontStyle.ITALIC)
                    Label.FontAttributes = FontAttributes.Italic;
                else
                    Label.FontAttributes = FontAttributes.None;

                Label.TextColor = (IsHeader) ? Color.White : Color.FromHex(_font.color.TrimStart('#'));

                if (_font.Caps)
                    Label.Text = Label.Text.ToUpper();

                if (_font.Underline)
                    Label.TextDecorations = TextDecorations.Underline;
                else if (_font.Strikethrough)
                    Label.TextDecorations = TextDecorations.Strikethrough;
            }
            else
            {
                if (IsHeader)
                {
                    Label.TextColor = Color.White;
                }
            }
        }

        private void AppendListHeader(StackLayout _Stack)
        {
            StackLayout Sl = new StackLayout { Spacing = 0, BackgroundColor = Color.FromHex("315eff") };

            Grid _Gd = new Grid();
            _Gd.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            _Gd.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            _Gd.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            _Gd.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            _Gd.Children.Add(this.HeaderFrame, 0, 0);
            Grid.SetColumnSpan(this.HeaderFrame, 2);

            _Gd.Children.Add(new Button
            {
                Text = "\uf040",
                Command = new Command(EditButtonClicked),
                Margin = 10,
                Style = (Style)Application.Current.Resources["IconedRoundButton"]
            }, 1, 1);

            Sl.Children.Add(_Gd);
            _Stack.Children.Add(Sl);
        }

        void AddButtonClicked(object sender)
        {
            if (!string.IsNullOrEmpty(Visualization.LinkRefId))
            {
                EbMobilePage _page = HelperFunctions.GetPage(Visualization.LinkRefId);
                if (_page.Container is EbMobileForm)
                {
                    FormRender Renderer = new FormRender(_page);
                    (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(Renderer);
                }
            }
        }

        void EditButtonClicked(object sender)
        {
            if (!string.IsNullOrEmpty(SourceVisualization.SourceFormRefId))
            {
                EbMobilePage _page = HelperFunctions.GetPage(SourceVisualization.SourceFormRefId);
                FormRender Renderer = new FormRender(_page, this.HeaderFrame.DataRow, this.HeaderFrame.Columns);
                (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(Renderer);
            }
        }
    }
}
