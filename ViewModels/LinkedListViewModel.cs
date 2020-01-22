using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Views;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels
{
    public class LinkedListViewModel : BaseViewModel
    {
        public bool IsRedirect { set; get; } = false;

        public EbMobileVisualization SourceVisualization { set; get; }

        public EbMobileVisualization Visualization { set; get; }

        public CustomFrame HeaderFrame { set; get; }

        public EbDataTable DataTable { set; get; }

        public StackLayout View { set; get; }

        public Command AddCommand { set; get; }

        public Command EditCommand { set; get; }

        public LinkedListViewModel() { }

        public LinkedListViewModel(EbMobilePage LinkPage, EbMobileVisualization SourceVis, CustomFrame CustFrame)
        {
            PageTitle = LinkPage.DisplayName;

            this.Visualization = (LinkPage.Container as EbMobileVisualization);
            SourceVisualization = SourceVis;

            this.HeaderFrame = new CustomFrame(CustFrame.DataRow, CustFrame.Columns, SourceVis, true)
            {
                BackgroundColor = Color.Transparent,
                Padding = new Thickness(20, 10, 20, 0),
                Margin = 0
            };

            this.GetData();
            this.CreateView();

            AddCommand = new Command(AddButtonClicked);
            EditCommand = new Command(EditButtonClicked);
        }

        private void GetData()
        {
            byte[] b = Convert.FromBase64String(this.Visualization.OfflineQuery.Code);
            string sql = HelperFunctions.WrapSelectQuery(System.Text.Encoding.UTF8.GetString(b));

            List<DbParameter> _DbParams = new List<DbParameter>();
            try
            {
                List<string> _Params = HelperFunctions.GetSqlParams(sql);
                if (_Params.Count > 0)
                {
                    this.GetParameterValues(_DbParams, _Params);
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

            this.View = StackL;
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
