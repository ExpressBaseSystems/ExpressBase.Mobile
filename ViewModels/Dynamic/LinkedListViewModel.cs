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
using System.Threading.Tasks;
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

        public List<DbParameter> Parameters { set; get; }

        public Command AddCommand { get; set; }

        public Command EditCommand { get; set; }

        public LinkedListViewModel() { }

        public LinkedListViewModel(EbMobilePage LinkPage, EbMobileVisualization SourceVis, CustomFrame CustFrame) : base(LinkPage)
        {
            this.Visualization = LinkPage.Container as EbMobileVisualization;
            this.SourceVisualization = SourceVis;

            this.HeaderFrame = new CustomFrame(CustFrame.DataRow, SourceVis, true)
            {
                BackgroundColor = Color.Transparent,
                Padding = new Thickness(20, 10, 20, 0),
                Margin = 0
            };
            this.SetParameters(CustFrame.DataRow);

            AddCommand = new Command(AddButtonClicked);
            EditCommand = new Command(EditButtonClicked);
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

        public async Task SetData(int offset = 0)
        {
            try
            {
                if (this.Page.NetworkMode == NetworkMode.Online && !Utils.HasInternet)
                {
                    DependencyService.Get<IToast>().Show("You are not connected to internet.");
                    throw new Exception("no internet");
                }

                await Task.Run(() =>
                {
                    EbDataSet ds = this.Visualization.GetData(this.Page.NetworkMode, offset, this.Parameters);
                    if (ds != null && ds.Tables.HasLength(2))
                    {
                        DataTable = ds.Tables[1];
                        DataCount = Convert.ToInt32(ds.Tables[0].Rows[0]["count"]);
                    }
                    else
                        throw new Exception("no internet");
                });
            }
            catch (Exception ex)
            {
                Log.Write("LinkedList_SetData---" + ex.Message);
            }
        }

        async void AddButtonClicked(object sender)
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
                        await (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(Renderer);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write("LinkedListViewModel.AddButtonClicked::" + ex.Message);
            }
        }

        async void EditButtonClicked(object sender)
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
                        await (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(Renderer);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write("LinkedListViewModel.EditButtonClicked::" + ex.Message);
            }
        }

        public async Task<ContentPage> GetPageByContainer(CustomFrame frame, EbMobilePage page)
        {
            ContentPage renderer = null;
            try
            {
                await Task.Delay(10);

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
    }
}
