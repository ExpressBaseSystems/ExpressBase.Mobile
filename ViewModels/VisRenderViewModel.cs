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

namespace ExpressBase.Mobile.ViewModels
{
    public class VisRenderViewModel : BaseViewModel
    {
        public bool IsRedirect { set; get; } = false;

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

            //list header
            this.HeaderFrame = new CustomFrame(CustFrame.DataRow, CustFrame.Columns, SourceVis, true)
            {
                BackgroundColor = Color.Transparent,
                Padding = new Thickness(20, 10, 20, 0),
                Margin = 0
            };

            PageTitle = LinkPage.DisplayName;
            this.Visualization = (LinkPage.Container as EbMobileVisualization);
            this.GetData();
            this.CreateView();
        }

        private void GetData()
        {
            byte[] b = Convert.FromBase64String(this.Visualization.OfflineQuery.Code);
            string sql = HelperFunctions.WrapSelectQuery(System.Text.Encoding.UTF8.GetString(b));
            List<DbParameter> _DbParams = new List<DbParameter>();
            try
            {
                if (RenderType == VisRenderType.LIST2LIST)
                {
                    List<string> _Params = HelperFunctions.GetSqlParams(sql);
                    if (_Params.Count > 0)
                    {
                        this.GetParameterValues(_DbParams, _Params);
                    }
                }

                DataTable = App.DataDB.DoQuery(sql, _DbParams.ToArray());
            }
            catch (Exception e)
            {
                DataTable = new EbDataTable();
                Console.WriteLine(e.Message);
            }
        }

        private void GetParameterValues(List<DbParameter> _DbParams, List<string> _Params)
        {
            try
            {
                foreach (string _p in _Params)
                {
                    _DbParams.Add(new DbParameter
                    {
                        ParameterName = _p,
                        Value = this.HeaderFrame.DataRow[_p] ?? null,
                        DbType = (int)this.HeaderFrame.Columns[_p].Type
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
                CustomFrame CustFrame = new CustomFrame(_row, this.DataTable.Columns, this.Visualization);
                CustFrame.SetBackGroundColor(_rowColCount);
                CustFrame.GestureRecognizers.Add(tapGestureRecognizer);

                StackL.Children.Add(CustFrame);

                _rowColCount++;
            }

            this.View = new ScrollView { Content = StackL };
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

        private void AppendListHeader(StackLayout _Stack)
        {
            StackLayout Sl = new StackLayout { Spacing = 0, BackgroundColor = Color.FromHex("315eff") };

            Grid _Gd = new Grid();
            _Gd.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            _Gd.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            _Gd.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            _Gd.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

            _Gd.Children.Add(this.HeaderFrame, 0, 0);
            Grid.SetColumnSpan(this.HeaderFrame, 2);

            _Gd.Children.Add(new Button
            {
                Text = "\uf040",
                Command = new Command(EditButtonClicked),
                Margin = new Thickness(20,10,0,10),
                Style = (Style)Application.Current.Resources["IconedRoundButton"]
            }, 0, 1);

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
                    if (!string.IsNullOrEmpty(SourceVisualization.SourceFormRefId))
                    {
                        this.IsRedirect = true;
                        EbMobilePage ParentForm = HelperFunctions.GetPage(SourceVisualization.SourceFormRefId);

                        FormRender Renderer = new FormRender(_page, ParentForm, this.HeaderFrame.DataRow);
                        (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(Renderer);
                    }
                }
            }
        }

        void EditButtonClicked(object sender)
        {
            if (!string.IsNullOrEmpty(SourceVisualization.SourceFormRefId))
            {
                this.IsRedirect = true;
                EbMobilePage _page = HelperFunctions.GetPage(SourceVisualization.SourceFormRefId);
                FormRender Renderer = new FormRender(_page, this.HeaderFrame.DataRow, this.HeaderFrame.Columns);
                (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(Renderer);
            }
        }

        public override void RefreshPage()
        {
            this.View = null;
            this.GetData();
            this.CreateView();
        }
    }
}
