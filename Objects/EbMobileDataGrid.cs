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

        public FormMode Mode { set; get; }

        private Frame Container { set; get; }

        private StackLayout GridHeader { set; get; }

        private StackLayout GridBody { set; get; }

        private StackLayout GridFooter { set; get; }

        private TapGestureRecognizer TapRecognizer { set; get; }

        private readonly Style ButtonStyles = new Style(typeof(Button))
        {
            Setters =
            {
                new Setter{ Property = Button.VerticalOptionsProperty,Value = LayoutOptions.Center },
                new Setter{ Property = Button.PaddingProperty,Value = 0 },
                new Setter{ Property = Button.HeightRequestProperty,Value = 30 },
                new Setter{ Property = Button.WidthRequestProperty,Value = 30 },
                new Setter{ Property = Button.CornerRadiusProperty,Value = 4 },
                new Setter
                {
                    Property = Button.FontFamilyProperty,
                    Value = (OnPlatform<string>)HelperFunctions.GetResourceValue("FontAwesome")
                },
            }
        };

        private Dictionary<string, MobileTableRow> DataDictionary { set; get; }

        public EbMobileDataGrid()
        {
            Container = new Frame
            {
                BorderColor = Color.FromHex("cccccc"),
                CornerRadius = 4,
                HasShadow = false,
                Padding = 1
            };

            GridHeader = new StackLayout { BackgroundColor = Color.FromHex("eeeeee") };
            GridBody = new StackLayout { IsVisible = false };
            GridFooter = new StackLayout { IsVisible = false };
            DataDictionary = new Dictionary<string, MobileTableRow>();

            StackLayout stackL = new StackLayout
            {
                Spacing = 0,
                Children = { GridHeader, GridBody, GridFooter }
            };
            Container.Content = stackL;
            TapRecognizer = new TapGestureRecognizer();
            TapRecognizer.Tapped += TapRecognizer_Tapped;
        }

        private Grid CreateGridLayout(string name = null)
        {
            return new Grid
            {
                ClassId = name ?? Guid.NewGuid().ToString("N"),
                VerticalOptions = LayoutOptions.Center,
                Padding = new Thickness(5, 5),
                ColumnDefinitions =
                {
                    new ColumnDefinition{ Width=GridLength.Star },
                    new ColumnDefinition{ Width=GridLength.Auto }
                }
            };
        }

        public override void InitXControl(FormMode mode)
        {
            this.Mode = mode;
            this.CreateHeader();//creating grid header
            this.CreateFooter();//creating grid footer
            this.XControl = Container;
            this.AutoFill();
        }

        private void CreateHeader()
        {
            Grid grid = CreateGridLayout();//creating new grid
            Button addRowBtn = new Button
            {
                Text = "\uf055",
                Style = ButtonStyles,
                TextColor = Color.White,
                BackgroundColor = Color.FromHex("#33b762")
            };
            addRowBtn.Clicked += AddRowBtn_Clicked;
            grid.Children.Add(addRowBtn, 1, 0);
            CustomFrame frame = new CustomFrame(this.GetTableRow(true), DataLayout.CellCollection, DataLayout.RowCount, DataLayout.ColumCount, true)
            {
                BackgroundColor = Color.Transparent,
                Padding = 0
            };
            grid.Children.Add(frame, 0, 0);
            GridHeader.Children.Add(grid);
        }

        private Grid CreateRow(string name = null)
        {
            Grid grid = CreateGridLayout(name);
            try
            {
                Button rowOptions = new Button
                {
                    ClassId = grid.ClassId,
                    Text = "\uf014",
                    Style = ButtonStyles,
                    BackgroundColor = Color.Transparent
                };
                rowOptions.Clicked += RowDelete_Clicked;
                grid.Children.Add(rowOptions, 1, 0);
                MobileTableRow row = this.GetTableRow();

                grid.Children.Add(new CustomFrame(row, DataLayout.CellCollection, DataLayout.RowCount, DataLayout.ColumCount)
                {
                    ClassId = grid.ClassId,
                    BackgroundColor = Color.Transparent,
                    Padding = 0,
                    GestureRecognizers = { this.TapRecognizer }
                }, 0, 0);

                DataDictionary[grid.ClassId] = row;
            }
            catch(Exception ex)
            {
                Log.Write(ex.Message);
            }   
            return grid;
        }

        private void CreateFooter()
        {
            Grid grid = CreateGridLayout();
            grid.Children.Add(new CustomFrame(this.GetTableRow(true), DataLayout.CellCollection, DataLayout.RowCount, DataLayout.ColumCount)
            {
                BackgroundColor = Color.Transparent,
                Padding = 0
            }, 0, 0);
            GridFooter.Children.Add(grid);
        }

        private MobileTableRow GetTableRow(bool isHeader = false)
        {
            MobileTableRow row = new MobileTableRow();
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
            return row;
        }

        private void AddRowBtn_Clicked(object sender, EventArgs e)
        {
            DataGridView gridview = new DataGridView(this);
            (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushModalAsync(gridview);
        }

        private void RowDelete_Clicked(object sender, EventArgs e)
        {
            Button button = sender as Button;
            foreach (View el in GridBody.Children)
            {
                if (el.ClassId == button.ClassId)
                {
                    GridBody.Children.Remove(el);
                    DataDictionary.Remove(el.ClassId);
                    break;
                }
            }
        }

        public void RowAddCallBack(string name = null)
        {
            if (name == null)
            {
                Grid grid = this.CreateRow();
                GridBody.Children.Add(grid);
            }
            else
            {
                MobileTableRow row = this.GetTableRow();
                DataDictionary[name] = row;

                for (int i = 0; i < GridBody.Children.Count; i++)
                {
                    if (GridBody.Children[i].ClassId == name)
                    {
                        GridBody.Children.Remove(GridBody.Children[i]);
                        Grid ig = this.CreateRow(name);
                        GridBody.Children.Insert(i, ig);
                        break;
                    }
                }
            }
            if (GridBody.Children.Count > 0)
                GridBody.IsVisible = true;
        }

        private void TapRecognizer_Tapped(object sender, EventArgs e)
        {
            string classId = (sender as CustomFrame).ClassId;
            DataGridView gridview = new DataGridView(this, DataDictionary[classId], classId);
            (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushModalAsync(gridview);
        }

        public override object GetValue()
        {
            try
            {
                MobileTable gTable = new MobileTable(this.TableName);
                foreach (KeyValuePair<string, MobileTableRow> pair in DataDictionary)
                {
                    pair.Value.AppendEbColValues();
                    gTable.Add(pair.Value);
                }
                return gTable;
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
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
                    VisualizationLiveData data = RestServices.Instance.PullReaderData(this.DataSourceRefId, null, 0, 0);
                    if (data.Data != null && data.Data.Tables.Count >= 2)
                        dt = data.Data.Tables[1];
                }
                else
                {
                    string query = HelperFunctions.B64ToString(this.OfflineQuery.Code);
                    dt = App.DataDB.DoQuery(query);
                }

                if (dt != null)
                {
                    foreach (var row in dt.Rows)
                        CreateAutoFillRow(row);
                    GridBody.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }

        public void CreateAutoFillRow(EbDataRow row)
        {
            try
            {
                Grid grid = CreateGridLayout(null);
                Button rowOptions = new Button { ClassId = grid.ClassId, Text = "\uf014", Style = ButtonStyles, BackgroundColor = Color.Transparent };
                rowOptions.Clicked += RowDelete_Clicked;
                grid.Children.Add(rowOptions, 1, 0);

                MobileTableRow table_row = new MobileTableRow();
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

                grid.Children.Add(new CustomFrame(table_row, DataLayout.CellCollection, DataLayout.RowCount, DataLayout.ColumCount)
                {
                    ClassId = grid.ClassId,
                    BackgroundColor = Color.Transparent,
                    Padding = 0,
                    GestureRecognizers = { this.TapRecognizer }
                }, 0, 0);

                DataDictionary[grid.ClassId] = table_row;
                GridBody.Children.Add(grid);
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }
    }
}
