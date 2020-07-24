using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
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

        public Command AddCommand => new Command(async () => await AddButtonClicked());

        public Command EditCommand => new Command(async () => await EditButtonClicked());

        private readonly CustomFrame linkFrame;

        public LinkedListViewModel(EbMobilePage page, EbMobileVisualization sourcevis, CustomFrame linkframe) : base(page)
        {
            this.Visualization = (EbMobileVisualization)page.Container;
            this.SourceVisualization = sourcevis;
            linkFrame = linkframe;
        }

        public override async Task InitializeAsync()
        {
            this.HeaderFrame = new CustomFrame(linkFrame.DataRow, this.SourceVisualization, true)
            {
                BackgroundColor = Color.Transparent,
                Padding = new Thickness(20, 10, 20, 0),
                Margin = 0
            };
            this.SetParameters(linkFrame.DataRow);
            await SetData();
        }

        public void SetParameters(EbDataRow row)
        {
            try
            {
                Parameters = new List<DbParameter>();

                if (this.Page.NetworkMode == NetworkMode.Online)
                {
                    EbLog.Write($"{this.Page.DisplayName} rendered with {this.Visualization.DataSourceParams.Count} params");

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
                EbLog.Write(ex.Message);
            }
        }

        public async Task SetData(int offset = 0)
        {
            try
            {
                if (this.Page.NetworkMode == NetworkMode.Online && !Utils.HasInternet)
                {
                    Utils.Alert_NoInternet();
                    throw new Exception("no internet");
                }

                EbDataSet ds = await this.Visualization.GetData(this.Page.NetworkMode, offset, this.Parameters);
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
                EbLog.Write("LinkedList_SetData---" + ex.Message);
            }
        }

        private async Task AddButtonClicked()
        {
            EbMobilePage page = HelperFunctions.GetPage(Visualization.LinkRefId);

            if (page != null && page.Container is EbMobileForm)
            {
                FormRender Renderer = new FormRender(page, Visualization, linkFrame.DataRow, 0);
                await (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(Renderer);
            }
        }

        private async Task EditButtonClicked()
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

        public ContentPage GetPageByContainer(CustomFrame frame, EbMobilePage page)
        {
            ContentPage renderer = null;
            try
            {
                switch (page.Container)
                {
                    case EbMobileForm f:
                        if (this.Visualization.FormMode == WebFormDVModes.New_Mode)
                            renderer = new FormRender(page, Visualization, frame.DataRow);
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
                        EbLog.Write("inavlid container type");
                        break;
                }
            }
            catch (Exception ex)
            {
                EbLog.Write(ex.Message);
            }
            return renderer;
        }
    }
}
