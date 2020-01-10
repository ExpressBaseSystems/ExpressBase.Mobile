using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressBase.Mobile
{
    public class EbMobileForm : EbMobileContainer
    {
        public override string Name { set; get; }

        public List<EbMobileControl> ChiledControls { get; set; }

        public List<EbMobileControl> FlatControls { set; get; }

        public string TableName { set; get; }

        public bool AutoDeployMV { set; get; }

        public string AutoGenMVRefid { set; get; }

        public string WebFormRefId { set; get; }

        //mobile prop
        private FormMode Mode { set; get; }

        //ref mode prop
        private EbDataRow ParentRow;

        private string ParentTable;

        private bool HasFileSelect
        {
            get
            {
                return FlatControls.Any(x => x.GetType() == typeof(EbMobileFileUpload));
            }
        }

        public string SelectQuery
        {
            get
            {
                List<string> colums = new List<string> { "eb_device_id", "eb_appversion", "eb_created_at_device", "eb_loc_id", "id" };

                foreach (EbMobileControl ctrl in ChiledControls)
                {
                    if (ctrl is EbMobileTableLayout)
                    {
                        foreach (EbMobileTableCell cell in (ctrl as EbMobileTableLayout).CellCollection)
                        {
                            foreach (EbMobileControl tctrl in cell.ControlCollection)
                            {
                                colums.Add(tctrl.Name);
                            }
                        }
                    }
                    else
                        colums.Add(ctrl.Name);
                }
                colums.Reverse();
                return string.Join(",", colums.ToArray());
            }
        }

        public EbMobileForm()
        {
            FlatControls = new List<EbMobileControl>();
        }

        public DbTypedValue GetDbType(string name, object value, EbDbTypes type)
        {
            DbTypedValue TV = new DbTypedValue { Type = type, Value = (type == EbDbTypes.DateTime || type == EbDbTypes.Date) ? value.ToString() : value };

            foreach (EbMobileControl ctrl in ChiledControls)
            {
                if (!(ctrl is EbMobileTableLayout) && ctrl.Name == name)
                {
                    TV.Type = ctrl.EbDbType;
                    TV.Value = ctrl.SQLiteToActual(value);
                }
                else if (ctrl is EbMobileTableLayout)
                {
                    foreach (EbMobileTableCell cell in (ctrl as EbMobileTableLayout).CellCollection)
                    {
                        foreach (EbMobileControl tctrl in cell.ControlCollection)
                        {
                            if (ctrl.Name == name)
                            {
                                TV.Type = ctrl.EbDbType;
                                TV.Value = ctrl.SQLiteToActual(value);
                                return TV;
                            }
                        }
                    }
                }
            }
            return TV;
        }

        public void PushRecords()
        {
            try
            {
                EbDataTable dt = App.DataDB.DoQuery(string.Format(StaticQueries.STARFROM_TABLE, this.SelectQuery, this.TableName));
                WebformData FormData = new WebformData { MasterTable = this.TableName };
                //start pushing
                this.InitPush(FormData, dt);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void InitPush(WebformData WebFormData, EbDataTable LocalData)
        {
            SingleTable SingleTable = new SingleTable();
            try
            {
                SingleRow row = new SingleRow { RowId = "0", IsUpdate = false };
                SingleTable.Add(row);

                for (int i = 0; i < LocalData.Rows.Count; i++)
                {
                    row.Columns.Clear();
                    WebFormData.MultipleTables.Clear();
                    int rowid = Convert.ToInt32(LocalData.Rows[i]["id"]);

                    row.LocId = Convert.ToInt32(LocalData.Rows[i]["eb_loc_id"]);
                    row.Columns.AddRange(this.GetColumnValues(LocalData, i));
                    WebFormData.MultipleTables.Add(this.TableName, SingleTable);

                    PushResponse response = Api.Push(WebFormData, 0, this.WebFormRefId, row.LocId);
                    this.FlagLocalRow(response, rowid, this.TableName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void FlagLocalRow(PushResponse Response, int RowId, string TableName)
        {
            try
            {
                if (Response.RowAffected > 0)
                {
                    DbParameter[] parameter = new DbParameter[]
                    {
                        new DbParameter{ParameterName="@rowid",Value = RowId},
                        new DbParameter{ParameterName="@cloudrowid",Value = Response.RowId}
                    };

                    int rowAffected = App.DataDB.DoNonQuery(string.Format(StaticQueries.FLAG_LOCALROW_SYNCED, TableName), parameter);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private List<SingleColumn> GetColumnValues(EbDataTable LocalData, int RowIndex)
        {
            List<SingleColumn> SC = new List<SingleColumn>();

            for (int i = 0; i < LocalData.Rows[RowIndex].Count; i++)
            {
                EbDataColumn column = LocalData.Columns.Find(o => o.ColumnIndex == i);

                if (column != null && column.ColumnName != "eb_loc_id" && column.ColumnName != "id")
                {
                    DbTypedValue DTV = this.GetDbType(column.ColumnName, LocalData.Rows[RowIndex][i], column.Type);
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

        public void CreateTableSchema()
        {
            SQLiteTableSchema Schema = new SQLiteTableSchema() { TableName = this.TableName };

            foreach (EbMobileControl ctrl in this.FlatControls)
            {
                if (ctrl is EbMobileFileUpload)
                    continue;
                else
                {
                    Schema.Columns.Add(new SQLiteColumSchema
                    {
                        ColumnName = ctrl.Name,
                        ColumnType = ctrl.SQLiteType
                    });
                }
            }
            Schema.AppendDefault();
            CommonServices.Instance.CreateLocalTable(Schema);
        }

        public bool Save(int RowId)
        {
            MobileFormData data = this.GetFormData(RowId);
            string query = string.Empty;
            try
            {
                if (data.Tables.Count > 0)
                {
                    List<DbParameter> _params = new List<DbParameter>();
                    foreach (MobileTable _table in data.Tables)
                    {
                        query += HelperFunctions.GetQuery(_table, _params);
                    }
                    int rowAffected = App.DataDB.DoNonQuery(query, _params.ToArray());

                    if (this.HasFileSelect)
                    {
                        object lastRowId = (RowId != 0) ? RowId : App.DataDB.DoScalar(string.Format(StaticQueries.CURRVAL, this.TableName));
                        this.PushFiles(lastRowId);
                    }
                    return (rowAffected > 0);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return false;
        }

        public bool SaveWithParentId(EbDataRow _ParentRow, string ParentTableName)
        {
            this.Mode = FormMode.REF;
            this.ParentRow = _ParentRow;
            this.ParentTable = ParentTableName;

            MobileFormData data = this.GetFormData(0);
            string query = string.Empty;
            try
            {
                if (data.Tables.Count > 0)
                {
                    List<DbParameter> _params = new List<DbParameter>();
                    foreach (MobileTable _table in data.Tables)
                    {
                        query += HelperFunctions.GetQuery(_table, _params);
                    }
                    int rowAffected = App.DataDB.DoNonQuery(query, _params.ToArray());

                    if (this.HasFileSelect)
                    {
                        object lastRowId = App.DataDB.DoScalar(string.Format(StaticQueries.CURRVAL, this.TableName));
                        this.PushFiles(lastRowId);
                    }
                    return (rowAffected > 0);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return false;
        }

        private MobileFormData GetFormData(int RowId)
        {
            MobileFormData FormData = new MobileFormData
            {
                MasterTable = this.TableName
            };
            MobileTable Table = new MobileTable { TableName = this.TableName };
            MobileTableRow row = new MobileTableRow
            {
                RowId = RowId,
                IsUpdate = (RowId > 0)
            };

            Table.Add(row);
            foreach (EbMobileControl Ctrl in this.FlatControls)
            {
                MobileTableColumn Column = Ctrl.GetMobileTableColumn();

                if (Column != null)
                {
                    row.Columns.Add(Column);
                }
            }

            if (RowId <= 0)
                row.AppendEbColValues();//append ebcol values

            if (this.Mode == FormMode.REF)
            {
                row.Columns.Add(new MobileTableColumn
                {
                    Name = this.ParentTable + "_id",
                    Type = EbDbTypes.Int32,
                    Value = this.ParentRow["id"]
                });
            }

            FormData.Tables.Add(Table);
            return FormData;
        }

        private void PushFiles(object LastRowId)
        {
            int rowid = Convert.ToInt32(LastRowId);

            Task.Run(() =>
            {
                foreach (EbMobileControl ctrl in this.FlatControls)
                {
                    if (ctrl is EbMobileFileUpload)
                    {
                        (ctrl as EbMobileFileUpload).PushFilesToDir(this.TableName, rowid);
                    }
                }
            });
        }
    }
}
