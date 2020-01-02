using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Structures;
using System;
using System.Collections.Generic;

namespace ExpressBase.Mobile
{
    public class DbTypedValue
    {
        public EbDbTypes Type { set; get; }

        public object Value { set; get; }
    }

    public class EbMobileContainer : EbMobilePageBase
    {

    }

    public class EbMobileForm : EbMobileContainer
    {
        public override string Name { set; get; }

        public List<EbMobileControl> ChiledControls { get; set; }

        public string TableName { set; get; }

        public bool AutoDeployMV { set; get; }

        public string AutoGenMVRefid { set; get; }

        public string WebFormRefId { set; get; }

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
                                colums.Add(ctrl.Name);
                            }
                        }
                    }
                    else
                    {
                        colums.Add(ctrl.Name);
                    }
                }
                colums.Reverse();
                return string.Join(",", colums.ToArray());
            }
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

        private void UpdateRowFlag()
        {

        }
    }

    public class EbMobileVisualization : EbMobileContainer
    {
        public string DataSourceRefId { set; get; }

        public string SourceFormRefId { set; get; }

        public EbScript OfflineQuery { set; get; }

        public EbMobileTableLayout DataLayout { set; get; }

        public string LinkRefId { get; set; }
    }
}
