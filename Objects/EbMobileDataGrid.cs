using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Structures;
using ExpressBase.Mobile.Views.Shared;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbMobileDataGrid : EbMobileControl, ILinesEnabled
    {
        public List<EbMobileControl> ChildControls { set; get; }

        public EbMobileTableLayout DataLayout { set; get; }

        public string TableName { set; get; }

        public string DataSourceRefId { set; get; }

        public EbScript OfflineQuery { set; get; }

        //mobile props
        private Frame container;

        private StackLayout gridHeader;

        private StackLayout gridBody;

        private StackLayout gridFooter;

        private TapGestureRecognizer tapRecognizer;

        private Dictionary<string, MobileTableRow> dataDictionary;

        private Grid CreateGridLayout(string name = null)
        {
            return new Grid
            {
                ClassId = name ?? Guid.NewGuid().ToString("N"),
                VerticalOptions = LayoutOptions.Center,
                Padding = new Thickness(5, 5),
                BackgroundColor = Color.White,
                ColumnDefinitions =
                {
                    new ColumnDefinition{ Width=GridLength.Star },
                    new ColumnDefinition{ Width=GridLength.Auto }
                }
            };
        }

        public override void InitXControl(FormMode mode, NetworkMode network)
        {
            base.InitXControl(mode, network);

            try
            {
                container = new Frame
                {
                    Style = (Style)HelperFunctions.GetResourceValue("DGContainerFrame")
                };

                gridHeader = new StackLayout { Spacing = 0 };
                gridBody = new StackLayout { IsVisible = false, BackgroundColor = Color.FromHex("cccccc"), Spacing = 1 };
                gridFooter = new StackLayout { IsVisible = false };
                dataDictionary = new Dictionary<string, MobileTableRow>();

                StackLayout stackL = new StackLayout
                {
                    Spacing = 0,
                    Children = { gridHeader, gridBody, gridFooter }
                };
                container.Content = stackL;
                tapRecognizer = new TapGestureRecognizer();
                tapRecognizer.Tapped += TapRecognizer_Tapped;

                this.CreateHeader();//creating grid header
                this.CreateFooter();//creating grid footer
                this.XControl = container;
                this.AutoFill();
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
        }

        private void CreateHeader()
        {
            Grid grid = CreateGridLayout();//creating new grid
            grid.BackgroundColor = Color.FromHex("eeeeee");
            Button addRowBtn = new Button
            {
                Style = (Style)HelperFunctions.GetResourceValue("DGNewRowButton")
            };
            addRowBtn.Clicked += AddRowBtn_Clicked;
            grid.Children.Add(addRowBtn, 1, 0);
            DGDynamicFrame frame = new DGDynamicFrame(this.GetControlValues(true), DataLayout.CellCollection, DataLayout.RowCount, DataLayout.ColumCount, true)
            {
                BackgroundColor = Color.Transparent,
                Padding = 0
            };
            grid.Children.Add(frame, 0, 0);
            gridHeader.Children.Add(grid);
            gridHeader.Children.Add(new BoxView { Color = Color.FromHex("cccccc"), HeightRequest = 1 });
        }

        private MobileTableRow GetControlValues(bool isHeader = false)
        {
            MobileTableRow row = new MobileTableRow();
            try
            {
                foreach (EbMobileControl ctrl in this.ChildControls)
                {
                    if (ctrl is INonPersistControl || ctrl is ILinesEnabled)
                        continue;
                    row.Columns.Add(new MobileTableColumn
                    {
                        Name = ctrl.Name,
                        Type = ctrl.EbDbType,
                        Value = isHeader ? ctrl.Label : ctrl.GetValue()
                    });
                }
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
            return row;
        }

        private Grid CreateDataRow(MobileTableRow row, string name = null)
        {
            Grid grid = CreateGridLayout(name);
            try
            {
                Button rowOptions = new Button
                {
                    ClassId = grid.ClassId,
                    Style = (Style)HelperFunctions.GetResourceValue("DGEditRowButton")
                };
                rowOptions.Clicked += RowDelete_Clicked;
                grid.Children.Add(rowOptions, 1, 0);

                grid.Children.Add(new DGDynamicFrame(row, DataLayout.CellCollection, DataLayout.RowCount, DataLayout.ColumCount)
                {
                    ClassId = grid.ClassId,
                    BackgroundColor = Color.Transparent,
                    Padding = 0,
                    GestureRecognizers = { this.tapRecognizer }
                }, 0, 0);

                dataDictionary[grid.ClassId] = row;
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
            return grid;
        }

        private void CreateFooter()
        {
            Grid grid = CreateGridLayout();
            grid.Children.Add(new DGDynamicFrame(this.GetControlValues(true), DataLayout.CellCollection, DataLayout.RowCount, DataLayout.ColumCount)
            {
                BackgroundColor = Color.Transparent,
                Padding = 0
            }, 0, 0);
            gridFooter.Children.Add(grid);
        }

        private void AddRowBtn_Clicked(object sender, EventArgs e)
        {
            DataGridView gridview = new DataGridView(this);
            (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushModalAsync(gridview);
        }

        private void RowDelete_Clicked(object sender, EventArgs e)
        {
            Button button = sender as Button;
            foreach (View el in gridBody.Children)
            {
                if (el.ClassId == button.ClassId)
                {
                    gridBody.Children.Remove(el);
                    if (this.FormRenderMode == FormMode.EDIT)
                    {
                        if (dataDictionary.TryGetValue(el.ClassId, out MobileTableRow row))
                            if (row.RowId == 0) dataDictionary.Remove(el.ClassId); else row.IsDelete = true;
                    }
                    else
                        dataDictionary.Remove(el.ClassId);
                    break;
                }
            }
        }

        public void RowAddCallBack(string name = null)
        {
            try
            {
                MobileTableRow values = this.GetControlValues();
                if (name == null)
                {
                    Grid grid = this.CreateDataRow(values);
                    gridBody.Children.Add(grid);
                }
                else
                {
                    MobileTableRow row = this.GetControlValues();
                    MobileTableRow oldRow = dataDictionary[name];
                    row.RowId = oldRow.RowId;
                    dataDictionary[name] = row;

                    for (int i = 0; i < gridBody.Children.Count; i++)
                    {
                        if (gridBody.Children[i].ClassId == name)
                        {
                            gridBody.Children.Remove(gridBody.Children[i]);
                            Grid ig = this.CreateDataRow(row, name);
                            gridBody.Children.Insert(i, ig);
                            break;
                        }
                    }
                }
                if (gridBody.Children.Count > 0)
                    gridBody.IsVisible = true;
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
        }

        private void TapRecognizer_Tapped(object sender, EventArgs e)
        {
            string classId = (sender as DynamicFrame).ClassId;
            DataGridView gridview = new DataGridView(this, dataDictionary[classId], classId);
            (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushModalAsync(gridview);
        }

        public override object GetValue()
        {
            try
            {
                MobileTable gTable = new MobileTable(this.TableName);
                foreach (KeyValuePair<string, MobileTableRow> pair in dataDictionary)
                {
                    pair.Value.AppendEbColValues();
                    gTable.Add(pair.Value);
                }
                return gTable;
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
            return null;
        }

        public string GetQuery(string parentTable = null)
        {
            List<string> colums = new List<string> { "eb_device_id", "eb_appversion", "eb_created_at_device", "eb_loc_id", "id" };
            colums.Add($"{parentTable}_id");
            foreach (var ctrl in ChildControls)
                colums.Add(ctrl.Name);

            colums.Reverse();
            return string.Join(",", colums.ToArray());
        }

        public EbDataTable GetLocalData(string parentTable, int rowid)
        {
            EbDataTable dt;
            try
            {
                dt = App.DataDB.DoQuery(string.Format(StaticQueries.STARFROM_TABLE_WDEP, this.GetQuery(parentTable), this.TableName, $"{parentTable}_id", rowid));
            }
            catch (Exception ex)
            {
                dt = new EbDataTable();
                Console.WriteLine(ex.Message);
            }
            return dt;
        }

        public DbTypedValue GetDbType(string name, object value, EbDbTypes type)
        {
            DbTypedValue TV = new DbTypedValue(name, value, type);
            EbMobileControl ctrl = ChildControls.Find(item => item.Name == name);
            if (ctrl != null)
            {
                TV.Type = ctrl.EbDbType;
                TV.Value = ctrl.SQLiteToActual(value);
            }
            else
            {
                if (type == EbDbTypes.Date)
                    TV.Value = Convert.ToDateTime(value).ToString("yyyy-MM-dd");
                else if (type == EbDbTypes.DateTime)
                    TV.Value = Convert.ToDateTime(value).ToString("yyyy-MM-dd HH:mm:ss");
            }
            return TV;
        }

        public List<SingleColumn> GetColumnValues(ColumnColletion columns, EbDataRow row)
        {
            List<SingleColumn> SC = new List<SingleColumn>();
            for (int i = 0; i < row.Count; i++)
            {
                EbDataColumn column = columns.Find(o => o.ColumnIndex == i);
                if (column != null && column.ColumnName != "eb_loc_id" && column.ColumnName != "id")
                {
                    DbTypedValue DTV = this.GetDbType(column.ColumnName, row[i], column.Type);
                    SC.Add(new SingleColumn
                    {
                        Name = column.ColumnName,
                        Type = (int)DTV.Type,
                        Value = DTV.Value
                    });
                }
            }
            return SC;
        }

        public void AutoFill()
        {
            try
            {
                EbDataTable dt = null;
                if (this.NetworkType == NetworkMode.Online)
                {
                    if (string.IsNullOrEmpty(this.DataSourceRefId))
                        return;
                    VisualizationLiveData data = DataService.Instance.GetData(this.DataSourceRefId, null, null, 0, 0);
                    if (data.Data != null && data.Data.Tables.Count >= 2)
                        dt = data.Data.Tables[1];
                }
                else if (this.NetworkType == NetworkMode.Offline)
                {
                    if (string.IsNullOrEmpty(this.OfflineQuery.Code))
                        return;
                    dt = App.DataDB.DoQuery(HelperFunctions.B64ToString(this.OfflineQuery.Code));
                }
                else
                {
                    if (Utils.HasInternet && !string.IsNullOrEmpty(this.DataSourceRefId))
                    {
                        VisualizationLiveData data = DataService.Instance.GetData(this.DataSourceRefId, null, null, 0, 0);
                        if (data.Data != null && data.Data.Tables.Count >= 2)
                            dt = data.Data.Tables[1];
                    }
                    else if (!string.IsNullOrEmpty(this.OfflineQuery.Code))
                    {
                        dt = App.DataDB.DoQuery(HelperFunctions.B64ToString(this.OfflineQuery.Code));
                    }
                    else
                        return;
                }

                if (dt != null)
                {
                    foreach (var row in dt.Rows)
                        CreateAutoFillRow(row);
                    gridBody.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
        }

        public MobileTableRow CreateAutoFillRow(EbDataRow row)
        {
            MobileTableRow table_row = new MobileTableRow();
            try
            {
                foreach (EbMobileControl ctrl in this.ChildControls)
                {
                    if (ctrl is INonPersistControl || ctrl is ILinesEnabled)
                        continue;
                    table_row.Columns.Add(new MobileTableColumn
                    {
                        Name = ctrl.Name,
                        Type = ctrl.EbDbType,
                        Value = row[ctrl.Name] ?? null
                    });
                }
                Grid grid = this.CreateDataRow(table_row);
                gridBody.Children.Add(grid);
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
            return table_row;
        }

        public override void SetValue(object value)
        {
            try
            {
                EbDataTable dt = value == null ? new EbDataTable() : (value as EbDataTable);

                foreach (var row in dt.Rows)
                {
                    MobileTableRow mobileRow = this.CreateAutoFillRow(row);
                    int id = Convert.ToInt32(row["id"]);
                    mobileRow.RowId = id;
                }
                gridBody.IsVisible = true;
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
        }

        public override void SetAsReadOnly(bool enable)
        {
            try
            {
                if (enable)
                    container.IsEnabled = false;
                else
                    container.IsEnabled = true;
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
        }
    }
}
