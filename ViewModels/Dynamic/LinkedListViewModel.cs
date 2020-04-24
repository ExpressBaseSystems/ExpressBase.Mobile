using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.ViewModels.BaseModels;
using ExpressBase.Mobile.Views.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels.Dynamic
{
    public class LinkedListViewModel : DynamicBaseViewModel
    {
        public EbMobileVisualization SourceVisualization { set; get; }

        public EbMobileVisualization Visualization { set; get; }

        public CustomFrame HeaderFrame { set; get; }

        public int DataCount { set; get; }

        public EbDataTable DataTable { set; get; }

        private List<DbParameter> Parameters { set; get; }

        public Command AddCommand => new Command(AddButtonClicked);

        public Command EditCommand => new Command(EditButtonClicked);

        public LinkedListViewModel() { }

        public LinkedListViewModel(EbMobilePage LinkPage, EbMobileVisualization SourceVis, CustomFrame CustFrame) : base(LinkPage)
        {
            this.Visualization = LinkPage.Container as EbMobileVisualization;
            SourceVisualization = SourceVis;

            this.HeaderFrame = new CustomFrame(CustFrame.DataRow, SourceVis, true)
            {
                BackgroundColor = Color.Transparent,
                Padding = new Thickness(20, 10, 20, 0),
                Margin = 0
            };
            this.SetParameters(CustFrame.DataRow);
            this.SetData(); //set query result
            this.CreateView();
        }

        public void SetParameters(EbDataRow row)
        {
            try
            {
                Parameters = new List<DbParameter>();
                if (this.Page.NetworkMode == NetworkMode.Online)
                {
                    foreach (Param param in this.Visualization.DataSourceParams)
                    {
                        object data = row[param.Name];
                        if (data != null)
                        {
                            Parameters.Add(new DbParameter
                            {
                                ParameterName = param.Name,
                                DbType = Convert.ToInt32(param.Type),
                                Value = data
                            });
                        }
                    }
                }
                else
                {
                    string sql = HelperFunctions.B64ToString(this.Visualization.OfflineQuery.Code);
                    List<string> _parameters = HelperFunctions.GetSqlParams(sql);

                    foreach (string param in _parameters)
                    {
                        object data = row[param];
                        if (data != null)
                        {
                            Parameters.Add(new DbParameter
                            {
                                ParameterName = param,
                                Value = data
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
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

                EbDataSet ds = this.Visualization.GetData(this.Page.NetworkMode, offset, this.Parameters);
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
                Log.Write("LinkedList_SetData---" + ex.Message);
            }
        }

        public void CreateView()
        {
            StackLayout StackL = new StackLayout { Spacing = 1, BackgroundColor = Color.FromHex("eeeeee") };

            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += ListItem_Clicked;
            int rowColCount = 1;
            if (this.DataTable.Rows.Any())
            {
                foreach (EbDataRow _row in this.DataTable.Rows)
                {
                    CustomFrame CustFrame = new CustomFrame(_row, this.Visualization);
                    CustFrame.SetBackGroundColor(rowColCount);

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

        void AddButtonClicked(object sender)
        {
            try
            {
                EbMobilePage _page = HelperFunctions.GetPage(Visualization.LinkRefId);

                if (_page != null && _page.Container is EbMobileForm)
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
            catch (Exception ex)
            {
                Log.Write("LinkedListViewModel.AddButtonClicked::" + ex.Message);
            }
        }

        void EditButtonClicked(object sender)
        {
            try
            {
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
            catch (Exception ex)
            {
                Log.Write("LinkedListViewModel.EditButtonClicked::" + ex.Message);
            }
        }

        void ListItem_Clicked(object Frame, EventArgs args)
        {
            CustomFrame customFrame = Frame as CustomFrame;
            try
            {
                EbMobilePage _page = HelperFunctions.GetPage(this.Visualization.LinkRefId);

                if (this.NetworkType != _page.NetworkMode)
                {
                    DependencyService.Get<IToast>().Show("Link page Mode is different.");
                    return;
                }

                if (_page != null && _page.Container is EbMobileForm)
                {
                    FormRender Renderer;
                    if (this.Visualization.FormMode == WebFormDVModes.New_Mode)
                        Renderer = new FormRender(_page, customFrame.DataRow);
                    else
                    {
                        int id = Convert.ToInt32(customFrame.DataRow["id"]);
                        if (id <= 0) throw new Exception("id has ivalid value" + id);
                        Renderer = new FormRender(_page, id);
                    }
                    (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(Renderer);
                }
            }
            catch (Exception ex)
            {
                Log.Write("LinkedListViewModel.ListItem_Clicked::" + ex.Message);
            }
        }
    }
}
