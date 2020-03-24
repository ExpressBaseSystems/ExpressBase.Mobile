using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Structures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbMobileForm : EbMobileContainer
    {
        public List<EbMobileControl> ChildControls { get; set; }

        public string TableName { set; get; }

        public bool AutoDeployMV { set; get; }

        public string AutoGenMVRefid { set; get; }

        public string WebFormRefId { set; get; }

        //mobile prop
        public Dictionary<string, EbMobileControl> ControlDictionary { set; get; }

        private FormMode Mode { set; get; }

        private int ParentId;

        private string ParentTable;

        private bool HasFileSelect
        {
            get { return ControlDictionary.Any(x => x.GetType() == typeof(EbMobileFileUpload)); }
        }

        public EbMobileForm()
        {
            ControlDictionary = new Dictionary<string, EbMobileControl>();
        }

        public string GetQuery()
        {
            List<string> colums = new List<string> { "eb_device_id", "eb_appversion", "eb_created_at_device", "eb_loc_id", "id" };

            if (!ControlDictionary.Any())
                ControlDictionary = ChildControls.ToControlDictionary();

            foreach (var pair in ControlDictionary)
            {
                if (!(pair.Value is INonPersistControl) && !(pair.Value is ILinesEnabled))
                    colums.Add(pair.Value.Name);
            }
            colums.Reverse();

            return string.Join(",", colums.ToArray());
        }

        public EbDataTable GetLocalData()
        {
            EbDataTable dt;
            try
            {
                dt = App.DataDB.DoQuery(string.Format(StaticQueries.STARFROM_TABLE, this.GetQuery(), this.TableName));
            }
            catch (Exception ex)
            {
                dt = new EbDataTable();
                Console.WriteLine(ex.Message);
            }
            return dt;
        }

        public FormSaveResponse SaveForm(int rowId)
        {
            FormSaveResponse response = new FormSaveResponse();
            try
            {
                MobileFormData data = this.PrepareFormData(rowId);
                data.SortByMaster();//sort then mastertable will be the first index

                switch (this.NetworkType)
                {
                    case NetworkMode.Online:
                        this.PersistCloud(data, response, rowId);
                        break;
                    case NetworkMode.Mixed:
                        if (Settings.HasInternet)
                            this.PersistCloud(data, response, rowId);
                        else
                            this.PersistLocal(data, response, rowId);
                        break;
                    default:
                        this.PersistLocal(data, response, rowId);
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Write("EbMobileForm.SaveForm::" + ex.Message);
            }
            return response;
        }

        public FormSaveResponse SaveFormWParent(int parentId, string parentTable)
        {
            this.Mode = FormMode.REF;
            this.ParentId = parentId;
            this.ParentTable = parentTable;
            return SaveForm(0);
        }

        private MobileFormData PrepareFormData(int RowId)
        {
            MobileFormData FormData = new MobileFormData(this.TableName);
            MobileTable Table = new MobileTable(this.TableName);
            MobileTableRow row = new MobileTableRow(RowId);

            Table.Add(row);
            foreach (var pair in this.ControlDictionary)
            {
                if (pair.Value is EbMobileFileUpload)
                    Table.Files.Add(pair.Key, (pair.Value as EbMobileFileUpload).GetFiles());
                else if (pair.Value is EbMobileDataGrid)
                    FormData.Tables.Add((MobileTable)(pair.Value as EbMobileDataGrid).GetValue());
                else
                {
                    MobileTableColumn Column = pair.Value.GetMobileTableColumn();
                    if (Column != null)
                        row.Columns.Add(Column);
                }
            }
            if (RowId <= 0)
                row.AppendEbColValues();//append ebcol values

            if (this.Mode == FormMode.REF)
                row.Columns.Add(new MobileTableColumn(this.ParentTable + "_id", EbDbTypes.Int32, ParentId));

            FormData.Tables.Add(Table);
            return FormData;
        }

        private void PersistCloud(MobileFormData data, FormSaveResponse response, int rowId)
        {
            try
            {
                WebformData webformdata = data.ToWebFormData();

                foreach (MobileTable table in data.Tables)
                {
                    List<FileWrapper> files = new List<FileWrapper>();
                    foreach (var pair in table.Files)
                        files.AddRange(pair.Value);
                    if (files.Any())
                    {
                        var resp = RestServices.Instance.PushFiles(files);
                        webformdata.ExtendedTables = files.GroupByControl(resp);

                        if (!webformdata.ExtendedTables.Any())
                            throw new Exception("Image Upload faild");
                    }
                }
                PushResponse pushResponse = RestServices.Instance.Push(webformdata, rowId, this.WebFormRefId, Settings.LocationId);
                if (pushResponse.RowAffected > 0)
                {
                    response.Status = true;
                    response.Message = "Data pushed to cloud :)";
                }
                else
                    throw new Exception("Failed to push data");
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong :(";
                Log.Write("EbMobileForm.PushToCloud---" + ex.Message);
            }
        }

        private void PersistLocal(MobileFormData data, FormSaveResponse response, int rowId)
        {
            try
            {
                List<DbParameter> _params = new List<DbParameter>();

                string query = data.GetQuery(_params);

                int rowAffected = App.DataDB.DoNonQuery(query, _params.ToArray());

                if (this.HasFileSelect)
                {
                    object lastRowId = (rowId != 0) ? rowId : App.DataDB.DoScalar(string.Format(StaticQueries.CURRVAL, this.TableName));
                    this.WriteFilesToLocal(lastRowId);
                }

                if (rowAffected > 0)
                {
                    response.Status = true;
                    response.Message = "Data stored locally :)";
                }
                else
                    throw new Exception("failed to store data locally");
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = "Something went wrong :(";
                Log.Write("EbMobileForm.PersistOnLocal::" + ex.Message);
            }
        }

        private void WriteFilesToLocal(object LastRowId)
        {
            int rowid = Convert.ToInt32(LastRowId);

            Task.Run(() =>
            {
                foreach (var pair in this.ControlDictionary)
                {
                    if (pair.Value is EbMobileFileUpload)
                        (pair.Value as EbMobileFileUpload).PushFilesToDir(this.TableName, rowid);
                }
            });
        }

        public void UploadFiles(int RowId, WebformData WebFormData)
        {
            ControlDictionary = ChildControls.ToControlDictionary();
            List<FileWrapper> Files = new List<FileWrapper>();

            foreach (var pair in ControlDictionary)
            {
                if (pair.Value is EbMobileFileUpload)
                {
                    string pattern = $"{this.TableName}-{RowId}-{pair.Value.Name}*";
                    Files.AddRange(HelperFunctions.GetFilesByPattern(pattern, pair.Value.Name));
                }
            }
            if (Files.Count > 0)
            {
                var ApiFiles = RestServices.Instance.PushFiles(Files);
                var ExtendedTable = Files.GroupByControl(ApiFiles);
                if (ExtendedTable.Any())
                    WebFormData.ExtendedTables = ExtendedTable;
            }
        }

        public void FlagLocalRow(PushResponse Response, int RowId, string TableName)
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

        public DbTypedValue GetDbType(string name, object value, EbDbTypes type)
        {
            DbTypedValue TV = new DbTypedValue(name, value, type);

            var ctrl = ControlDictionary.ContainsKey(name) ? ControlDictionary[name] : null;
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

        public List<SingleColumn> GetColumnValues(EbDataTable LocalData, int RowIndex)
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
            try
            {
                SQLiteTableSchemaList schemas = new SQLiteTableSchemaList();

                this.ControlDictionary = this.ChildControls.ToControlDictionary();

                SQLiteTableSchema masterSchema = new SQLiteTableSchema() { TableName = this.TableName };
                schemas.Add(masterSchema);
                foreach (var pair in this.ControlDictionary)
                {
                    if (pair.Value is INonPersistControl)
                        continue;

                    if (pair.Value is ILinesEnabled)
                    {
                        SQLiteTableSchema linesSchema = new SQLiteTableSchema() { TableName = (pair.Value as ILinesEnabled).TableName };

                        foreach (var ctrl in (pair.Value as ILinesEnabled).ChildControls)
                        {
                            linesSchema.Columns.Add(new SQLiteColumSchema
                            {
                                ColumnName = ctrl.Name,
                                ColumnType = ctrl.SQLiteType
                            });
                        }
                        linesSchema.AppendDefault();
                        linesSchema.Columns.Add(new SQLiteColumSchema
                        {
                            ColumnName = this.TableName + "_id",
                            ColumnType = "INT"
                        });

                        schemas.Add(linesSchema);
                    }
                    else
                    {
                        masterSchema.Columns.Add(new SQLiteColumSchema
                        {
                            ColumnName = pair.Value.Name,
                            ColumnType = pair.Value.SQLiteType
                        });
                    }
                }

                masterSchema.AppendDefault();
                CommonServices.Instance.CreateLocalTable(schemas);
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }
    }
}
