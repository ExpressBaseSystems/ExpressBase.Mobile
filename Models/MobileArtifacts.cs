using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Structures;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Models
{
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
                        LocId = App.Settings.CurrentLocId,
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
                foreach (MobileTable table in Tables)
                {
                    sb.Append(table.GetQuery(MasterTable, parameters, masterRowId));
                }
            }
            catch (Exception ex)
            {
                EbLog.Error("Error in [MobileFormData.GetQuery]" + ex.Message);
            }
            return sb.ToString();
        }

        public MobileTable CreateTable()
        {
            MobileTable table = new MobileTable(this.MasterTable);
            Tables.Add(table);
            return table;
        }
    }

    public class MobileTable : List<MobileTableRow>
    {
        public string TableName { set; get; }

        public List<FileWrapper> OldFiles { set; get; }

        public List<FileWrapper> NewFiles { set; get; }

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
                for (int i = 0; i < Count; i++)
                {
                    if (this[i].IsUpdate && !this[i].IsDelete)//update
                    {
                        List<string> _colstrings = new List<string>();

                        foreach (MobileTableColumn col in this[i].Columns)
                        {
                            _colstrings.Add($"{col.Name} = @{TableName}_{col.Name}_{i}");

                            parameters.Add(new DbParameter
                            {
                                ParameterName = $"@{TableName}_{col.Name}_{i}",
                                DbType = (int)col.Type,
                                Value = col.Value
                            });
                        }

                        sb.AppendFormat("UPDATE {0} SET {1} WHERE id = {2};", TableName, string.Join(",", _colstrings), $"@{TableName}_rowid_{i}");

                        parameters.Add(new DbParameter
                        {
                            ParameterName = $"@{TableName}_rowid_{i}",
                            DbType = (int)EbDbTypes.Int32,
                            Value = this[i].RowId
                        });
                    }
                    else if (this[i].IsDelete)//delete
                    {
                        sb.AppendLine($"DELETE FROM {TableName} WHERE id = @{TableName}_deleterow_{i};");

                        parameters.Add(new DbParameter
                        {
                            ParameterName = $"@{TableName}_deleterow_{i}",
                            DbType = (int)EbDbTypes.Int32,
                            Value = this[i].RowId
                        });
                    }
                    else//insert
                    {
                        List<string> _cols = (Count > 0) ? this[i].Columns.Select(en => en.Name).ToList() : new List<string>();

                        List<string> _vals = new List<string>();

                        foreach (MobileTableColumn col in this[i].Columns)
                        {
                            string _prm = $"@{TableName}_{col.Name}_{i}";

                            _vals.Add(_prm);

                            parameters.Add(new DbParameter
                            {
                                ParameterName = _prm,
                                DbType = (int)col.Type,
                                Value = col.Control?.ActualToSQLite(col.Value) ?? col.Value
                            });
                        }

                        if (TableName != masterTable)
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
                            {
                                _vals.Add($"(SELECT MAX(id) FROM {masterTable})");
                            }
                        }
                        sb.AppendFormat("INSERT INTO {0}({1}) VALUES ({2});", TableName, string.Join(",", _cols), string.Join(",", _vals.ToArray()));
                    }
                }
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
            return sb.ToString();
        }

        public void InitFilesToUpload()
        {
            OldFiles = new List<FileWrapper>();
            NewFiles = new List<FileWrapper>();

            foreach (var pair in this.Files)
            {
                pair.Value.ForEach(item =>
                {
                    if (item.IsUploaded == false)
                        NewFiles.Add(item);
                    else
                        OldFiles.Add(item);
                });
            }
        }

        public MobileTableRow CreateRow(int rowid = 0)
        {
            MobileTableRow row = new MobileTableRow(rowid);
            this.Add(row);
            return row;
        }

        public List<T> GetColumnValues<T>(string columnName)
        {
            List<T> values = new List<T>();
            var converter = TypeDescriptor.GetConverter(typeof(T));

            foreach (MobileTableRow row in this)
            {
                MobileTableColumn column = row[columnName];

                if (column != null)
                {
                    try
                    {
                        string st = Convert.ToString(column.Value);
                        if (string.IsNullOrWhiteSpace(st))
                            values.Add(default(T));
                        else
                            values.Add((T)converter.ConvertFromString(st));
                    }
                    catch (Exception ex)
                    {
                        EbLog.Error("GetColumnValues error : " + ex.Message);
                    }
                }
            }
            return values;
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

        public void AppendEbColValues(bool addCreatedAt, bool isPrimaryTable)
        {
            this.Columns.Add(new MobileTableColumn { Name = "eb_loc_id", Type = EbDbTypes.Int32, Value = App.Settings.CurrentLocId });

            if (isPrimaryTable)
            {
                if (addCreatedAt)//eb_created_at_device for Offline submission
                    this.Columns.Add(new MobileTableColumn { Name = "eb_created_at_device", Type = EbDbTypes.DateTime, Value = DateTime.UtcNow });

                try
                {
                    INativeHelper helper = DependencyService.Get<INativeHelper>();
                    string appversion = string.Format("{0}({1} {2}:{3})-{4}", DeviceInfo.Manufacturer, DeviceInfo.Model, DeviceInfo.Platform, DeviceInfo.VersionString, helper.AppVersion);

                    this.Columns.Add(new MobileTableColumn { Name = "eb_device_id", Type = EbDbTypes.String, Value = helper.DeviceId });
                    this.Columns.Add(new MobileTableColumn { Name = "eb_appversion", Type = EbDbTypes.String, Value = appversion });
                }
                catch (Exception ex)
                {
                    EbLog.Error(ex.Message);
                }
            }
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

        public object DisplayValue { set; get; }

        public EbDbTypes Type { set; get; }

        public EbMobileControl Control { set; get; }

        public MobileTableColumn() { }

        public MobileTableColumn(string name, EbDbTypes type, object value)
        {
            Name = name;
            Type = type;
            Value = value;
        }

        public object getValue() //for script
        {
            return Value;
        }
    }

    public class FileWrapper
    {
        public string Name { set; get; }

        public string FileName { set; get; }

        public byte[] Bytea { set; get; }

        public string ControlName { set; get; }

        public int FileRefId { set; get; }

        public bool IsUploaded { set; get; }
    }

    public class FormSaveResponse
    {
        public bool Status { set; get; }

        public string Message { set; get; }

        public PushResponse PushResponse { set; get; }
    }

    public class SyncResponse
    {
        public bool Status { set; get; }

        public string Message { set; get; }
    }

    public class FUPSetValueMeta
    {
        public string TableName { set; get; }

        public int RowId { set; get; }

        public List<FileMetaInfo> Files { set; get; }

        public string FileRefIds { set; get; }

        public FUPSetValueMeta()
        {
            Files = new List<FileMetaInfo>();
        }
    }

    public class FileMetaInfo
    {
        public string FileName { get; set; }

        public int FileSize { get; set; }

        public int FileRefId { get; set; }

        public Dictionary<string, List<string>> Meta { set; get; }

        public string UploadTime { get; set; }

        public EbFileCategory FileCategory { set; get; }
    }

    //for sort listview
    public class SortColumn
    {
        public string Name { set; get; }

        public SortOrder Order
        {
            get
            {
                if (IsToggled)
                    return SortOrder.Descending;
                else
                    return SortOrder.Ascending;
            }
        }

        public bool Selected { set; get; }

        public bool IsToggled { set; get; } = false;
    }

    public class MenuPreloadResponse : ApiResponse
    {
        public List<string> Result { set; get; }
    }

    public class IntRef
    {
        public int Value { set; get; }

        public IntRef()
        {
            Value = 0;
        }

        public IntRef(int value)
        {
            Value = value;
        }

        public void Increment()
        {
            Value += 1;
        }

        public void Decrement()
        {
            Value -= 1;
        }

        public void Reset()
        {
            Value = 0;
        }
    }

    public class EbPageRenderer
    {
        public bool IsReady { set; get; }

        public string Message { set; get; }

        public Page Renderer { set; get; }
    }
}
