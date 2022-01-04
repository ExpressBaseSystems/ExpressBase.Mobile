using ExpressBase.Mobile.CustomControls.Views;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Helpers.Script;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public bool DisableAdd { set; get; }

        public bool DisableDelete { set; get; }

        public bool DisableEdit { set; get; }

        public EbDataRow Context { set; get; }

        public EbScript RowColorExpr { get; set; }

        private DataGrid gridView;

        private bool isTaped { get; set; }

        public DataGridRowHelper RowHelper { get; set; }

        public override View Draw(FormMode mode, NetworkMode network)
        {
            FormRenderMode = mode;
            NetworkType = network;
            RowHelper = new DataGridRowHelper(this);

            gridView = new DataGrid(this);
            XControl = gridView;

            return base.Draw();
        }

        public override View Draw(FormMode mode, NetworkMode network, EbDataRow context)
        {
            Context = context;
            return Draw(mode, network);
        }

        public MobileTableRow GetControlValues(bool isHeader = false)
        {
            MobileTableRow row = new MobileTableRow();
            try
            {
                foreach (EbMobileControl ctrl in ChildControls)
                {
                    if (ctrl is INonPersistControl || ctrl is ILinesEnabled)
                        continue;

                    MobileTableColumn column = new MobileTableColumn
                    {
                        Name = ctrl.Name,
                        Type = ctrl.EbDbType,
                        Value = isHeader ? ctrl.Label : ctrl.GetValue()
                    };

                    if (ctrl is EbMobileSimpleSelect ps)
                    {
                        column.DisplayValue = ps.GetDisplayValue();
                    }
                    row.Columns.Add(column);
                }
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
            return row;
        }

        public override object GetValue()
        {
            return gridView.GetValue();
        }

        public string GetQuery(string parentTable = null)
        {
            List<string> colums = new List<string> {
                "eb_device_id",
                "eb_appversion",
                "eb_created_at_device",
                "eb_loc_id",
                "id"
            };

            colums.Add($"{parentTable}_id");

            foreach (EbMobileControl ctrl in ChildControls)
            {
                colums.Add(ctrl.Name);
            }

            colums.Reverse();
            return string.Join(",", colums.ToArray());
        }

        public EbDataTable GetLocalData(string parentTable, int rowid)
        {
            EbDataTable dt;
            try
            {
                string query = string.Format(StaticQueries.STARFROM_TABLE_WDEP, GetQuery(parentTable), TableName, $"{parentTable}_id", rowid);
                dt = App.DataDB.DoQuery(query);
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
            List<SingleColumn> singleColumn = new List<SingleColumn>();

            for (int i = 0; i < row.Count; i++)
            {
                EbDataColumn column = columns.Find(o => o.ColumnIndex == i);

                if (column != null && column.ColumnName != "eb_loc_id" && column.ColumnName != "id")
                {
                    DbTypedValue DTV = GetDbType(column.ColumnName, row[i], column.Type);

                    singleColumn.Add(new SingleColumn
                    {
                        Name = column.ColumnName,
                        Type = (int)DTV.Type,
                        Value = DTV.Value
                    });
                }
            }
            return singleColumn;
        }

        public override void SetValue(object value)
        {
            if (value != null && value is EbDataTable dt)
            {
                gridView.SetValue(dt);
            }
        }

        public override void SetAsReadOnly(bool disable)
        {
            gridView?.SetAsReadOnly(disable);
        }

        public EbMobileControl GetControl(string controlName)
        {
            return this.ChildControls.Find(ctrl => ctrl.Name == controlName);
        }

        public decimal Sum(string controlName)
        {
            MobileTable gridTable = gridView.GetValue();

            List<decimal> values = gridTable.GetColumnValues<decimal>(controlName);

            return values.Sum();
        }

        public bool IsTaped()
        {
            if (this.isTaped)
                return true;
            this.isTaped = true;
            Task task = Task.Run(async () =>
            {
                await Task.Delay(1000);
                this.isTaped = false;
            });
            return false;
        }

    }

    public class DataGridRowHelper
    {
        private EbMobileDataGrid DataGrid;

        private string RowColorExprCode;

        private readonly EbSciptEvaluator evaluator;

        private Dictionary<string, MobileTableColumn> ColumnDictionary;

        public DataGridRowHelper(EbMobileDataGrid dataGrid)
        {
            if (!string.IsNullOrWhiteSpace(dataGrid?.RowColorExpr?.Code))
            {
                DataGrid = dataGrid;
                string script = dataGrid.RowColorExpr.GetCode();
                RowColorExprCode = GetComputedExpression(script);

                evaluator = new EbSciptEvaluator
                {
                    OptionScriptNeedSemicolonAtTheEndOfLastExpression = false
                };

                ColumnDictionary = new Dictionary<string, MobileTableColumn>();
                foreach (EbMobileControl ctrl in DataGrid.ChildControls)
                {
                    ColumnDictionary.Add(ctrl.Name, new MobileTableColumn(ctrl.Name, ctrl.EbDbType, null));
                }
                evaluator.SetVariable("form", new EbDataGridEvaluator(ColumnDictionary));
            }
        }

        public Color GetBackGroundColor(MobileTableRow row)
        {
            Color color = Color.White;
            if (RowColorExprCode != null)
            {
                try
                {
                    SetRowData(row);
                    string hexcolur = evaluator.Execute<string>(RowColorExprCode);
                    if (!string.IsNullOrWhiteSpace(hexcolur))
                        color = Color.FromHex(hexcolur);
                }
                catch (Exception ex)
                {

                }
            }
            return color;
        }

        private void SetRowData(MobileTableRow row)
        {
            foreach (MobileTableColumn Column in row.Columns)
            {
                if (ColumnDictionary.ContainsKey(Column.Name))
                {
                    if (Column.Type == EbDbTypes.Decimal)
                    {
                        decimal.TryParse(Column.Value?.ToString(), out decimal val);
                        ColumnDictionary[Column.Name].Value = val;
                    }
                    else
                    {
                        ColumnDictionary[Column.Name].Value = Column.Value;
                    }
                }
            }
        }

        private string GetComputedExpression(string expression)
        {
            foreach (EbMobileControl ctrl in DataGrid.ChildControls)
            {
                if (expression.Contains($"{DataGrid.Name}.currentRow[\"{ctrl.Name}\"]"))
                    expression = expression.Replace($"{DataGrid.Name}.currentRow[\"{ctrl.Name}\"]", $"GetTableColumn(\"{ctrl.Name}\")");
            }
            return expression;
        }
    }
}
