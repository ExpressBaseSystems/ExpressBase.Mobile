using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Views.Dynamic;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels.Dynamic
{
    public class LinkedListViewModel : BaseViewModel
    {
        public EbMobileVisualization SourceVisualization { set; get; }

        public EbMobileVisualization Visualization { set; get; }

        public CustomFrame HeaderFrame { set; get; }

        public int DataCount { set; get; }

        public EbDataTable DataTable { set; get; }

        public StackLayout View { set; get; }

        public Command AddCommand => new Command(AddButtonClicked);

        public Command EditCommand => new Command(EditButtonClicked);

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

            this.SetData(); //set query result
            this.CreateView();
        }

        public void SetData(int offset = 0)
        {
            try
            {
                EbDataSet ds;
                var sqlParams = HelperFunctions.GetSqlParams(this.Visualization.GetQuery);

                if (sqlParams.Count > 0)
                {
                    List<DbParameter> dbParams = new List<DbParameter>();
                    foreach (string s in sqlParams)
                        dbParams.Add(new DbParameter { ParameterName = s, Value = this.HeaderFrame.DataRow?[s] });

                    ds = this.Visualization.GetData(dbParams, offset);
                }
                else
                    ds = this.Visualization.GetData(offset);

                if (ds.Tables.HasIndex(2))
                {
                    DataTable = ds.Tables[1];
                    DataCount = Convert.ToInt32(ds.Tables[0].Rows[0]["row_count"]);
                }
                else
                    DataTable = new EbDataTable();
            }
            catch (Exception ex)
            {
                Log.Write("LinkedList_SetData---" + ex.Message);
            }
        }

        public void CreateView()
        {
            StackLayout StackL = new StackLayout { Spacing = 0 };

            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += ListItem_Clicked;
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
                    FormRender Renderer = new FormRender(_page, id);
                    (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(Renderer);
                }
            }
        }

        void ListItem_Clicked(object Frame, EventArgs args)
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
    }
}
