using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Structures;
using ExpressBase.Mobile.ViewModels.BaseModels;
using ExpressBase.Mobile.Views.Dynamic;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels.Dynamic
{
    public class ListViewRenderViewModel : DynamicBaseViewModel
    {
        public int DataCount { set; get; }

        public EbMobileVisualization Visualization { set; get; }

        public EbDataTable DataTable { set; get; }

        public StackLayout FilterDialog { set; get; }

        public Dictionary<string, View> FilterControls { set; get; }

        public ListViewRenderViewModel(EbMobilePage page) : base(page)
        {
            this.Visualization = this.Page.Container as EbMobileVisualization;
            this.SetData();//get query result
            this.CreateView();
            if (!this.Visualization.Filters.IsNullOrEmpty())
                CreateFilter();
        }

        public void SetData(int offset = 0)
        {
            try
            {
                EbDataSet ds = null;
                if (this.NetworkType == NetworkMode.Online)
                    ds = GetDataFromLive(offset);
                else if (this.NetworkType == NetworkMode.Offline)
                    ds = GetDataFromLocal(offset);
                else
                {
                    if (Settings.HasInternet)
                        ds = GetDataFromLive(offset);
                    else
                        ds = GetDataFromLocal(offset);
                }

                if (ds != null && ds.Tables.HasIndex(2))
                {
                    DataTable = ds.Tables[1];
                    DataCount = Convert.ToInt32(ds.Tables[0].Rows[0]["count"]);
                }
                else
                    DataTable = new EbDataTable();
            }
            catch (Exception ex)
            {
                Log.Write("List_SetData---" + ex.Message);
            }
        }

        private EbDataSet GetDataFromLive(int offset)
        {
            EbDataSet ds = null;
            try
            {
                Auth.AuthIfTokenExpired();
                VisualizationLiveData vd = RestServices.Instance.PullReaderData(Visualization.DataSourceRefId, null, this.Visualization.PageLength, offset);
                ds = vd.Data;
            }
            catch (Exception ex)
            {
                Log.Write("ListViewRenderViewModel.GetDataFromLive---" + ex.Message);
            }
            return ds;
        }

        private EbDataSet GetDataFromLocal(int offset)
        {
            EbDataSet ds = null;
            try
            {
                var sqlParams = HelperFunctions.GetSqlParams(this.Visualization.GetQuery);

                if (sqlParams.Count > 0)
                {
                    List<DbParameter> dbParams = new List<DbParameter>();
                    foreach (string s in sqlParams)
                        dbParams.Add(new DbParameter { ParameterName = s });

                    ds = this.Visualization.GetLocalData(dbParams, offset);
                }
                else
                    ds = this.Visualization.GetLocalData(offset);
            }
            catch (Exception ex)
            {
                Log.Write("ListViewRenderViewModel.GetDataFromLocal---" + ex.Message);
            }
            return ds;
        }

        public void CreateView()
        {
            StackLayout StackL = new StackLayout { Spacing = 0 };

            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += ListItem_Clicked;
            int _rowColCount = 1;

            foreach (EbDataRow _row in this.DataTable.Rows)
            {
                CustomFrame CustFrame = new CustomFrame(_row, this.Visualization);

                if (this.NetworkType == NetworkMode.Offline)
                    CustFrame.ShowSyncFlag(this.DataTable.Columns);

                CustFrame.SetBackGroundColor(_rowColCount);
                CustFrame.GestureRecognizers.Add(tapGestureRecognizer);

                StackL.Children.Add(CustFrame);
                _rowColCount++;
            }
            this.XView = StackL;
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
            try
            {
                if (string.IsNullOrEmpty(this.Visualization.LinkRefId)) return;

                EbMobilePage _page = HelperFunctions.GetPage(Visualization.LinkRefId);

                if (this.NetworkType != _page.NetworkMode)
                {
                    DependencyService.Get<IToast>().Show("Link page Mode is different.");
                    return;
                }

                if (_page.Container is EbMobileForm)
                {
                    if (this.Visualization.FormMode == WebFormDVModes.New_Mode)
                    {
                        FormRender Renderer = new FormRender(_page, customFrame.DataRow);//to form newmode prefill
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
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }

        public void Refresh(List<DbParameter> parameters)
        {
            if (parameters != null)
            {
                var ds = this.Visualization.GetLocalData(parameters);
                DataTable = ds.Tables[1];
            }
            this.CreateView();
        }
    }
}
