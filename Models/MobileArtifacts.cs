using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Structures;
using System;
using System.Collections.Generic;
using System.IO;
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

        public string GetQuery(List<DbParameter> parameters, int masterRowId)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                foreach (MobileTable table in this.Tables)
                    sb.Append(table.GetQuery(this.MasterTable, parameters, masterRowId));
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

        public string GetQuery(string masterTable, List<DbParameter> parameters, int masterRowId = 0)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                for (int i = 0; i < this.Count; i++)
                {
                    if (this[i].IsUpdate && !this[i].IsDelete)//update
                    {
                        List<string> _colstrings = new List<string>();
                        foreach (MobileTableColumn col in this[i].Columns)
                        {
                            _colstrings.Add($"{col.Name} = @{this.TableName}_{col.Name}_{i}");

                            parameters.Add(new DbParameter
                            {
                                ParameterName = $"@{this.TableName}_{col.Name}_{i}",
                                DbType = (int)col.Type,
                                Value = col.Value
                            });
                        }
                        sb.AppendFormat("UPDATE {0} SET {1} WHERE id = {2};", this.TableName, string.Join(",", _colstrings), $"@{this.TableName}_rowid_{i}");

                        parameters.Add(new DbParameter
                        {
                            ParameterName = $"@{this.TableName}_rowid_{i}",
                            DbType = (int)EbDbTypes.Int32,
                            Value = this[i].RowId
                        });
                    }
                    else if (this[i].IsDelete)//delete
                    {
                        sb.AppendLine($"DELETE FROM {this.TableName} WHERE id = @{this.TableName}_deleterow_{i};");
                        parameters.Add(new DbParameter
                        {
                            ParameterName = $"@{this.TableName}_deleterow_{i}",
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
                            string _prm = $"@{this.TableName}_{col.Name}_{i}";

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
                            _cols.Add($"{masterTable}_id");

                            if (masterRowId > 0)
                            {
                                _vals.Add($"@{masterTable}_id_{i}");

                                parameters.Add(new DbParameter
                                {
                                    ParameterName = $"@{masterTable}_id_{i}",
                                    DbType = (int)EbDbTypes.Int32,
                                    Value = masterRowId
                                });
                            }
                            else
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

        public bool IsDelete { set; get; }

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

    public class SolutionInfo
    {
        public string RootUrl { set; get; }

        public string SolutionName { set; get; }

        public ImageSource Logo { set; get; }

        public bool IsCurrent { set; get; }

        public Color StatusColor
        {
            get
            {
                if (IsCurrent)
                    return Color.FromHex("26bd26");
                else
                    return Color.Transparent;
            }
        }

        public void SetLogo()
        {
            INativeHelper helper = DependencyService.Get<INativeHelper>();
            try
            {
                var bytes = helper.GetPhoto($"ExpressBase/{this.SolutionName}/logo.png");

                if (bytes != null)
                    this.Logo = ImageSource.FromStream(() => new MemoryStream(bytes));
            }
            catch (Exception ex)
            {
                Log.Write("Login_SetLogo" + ex.Message);
            }
        }
    }

    public class FUPSetValueMeta
    {
        public string TableName { set; get; }

        public int RowId { set; get; }
    }
}
