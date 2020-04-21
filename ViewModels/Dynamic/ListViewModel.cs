using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Structures;
using ExpressBase.Mobile.ViewModels.BaseModels;
using ExpressBase.Mobile.Views.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels.Dynamic
{
    public class ListViewModel : DynamicBaseViewModel
    {
        public int DataCount { set; get; }

        public EbMobileVisualization Visualization { set; get; }

        public EbDataTable DataTable { set; get; }

        public StackLayout FilterDialog { set; get; }

        public Dictionary<string, View> FilterControls { set; get; }

        public ListViewModel(EbMobilePage page) : base(page)
        {
            this.Visualization = this.Page.Container as EbMobileVisualization;
            this.SetData();
            this.CreateView();
            if (!this.Visualization.Filters.IsNullOrEmpty())
                this.CreateFilter();
        }

        public void SetData(int offset = 0)
        {
            try
            {
                if (this.Page.NetworkMode == NetworkMode.Online && !Settings.HasInternet)
                {
                    DependencyService.Get<IToast>().Show("You are not connected to internet.");
                    throw new Exception("no internet");
                }

                EbDataSet ds = this.Visualization.GetData(this.Page.NetworkMode, offset);
                if (ds != null && ds.Tables.HasLength(2))
                {
                    DataTable = ds.Tables[1];
                    DataCount = Convert.ToInt32(ds.Tables[0].Rows[0]["count"]);
                }
                else
                    throw new Exception("no internet");
            }
            catch (Exception ex)
            {
                DataTable = new EbDataTable();
                Log.Write(ex.Message);
            }
        }

        public void CreateView()
        {
            StackLayout StackL = new StackLayout { Spacing = 1, BackgroundColor = Color.FromHex("eeeeee") };
            int rowColCount = 1;

            if (this.DataTable.Rows.Any())
            {
                foreach (EbDataRow _row in this.DataTable.Rows)
                {
                    CustomFrame CustFrame = new CustomFrame(_row, this.Visualization);
                    CustFrame.SetBackGroundColor(rowColCount);
                    var tapGestureRecognizer = new TapGestureRecognizer();
                    tapGestureRecognizer.Tapped += ListItem_Clicked;

                    if (this.NetworkType == NetworkMode.Offline)
                        CustFrame.ShowSyncFlag(this.DataTable.Columns);

                    CustFrame.GestureRecognizers.Add(tapGestureRecognizer);
                    StackL.Children.Add(CustFrame);
                    rowColCount++;
                }
            }
            else
            {
                StackL.Children.Add(new Label
                {
                    Text = "Empty list.",
                    FontSize = 16,
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    HorizontalOptions = LayoutOptions.Center
                });
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
                return new NumericTextBox() { Keyboard = Keyboard.Numeric };
            else if (col.Type == EbDbTypes.Decimal || col.Type == EbDbTypes.Double)
                return new NumericTextBox() { Keyboard = Keyboard.Numeric };
            else if (col.Type == EbDbTypes.Date || col.Type == EbDbTypes.DateTime)
                return new CustomDatePicker();
            else if (col.Type == EbDbTypes.Boolean)
                return new CustomCheckBox();
            else
                return new TextBox();
        }

        void ListItem_Clicked(object Frame, EventArgs args)
        {
            IToast toast = DependencyService.Get<IToast>();
            CustomFrame customFrame = Frame as CustomFrame;
            try
            {
                if (string.IsNullOrEmpty(this.Visualization.LinkRefId)) return;
                EbMobilePage _page = HelperFunctions.GetPage(Visualization.LinkRefId);

                if (this.NetworkType != _page.NetworkMode)
                {
                    toast.Show("Link page Mode is different.");
                    return;
                }
                Device.BeginInvokeOnMainThread(() => IsBusy = true);
                if (_page.Container is EbMobileForm)
                {
                    int id = Convert.ToInt32(customFrame.DataRow["id"]);
                    if (id == 0) throw new Exception("id has ivalid value" + id);
                    FormMode mode = this.Visualization.FormMode == WebFormDVModes.New_Mode ? FormMode.NEW : FormMode.EDIT;

                    FormRender Renderer = new FormRender(_page, id, mode);
                    (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(Renderer);
                }
                else if (_page.Container is EbMobileVisualization)
                {
                    LinkedListRender Renderer = new LinkedListRender(_page, this.Visualization, customFrame);
                    (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(Renderer);
                }
                else if (_page.Container is EbMobileDashBoard)
                {
                    DashBoardRender Renderer = new DashBoardRender(_page, customFrame.DataRow);
                    (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(Renderer);
                }
                Device.BeginInvokeOnMainThread(() => IsBusy = false);
            }
            catch (Exception ex)
            {
                Device.BeginInvokeOnMainThread(() => IsBusy = false);
                Log.Write(ex.Message);
                toast.Show("something went wrong");
            }
        }

        public void Refresh(List<DbParameter> parameters)
        {
            try
            {
                if (parameters != null)
                {
                    var ds = this.Visualization.GetData(this.Page.NetworkMode, 0, parameters);
                    if (ds.Tables.HasLength(2))
                    {
                        DataTable = ds.Tables[1];
                        DataCount = Convert.ToInt32(ds.Tables[0].Rows[0]["count"]);
                    }
                }
                else
                {
                    var ds = this.Visualization.GetData(this.Page.NetworkMode, 0);
                    if (ds.Tables.HasLength(2))
                    {
                        DataTable = ds.Tables[1];
                        DataCount = Convert.ToInt32(ds.Tables[0].Rows[0]["count"]);
                    }
                }
                this.CreateView();
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }
    }
}
