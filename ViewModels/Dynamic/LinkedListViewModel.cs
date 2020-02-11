using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Views.Dynamic;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels.Dynamic
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

            this.SetData();
            //this.GetData();
            this.CreateView();

            AddCommand = new Command(AddButtonClicked);
            EditCommand = new Command(EditButtonClicked);
        }

        private void SetData()
        {
            try
            {
                var sqlParams = HelperFunctions.GetSqlParams(this.Visualization.GetQuery);

                if (sqlParams.Count > 0)
                {
                    List<DbParameter> dbParams = new List<DbParameter>();
                    foreach (string s in sqlParams)
                        dbParams.Add(new DbParameter { ParameterName = s, Value = this.HeaderFrame.DataRow?[s] });

                    DataTable = this.Visualization.GetData(dbParams);
                }
                else
                    DataTable = this.Visualization.GetData();
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
            if (string.IsNullOrEmpty(Visualization.LinkRefId))
                return;

            EbMobilePage _page = HelperFunctions.GetPage(Visualization.LinkRefId);
            if (_page.Container is EbMobileForm)
            {
                if (string.IsNullOrEmpty(SourceVisualization.SourceFormRefId))
                    return;

                this.IsRedirect = true;
                EbMobilePage ParentForm = HelperFunctions.GetPage(SourceVisualization.SourceFormRefId);

                int id = Convert.ToInt32(this.HeaderFrame.DataRow["id"]);
                if (id != 0)
                {
                    FormRender Renderer = new FormRender(_page, ParentForm, id);
                    (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(Renderer);
                }
            }
        }

        void EditButtonClicked(object sender)
        {
            if (string.IsNullOrEmpty(SourceVisualization.SourceFormRefId))
                return;
            EbMobilePage _page = HelperFunctions.GetPage(SourceVisualization.SourceFormRefId);

            if (_page != null)
            {
                int id = Convert.ToInt32(this.HeaderFrame.DataRow["id"]);
                if (id != 0)
                {
                    this.IsRedirect = true;
                    FormRender Renderer = new FormRender(_page, id);
                    (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(Renderer);
                }
            }
        }

        void VisNodeCommand(object Frame, EventArgs args)
        {
            if (string.IsNullOrEmpty(this.Visualization.LinkRefId))
                return;

            EbMobilePage _page = HelperFunctions.GetPage(Visualization.LinkRefId);

            if (_page != null && _page.Container is EbMobileForm)
            {
                int id = Convert.ToInt32((Frame as CustomFrame).DataRow["id"]);
                if (id != 0)
                {
                    FormRender Renderer = new FormRender(_page, id);//to form edit mode
                    (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(Renderer);
                }
            }
        }

        public override void RefreshPage()
        {
            this.View = null;
            this.SetData();
            this.CreateView();
        }
    }
}
