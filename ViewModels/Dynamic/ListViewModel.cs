using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.ViewModels.BaseModels;
using ExpressBase.Mobile.Views.Dynamic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public List<EbMobileControl> FilterControls { set; get; }

        public ObservableCollection<SortColumn> SortColumns { set; get; }

        private SortColumn CurrentSort;

        public ListViewModel(EbMobilePage page) : base(page)
        {
            this.Visualization = this.Page.Container as EbMobileVisualization;
            this.FilterControls = this.Visualization.FilterControls;

            //convert EbDataColumn to sortColumn
            this.SortColumns = new ObservableCollection<SortColumn>();
            foreach (var col in this.Visualization.SortColumns)
            {
                this.SortColumns.Add(new SortColumn { Name = col.ColumnName, Order = col.SortOrder });
            }
        }

        public async Task SetData(int offset = 0)
        {
            try
            {
                if (this.Page.NetworkMode == NetworkMode.Online && !Settings.HasInternet)
                {
                    DependencyService.Get<IToast>().Show("You are not connected to internet.");
                    throw new Exception("no internet");
                }

                await Task.Run(() =>
                {
                    EbDataSet ds = this.Visualization.GetData(this.Page.NetworkMode, offset);
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
                DataTable = new EbDataTable();
                Log.Write(ex.Message);
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

        public List<DbParameter> GetFilterValues()
        {
            List<DbParameter> p = new List<DbParameter>();

            foreach (EbMobileControl ctrl in FilterControls)
            {
                object value = ctrl.GetValue();

                if (value != null)
                {
                    p.Add(new DbParameter
                    {
                        DbType = (int)ctrl.EbDbType,
                        ParameterName = ctrl.Name,
                        Value = value
                    });
                }
            }
            return p.Any() ? p : null;
        }

        public async Task Refresh(List<DbParameter> parameters)
        {
            try
            {
                await Task.Run(() =>
                {
                    DataTable.Rows.Clear();
                    DataCount = 0;

                    EbDataSet ds = this.Visualization.GetData(this.Page.NetworkMode, 0, parameters);

                    if (ds != null && ds.Tables.HasLength(2))
                    {
                        DataTable = ds.Tables[1];
                        DataCount = Convert.ToInt32(ds.Tables[0].Rows[0]["count"]);
                    }
                });
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }

        public async Task UpdateSortView(SortColumn column)
        {
            try
            {
                await Task.Run(() =>
                {
                    if (column != CurrentSort)
                    {
                        this.CurrentSort = column;
                        List<SortColumn> temp = new List<SortColumn>(this.SortColumns);

                        this.SortColumns.Clear();
                        foreach (var item in temp)
                        {
                            if (item.Name == column.Name)
                                item.Selected = true;
                            else
                                item.Selected = false;

                            this.SortColumns.Add(item);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }

        public async Task SortList()
        {
            try
            {
                await Task.Delay(1);

                this.DataTable.Sort(CurrentSort.Name, CurrentSort.Order);

            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }
    }
}
