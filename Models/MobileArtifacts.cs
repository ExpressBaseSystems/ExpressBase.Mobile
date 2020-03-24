using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Models
{
    public interface INonPersistControl { }

    public interface ILinesEnabled
    {
        string TableName { set; get; }

        List<EbMobileControl> ChildControls { set; get; }

        EbDataTable GetLocalData(string parentTable, int rowid);

        string GetQuery(string parentTable);

        List<SingleColumn> GetColumnValues(ColumnColletion columns, EbDataRow row);
    }

    public class MobileFormData
    {
        public string MasterTable { set; get; }

        public int LocationId { set; get; }

        public List<MobileTable> Tables { set; get; }

        public MobileFormData()
        {
            Tables = new List<MobileTable>();
        }

        public MobileFormData(string masterTableName)
        {
            MasterTable = masterTableName;
            Tables = new List<MobileTable>();
        }

        public WebformData ToWebFormData()
        {
            WebformData wd = new WebformData(this.MasterTable);

            foreach (MobileTable table in this.Tables)
            {
                wd.MultipleTables.Add(table.TableName, new SingleTable());

                foreach (var row in table)
                {
                    var srow = new SingleRow
                    {
                        RowId = row.RowId,
                        LocId = Settings.LocationId,
                        IsUpdate = (row.RowId > 0)
                    };

                    foreach (var col in row.Columns)
                        srow.Columns.Add(new SingleColumn
                        {
                            Name = col.Name,
                            Value = col.Value,
                            Type = (int)col.Type
                        });

                    wd.MultipleTables[table.TableName].Add(srow);
                }
            }
            return wd;
        }

        public void SortByMaster()
        {
            var idx = this.Tables.FindIndex(x => x.TableName == this.MasterTable);
            var item = this.Tables[idx];
            this.Tables[idx] = this.Tables[0];
            this.Tables[0] = item;
        }

        public string GetQuery(List<DbParameter> parameters)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                foreach (MobileTable table in this.Tables)
                    sb.Append(table.GetQuery(this.MasterTable, parameters));
            }
            catch (Exception ex)
            {
                Log.Write("MobileFormData.GetQuery---" + ex.Message);
            }
            return sb.ToString();
        }
    }

    public class MobileTable : List<MobileTableRow>
    {
        public string TableName { set; get; }

        public Dictionary<string, List<FileWrapper>> Files { set; get; }

        public MobileTable()
        {
            Files = new Dictionary<string, List<FileWrapper>>();
        }

        public MobileTable(string tableName)
        {
            TableName = tableName;
            Files = new Dictionary<string, List<FileWrapper>>();
        }

        public string GetQuery(string masterTable, List<DbParameter> parameters)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                for (int i = 0; i < this.Count; i++)
                {
                    if (this[i].IsUpdate)//update
                    {
                        List<string> _colstrings = new List<string>();
                        foreach (MobileTableColumn col in this[i].Columns)
                        {
                            _colstrings.Add(string.Format("{0} = @{1}_{2}", col.Name, col.Name, i));

                            parameters.Add(new DbParameter
                            {
                                ParameterName = string.Format("@{0}_{1}", col.Name, i),
                                DbType = (int)col.Type,
                                Value = col.Value
                            });
                        }
                        sb.AppendFormat("UPDATE {0} SET {1} WHERE id = {2};", this.TableName, string.Join(",", _colstrings), ("@rowid" + i));

                        parameters.Add(new DbParameter
                        {
                            ParameterName = ("@rowid" + i),
                            DbType = (int)EbDbTypes.Int32,
                            Value = this[i].RowId
                        });
                    }
                    else//insert
                    {
                        List<string> _cols = (this.Count > 0) ? this[i].Columns.Select(en => en.Name).ToList() : new List<string>();
                        List<string> _vals = new List<string>();
                        foreach (MobileTableColumn col in this[i].Columns)
                        {
                            string _prm = string.Format("@{0}_{1}", col.Name, i);

                            _vals.Add(_prm);

                            parameters.Add(new DbParameter
                            {
                                ParameterName = _prm,
                                DbType = (int)col.Type,
                                Value = col.Value
                            });
                        }

                        if (this.TableName != masterTable)
                        {
                            _cols.Add(masterTable + "_id");
                            _vals.Add($"(SELECT MAX(id) FROM {masterTable})");
                        }
                        sb.AppendFormat("INSERT INTO {0}({1}) VALUES ({2});", this.TableName, string.Join(",", _cols), string.Join(",", _vals.ToArray()));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
            return sb.ToString();
        }
    }

    public class MobileTableRow
    {
        public int RowId { set; get; }

        public bool IsUpdate { get { return (RowId > 0); } }

        public List<MobileTableColumn> Columns { set; get; }

        public MobileTableRow()
        {
            Columns = new List<MobileTableColumn>();
        }

        public MobileTableRow(int rowId)
        {
            RowId = rowId;
            Columns = new List<MobileTableColumn>();
        }

        public void AppendEbColValues()
        {
            this.Columns.Add(new MobileTableColumn { Name = "eb_loc_id", Type = EbDbTypes.Int32, Value = Settings.LocationId });
            this.Columns.Add(new MobileTableColumn { Name = "eb_created_at_device", Type = EbDbTypes.DateTime, Value = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") });

            INativeHelper helper = DependencyService.Get<INativeHelper>();

            this.Columns.Add(new MobileTableColumn { Name = "eb_device_id", Type = EbDbTypes.String, Value = helper.DeviceId });
            //<manufacturer>(<model> <platform>:<osversion>)-<appversion>
            string appversion = string.Format("{0}({1} {2}:{3})-{4}", DeviceInfo.Manufacturer, DeviceInfo.Model, DeviceInfo.Platform, DeviceInfo.VersionString, helper.AppVersion);
            this.Columns.Add(new MobileTableColumn { Name = "eb_appversion", Type = EbDbTypes.String, Value = appversion });
        }

        public MobileTableColumn this[string columnname]
        {
            get
            {
                return this.Columns.Find(item => item.Name == columnname) ?? null;
            }
        }
    }

    public class MobileTableColumn
    {
        public string Name { get; set; }

        public object Value { get; set; }

        public EbDbTypes Type { set; get; }

        public EbMobileControl Control { set; get; }

        public MobileTableColumn() { }

        public MobileTableColumn(string name, EbDbTypes type, object value)
        {
            Name = name;
            Type = type;
            Value = value;
        }
    }

    public class FileWrapper
    {
        public string Name { set; get; }

        public string FileName { set; get; }

        public byte[] Bytea { set; get; }

        public string ControlName { set; get; }

        public int FileRefId { set; get; }
    }

    public class FormSaveResponse
    {
        public bool Status { set; get; }

        public string Message { set; get; }
    }

    public class SyncResponse
    {
        public bool Status { set; get; }

        public string Message { set; get; }
    }
}
