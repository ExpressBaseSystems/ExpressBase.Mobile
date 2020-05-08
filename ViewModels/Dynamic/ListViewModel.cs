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
using System.Threading.Tasks;
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

            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += ListItem_Clicked;
            bool IsClickable = !string.IsNullOrEmpty(this.Visualization.LinkRefId);

            if (this.DataTable.Rows.Any())
            {
                foreach (EbDataRow _row in this.DataTable.Rows)
                {
                    CustomFrame CustFrame = new CustomFrame(_row, this.Visualization);
                    CustFrame.SetBackGroundColor(rowColCount);

                    if (this.NetworkType == NetworkMode.Offline)
                        CustFrame.ShowSyncFlag(this.DataTable.Columns);

                    if (IsClickable) CustFrame.GestureRecognizers.Add(tapGestureRecognizer);
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

        private bool IsTapped;

        async void ListItem_Clicked(object Frame, EventArgs args)
        {
            if (IsTapped) return;

            IToast toast = DependencyService.Get<IToast>();
            CustomFrame customFrame = (CustomFrame)Frame;
            try
            {
                EbMobilePage _page = HelperFunctions.GetPage(Visualization.LinkRefId);

                if (this.NetworkType != _page.NetworkMode)
                {
                    toast.Show("Link page Mode is different.");
                    return;
                }
                IsTapped = IsBusy = true;

                ContentPage renderer = await GetPageByContainer(customFrame, _page);
                if (renderer != null)
                    await (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(renderer);

                IsBusy = IsBusy = false;
            }
            catch (Exception ex)
            {
                IsTapped = IsBusy = false;
                Log.Write(ex.Message);
                toast.Show("something went wrong");
            }
        }

        private async Task<ContentPage> GetPageByContainer(CustomFrame frame, EbMobilePage page)
        {
            ContentPage renderer = null;
            try
            {
                await Task.Delay(100);

                switch (page.Container)
                {
                    case EbMobileForm f:

                        if (this.Visualization.FormMode == WebFormDVModes.New_Mode)
                            renderer = new FormRender(page, frame.DataRow);
                        else
                        {
                            int id = Convert.ToInt32(frame.DataRow["id"]);
                            if (id <= 0) throw new Exception("id has ivalid value" + id);
                            renderer = new FormRender(page, id);
                        }
                        break;
                    case EbMobileVisualization v:
                        renderer = new LinkedListRender(page, this.Visualization, frame);
                        break;
                    case EbMobileDashBoard d:
                        renderer = new DashBoardRender(page, frame.DataRow);
                        break;
                    default:
                        Log.Write("inavlid container type");
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
            return renderer;
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
