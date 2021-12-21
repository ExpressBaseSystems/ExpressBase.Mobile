using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Views.Shared;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ExpressBase.Mobile.CustomControls.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DataGrid : ContentView
    {
        private readonly Dictionary<string, MobileTableRow> dataDictionary;

        private readonly TapGestureRecognizer tapRecognizer;

        public DataGrid()
        {
            InitializeComponent();
        }

        private readonly EbMobileDataGrid dataGrid;

        public DataGrid(EbMobileDataGrid dg)
        {
            InitializeComponent();

            dataGrid = dg;
            dataDictionary = new Dictionary<string, MobileTableRow>();
            tapRecognizer = new TapGestureRecognizer();
            if (!dataGrid.DisableEdit)
                tapRecognizer.Tapped += OpenGridFormOnEdit;

            DrawHeader();

            if (!string.IsNullOrEmpty(dataGrid.DataSourceRefId) && dataGrid.FormRenderMode != FormMode.EDIT)
            {
                AutoFill(dataGrid.DataSourceRefId);
            }
        }

        private void DrawHeader()
        {
            DGDynamicFrame frame = new DGDynamicFrame(this.dataGrid.GetControlValues(true), dataGrid.DataLayout, true)
            {
                BackgroundColor = Color.Transparent,
                Margin = new Thickness(5, 5, 0, 0),
                Padding = 0
            };
            Container.Children.Add(frame, 0, 0);

            if (dataGrid.DisableAdd)
                Container.Children.Remove(AddRowButton);
        }

        private void AddRowButtonClicked(object sender, EventArgs e)
        {
            DataGridForm gridview = new DataGridForm(dataGrid);
            gridview.OnInserted += (name) =>
            {
                this.OnRowInserted(name);
            };
            App.Navigation.NavigateMasterModalAsync(gridview);
        }

        public void OnRowInserted(string name = null)
        {
            try
            {
                MobileTableRow values = this.dataGrid.GetControlValues();
                if (name == null)
                {
                    var grid = GetRow(values);
                    Body.Children.Add(grid);
                }
                else
                {
                    MobileTableRow row = this.dataGrid.GetControlValues();
                    MobileTableRow oldRow = dataDictionary[name];
                    row.RowId = oldRow.RowId;
                    dataDictionary[name] = row;

                    for (int i = 0; i < Body.Children.Count; i++)
                    {
                        if (Body.Children[i].ClassId == name)
                        {
                            Body.Children.Remove(Body.Children[i]);
                            var ig = GetRow(row, name);
                            Body.Children.Insert(i, ig);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
        }

        private View GetRow(MobileTableRow row, string name = null)
        {
            string guid = name ?? Guid.NewGuid().ToString("N");

            StackLayout stack = new StackLayout
            {
                ClassId = guid,
                Orientation = Xamarin.Forms.StackOrientation.Horizontal,
                Padding = new Thickness(5, 5, 0, 5),
                BackgroundColor = Color.White,
            };

            DGDynamicFrame dynamicFrame = new DGDynamicFrame(row, dataGrid.DataLayout)
            {
                ClassId = guid,
                BackgroundColor = Color.Transparent,
                Padding = 0,
                GestureRecognizers = { this.tapRecognizer },
                HorizontalOptions = LayoutOptions.FillAndExpand
            };

            stack.Children.Add(dynamicFrame);

            if (!dataGrid.DisableDelete)
            {
                Button rowOptions = new Button
                {
                    ClassId = guid,
                    Style = (Style)HelperFunctions.GetResourceValue("DGEditRowButton")
                };
                rowOptions.Clicked += RowDelete_Clicked;
                stack.Children.Add(rowOptions);
            }
            if (name == null) dataDictionary[guid] = row;

            return stack;
        }

        private void RowDelete_Clicked(object sender, EventArgs e)
        {
            Button button = sender as Button;

            foreach (View el in Body.Children)
            {
                if (el.ClassId == button.ClassId)
                {
                    Body.Children.Remove(el);
                    if (this.dataGrid.FormRenderMode == FormMode.EDIT)
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

        private async void OpenGridFormOnEdit(object sender, EventArgs e)
        {
            string classId = (sender as DynamicFrame).ClassId;

            DataGridForm gridview = new DataGridForm(dataGrid, dataDictionary[classId], classId);

            gridview.OnInserted += (name) =>
            {
                this.OnRowInserted(name);
            };
            await App.Navigation.NavigateMasterModalAsync(gridview);
        }

        public void SetValue(EbDataTable dataTable)
        {
            foreach (EbDataRow row in dataTable.Rows)
            {
                MobileTableRow tableRow = DR2TableRow(row);
                var rowView = GetRow(tableRow);
                Body.Children.Add(rowView);
            }
        }

        private MobileTableRow DR2TableRow(EbDataRow row)
        {
            int dataId = row["id"] != null ? Convert.ToInt32(row["id"]) : 0;

            MobileTableRow tableRow = new MobileTableRow(dataId);

            foreach (EbMobileControl ctrl in dataGrid.ChildControls)
            {
                if (ctrl is INonPersistControl || ctrl is ILinesEnabled)
                    continue;

                MobileTableColumn column = new MobileTableColumn
                {
                    Name = ctrl.Name,
                    Type = ctrl.EbDbType,
                    Value = row[ctrl.Name] ?? null
                };

                if (column.Value != null && ctrl is EbMobileSimpleSelect select && !select.IsSimpleSelect)
                {
                    column.DisplayValue = select.GetDisplayName4DG(column.Value.ToString());
                }
                tableRow.Columns.Add(column);
            }
            return tableRow;
        }

        public void SetAsReadOnly(bool flag)
        {
            ReadOnlyMask.IsVisible = flag;
        }

        public MobileTable GetValue()
        {
            MobileTable mobileTable = new MobileTable(dataGrid.TableName);

            foreach (MobileTableRow row in dataDictionary.Values)
            {
                if (row.RowId <= 0) row.AppendEbColValues();
                mobileTable.Add(row);
            }
            return mobileTable;
        }

        public async void AutoFill(string dataSourceRefId)
        {
            try
            {
                MobileDataResponse data = await DataService.Instance.GetDataAsync(dataSourceRefId, 0, 0, dataGrid.Context?.ConvertToParams(), null, null, false, true);

                if (data != null && data.Data != null && data.Data.Tables.HasLength(2))
                {
                    SetValue(data.Data.Tables[1]);
                }
            }
            catch (Exception ex)
            {
                EbLog.Error("DataGrid autofill api error : " + ex.Message);
            }
        }
    }
}