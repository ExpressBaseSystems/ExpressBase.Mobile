using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Structures;
using ExpressBase.Mobile.Views.Dynamic;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels.Dynamic
{
    public class ListViewRenderViewModel : BaseViewModel
    {
        public int DataCount { set; get; }

        public EbMobileVisualization Visualization { set; get; }

        public EbDataTable DataTable { set; get; }

        public StackLayout View { set; get; }

        public StackLayout FilterDialog { set; get; }

        public Dictionary<string, View> FilterControls { set; get; }

        public ListViewRenderViewModel(EbMobilePage Page)
        {
            PageTitle = Page.DisplayName;
            this.Visualization = (Page.Container as EbMobileVisualization);

            this.SetData();//get query result
            this.CreateView();

            if (!this.Visualization.Filters.IsNullOrEmpty())
                CreateFilter();
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
                        dbParams.Add(new DbParameter { ParameterName = s });

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
                Log.Write("List_SetData---" + ex.Message);
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

        private void CreateFilter()
        {
            FilterControls = new Dictionary<string, View>();

            var stack = new StackLayout { Orientation = StackOrientation.Vertical };

            foreach (EbMobileDataColumn col in this.Visualization.Filters)
            {
                stack.Children.Add(new Label
                {
                    Text = col.ColumnName,
                    VerticalTextAlignment = TextAlignment.Center,
                    FontSize = 15
                });
                var view = this.GetFilterControl(col);
                view.Margin = new Thickness(0, 0, 0, 10);
                stack.Children.Add(view);
                FilterControls.Add(col.ColumnName, view);
            }
            this.FilterDialog = stack;
        }

        private View GetFilterControl(EbMobileDataColumn col)
        {
            if (col.Type == EbDbTypes.String)
                return new TextBox();
            else if (col.Type == EbDbTypes.Int16 || col.Type == EbDbTypes.Int32)
                return new TextBox() { Keyboard = Keyboard.Numeric };
            else if (col.Type == EbDbTypes.Decimal || col.Type == EbDbTypes.Double)
                return new TextBox() { Keyboard = Keyboard.Numeric };
            else if (col.Type == EbDbTypes.Date || col.Type == EbDbTypes.DateTime)
                return new CustomDatePicker();
            else if (col.Type == EbDbTypes.Boolean)
                return new CustomCheckBox();
            else
                return new TextBox();
        }

        void ListItem_Clicked(object Frame, EventArgs args)
        {
            var customFrame = Frame as CustomFrame;

            if (!string.IsNullOrEmpty(this.Visualization.LinkRefId))
            {
                EbMobilePage _page = HelperFunctions.GetPage(Visualization.LinkRefId);

                if (_page.Container is EbMobileForm)
                {
                    if (this.Visualization.FormMode == WebFormDVModes.New_Mode)
                    {
                        FormRender Renderer = new FormRender(_page, customFrame.DataRow, customFrame.Columns);//to form newmode prefill
                        (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(Renderer);
                    }
                    else
                    {
                        int id = Convert.ToInt32(customFrame.DataRow["id"]);
                        if (id != 0)
                        {
                            FormRender Renderer = new FormRender(_page, id);//to form edit mode
                            (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(Renderer);
                        }
                    }
                }
                else if (_page.Container is EbMobileVisualization)
                {
                    LinkedListViewRender Renderer = new LinkedListViewRender(_page, this.Visualization, customFrame);
                    (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(Renderer);
                }
                else if (_page.Container is EbMobileDashBoard)
                {
                    DashBoardRender Renderer = new DashBoardRender(_page, customFrame.DataRow);
                    (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(Renderer);
                }
            }
        }

        public void Refresh(List<DbParameter> parameters)
        {
            if (parameters != null)
            {
                var ds = this.Visualization.GetData(parameters);
                DataTable = ds.Tables[1];
            }
            this.CreateView();
        }
    }
}
