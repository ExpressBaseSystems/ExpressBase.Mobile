using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Structures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpressBase.Mobile
{
    public class EbMobileForm : EbMobileContainer
    {
        public List<EbMobileControl> ChildControls { get; set; }

        public string TableName { set; get; }

        public bool AutoDeployMV { set; get; }

        public string AutoGenMVRefid { set; get; }

        public string WebFormRefId { set; get; }

        public string RenderValidatorRefId { set; get; }

        public List<Param> RenderValidatorParams { get; set; }

        public string MessageOnFailed { set; get; }

        public string SubmitButtonText { set; get; }

        public int Spacing { set; get; }

        public Dictionary<string, EbMobileControl> ControlDictionary { set; get; }

        private bool HasFileSelect => ControlDictionary.Any(x => x.Value.GetType() == typeof(EbMobileFileUpload));

        public bool RenderingAsExternal { set; get; }

        public Type RAERedirectionType { set; get; }

        public object[] RAERedirectionParams { set; get; }

        public EbMobileForm()
        {
            RenderValidatorParams = new List<Param>();
        }

        public void InitializeControlDict()
        {
            this.ControlDictionary = this.ChildControls.ToControlDictionary();
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

        public async Task<FormSaveResponse> Save(int rowId, string pageRefId)
        {
            FormSaveResponse response = new FormSaveResponse();
            try
            {
                MobileFormData data = await this.GetFormData(rowId);
                data.SortByMaster();

                if (NetworkType == NetworkMode.Online)
                {
                    await this.SaveToCloudDB(data, response, rowId, pageRefId);
                }
                else if (NetworkType == NetworkMode.Offline)
                {
                    await this.SaveToLocalDB(data, response, rowId);
                }
                else
                {
                    if (Utils.HasInternet)
                        await this.SaveToCloudDB(data, response, rowId, pageRefId);
                    else
                        await this.SaveToLocalDB(data, response, rowId);
                }
            }
            catch (Exception ex)
            {
                EbLog.Error("EbMobileForm.SaveForm::" + ex.Message);
            }
            return response;
        }

        private async Task<MobileFormData> GetFormData(int RowId)
        {
            MobileFormData formData = new MobileFormData(this.TableName);
            MobileTable table = formData.CreateTable();
            MobileTableRow row = table.CreateRow(RowId);

            foreach (KeyValuePair<string, EbMobileControl> pair in this.ControlDictionary)
            {
                EbMobileControl ctrl = pair.Value;

                if (ctrl is IFileUploadControl)
                {
                    await this.GetFileData(ctrl, table, row);
                }
                else if (ctrl is EbMobileDataGrid dataGrid)
                {
                    formData.Tables.Add(dataGrid.GetValue<MobileTable>());
                }
                else
                {
                    MobileTableColumn Column = ctrl.GetMobileTableColumn();
                    if (Column != null) row.Columns.Add(Column);
                }
            }
            if (RowId <= 0) row.AppendEbColValues();

            return formData;
        }

        private async Task GetFileData(EbMobileControl ctrl, MobileTable table, MobileTableRow row)
        {
            if (ctrl is EbMobileFileUpload fup)
            {
                table.Files.Add(ctrl.Name, fup.GetValue<List<FileWrapper>>());
            }
            else if (ctrl is EbMobileDisplayPicture || ctrl is EbMobileAudioInput)
            {
                await SendAndFillFupPersisant(ctrl, row);
            }
            else
            {
                EbLog.Warning("Unknown control");
                EbLog.Warning(ctrl.ToString());
            }
        }

        private async Task<bool> SendAndFillFupData(WebformData webformdata, MobileTable table)
        {
            if (table.NewFiles.Any())
            {
                try
                {
                    List<ApiFileData> resp = await FormService.Instance.SendFilesAsync(table.NewFiles);

                    webformdata.FillFromSendFileResp(table.NewFiles, resp);
                    webformdata.FillUploadedFileRef(table.OldFiles);

                    if (!webformdata.ExtendedTables.Any())
                    {
                        EbLog.Error("Image upload response empty, breaking save");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    EbLog.Error("SendFilesAsync error :: " + ex.Message);
                }
            }
            return true;
        }

        private async Task SendAndFillFupPersisant(EbMobileControl ctrl, MobileTableRow row)
        {
            List<FileWrapper> Files = ctrl.GetValue<List<FileWrapper>>();

            if (!Files.Any()) return;

            try
            {
                List<ApiFileData> resp = await FormService.Instance.SendFilesAsync(Files);
                if (resp.Any())
                {
                    MobileTableColumn column = ctrl.GetMobileTableColumn();
                    List<int> refIds = (from item in resp where item.FileRefId > 0 select item.FileRefId).ToList();
                    if (refIds.Count > 0)
                    {
                        string uploadedRefs = string.Join<int>(",", refIds);

                        if (ctrl is EbMobileAudioInput audio && audio.MultiSelect)
                        {
                            if (audio.OldValue != null) uploadedRefs = audio.OldValue.ToString() + CharConstants.COMMA + uploadedRefs;
                        }
                        column.Value = uploadedRefs;
                        row.Columns.Add(column);
                    }
                }
            }
            catch (Exception ex)
            {
                EbLog.Error("error occured when sending persistant fup controls values");
                EbLog.Error(ex.Message + ", STACKTRACE:" + ex.StackTrace);
            }
        }

        private async Task SaveToCloudDB(MobileFormData data, FormSaveResponse response, int rowId, string pageRefId)
        {
            try
            {
                WebformData webFormData = data.ToWebFormData();

                foreach (MobileTable table in data.Tables)
                {
                    if (table.Files.Any())
                    {
                        table.InitFilesToUpload();
                        bool status = await this.SendAndFillFupData(webFormData, table);
                        if (!status)
                        {
                            throw new Exception("Image Upload faild, breaking form save");
                        }
                    }
                }

                int locid = App.Settings.CurrentLocId;
                EbLog.Info($"saving record in location {locid}");

                PushResponse pushResponse = await FormService.Instance.SendFormDataAsync(pageRefId, webFormData, rowId, this.WebFormRefId, locid);

                this.LogPushResponse(pushResponse);

                if (pushResponse.RowAffected > 0)
                {
                    response.Status = true;
                    response.Message = "saved successfully :)";
                    response.PushResponse = pushResponse;
                }
                else
                    throw new Exception(pushResponse.Message);
            }
            catch (Exception ex)
            {
                response.Status = false;
                response.Message = ex.Message;
                EbLog.Error(ex.Message);
            }
        }

        private async Task SaveToLocalDB(MobileFormData data, FormSaveResponse response, int rowId)
        {
            try
            {
                List<DbParameter> _params = new List<DbParameter>();

                string query = data.GetQuery(_params, rowId);

                int rowAffected = App.DataDB.DoNonQuery(query, _params.ToArray());

                if (this.HasFileSelect)
                {
                    object lastRowId = (rowId != 0) ? rowId : App.DataDB.DoScalar(string.Format(StaticQueries.CURRVAL, this.TableName));
                    await this.WriteFilesToLocal(lastRowId);
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
                EbLog.Error("EbMobileForm.PersistOnLocal::" + ex.Message);
            }
        }

        private void LogPushResponse(PushResponse resp)
        {
            EbLog.Info(resp.Message);
            EbLog.Info(resp.MessageInt);
            EbLog.Info(JsonConvert.SerializeObject(resp));
        }

        private async Task WriteFilesToLocal(object LastRowId)
        {
            int rowid = Convert.ToInt32(LastRowId);

            await Task.Run(() =>
            {
                foreach (var pair in this.ControlDictionary)
                {
                    if (pair.Value is EbMobileFileUpload)
                        (pair.Value as EbMobileFileUpload).PushFilesToDir(this.TableName, rowid);
                }
            });
        }

        public async Task UploadFiles(int RowId, WebformData WebFormData)
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
                var ApiFiles = await FormService.Instance.SendFilesAsync(Files);
                WebFormData.FillFromSendFileResp(Files, ApiFiles);
            }
        }

        public void FlagLocalRow(PushResponse response, int rowId)
        {
            try
            {
                if (response.RowAffected > 0)
                {
                    DbParameter[] parameter = new DbParameter[]
                    {
                        new DbParameter{ParameterName="@rowid",Value = rowId},
                        new DbParameter{ParameterName="@cloudrowid",Value = response.RowId}
                    };
                    int rowAffected = App.DataDB.DoNonQuery(string.Format(StaticQueries.FLAG_LOCALROW_SYNCED, this.TableName), parameter);
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

            EbMobileControl ctrl = ControlDictionary.ContainsKey(name) ? ControlDictionary[name] : null;
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
                EbLog.Error(ex.Message);
            }
        }

        public List<Param> GetRenderValidatorParams(EbDataRow row)
        {
            List<Param> param = new List<Param>();

            if (row == null) return param;

            foreach (Param p in RenderValidatorParams)
            {
                object val = row[p.Name];

                if (val != null)
                {
                    param.Add(new Param
                    {
                        Name = p.Name,
                        Type = p.Type,
                        Value = val.ToString()
                    });
                }
            }
            return param;
        }

        public string GetSubmitButtonText()
        {
            return string.IsNullOrEmpty(this.SubmitButtonText) ? "Save" : this.SubmitButtonText;
        }
    }
}
