﻿using ExpressBase.Mobile.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ExpressBase.Mobile.Data
{
    public class WebformData
    {
        public string Name { set; get; }

        public Dictionary<string, SingleTable> MultipleTables { get; set; }

        public Dictionary<string, SingleTable> ExtendedTables { get; set; }

        public Dictionary<string, bool> DisableDelete { get; set; }

        public Dictionary<string, bool> DisableCancel { get; set; }

        public string MasterTable { get; set; }

        public int FormVersionId { get; set; }

        public bool IsLocked { get; set; }

        public string DataPushId { get; set; }

        public int SourceId { get; set; }

        public WebformData()
        {
            MultipleTables = new Dictionary<string, SingleTable>();
            ExtendedTables = new Dictionary<string, SingleTable>();
            DisableDelete = new Dictionary<string, bool>();
            DisableCancel = new Dictionary<string, bool>();
        }

        public WebformData(string masterTable)
        {
            this.MasterTable = masterTable;
            MultipleTables = new Dictionary<string, SingleTable>();
            ExtendedTables = new Dictionary<string, SingleTable>();
            DisableDelete = new Dictionary<string, bool>();
            DisableCancel = new Dictionary<string, bool>();
        }

        public void FillFromSendFileResp(List<FileWrapper> Files, List<ApiFileData> ApiFiles)
        {
            if (ApiFiles.Count > 0)
            {
                foreach (FileWrapper file in Files)
                {
                    ApiFileData _apiFile = ApiFiles.Find(af => af.FileName == file.FileName);

                    if (_apiFile != null && _apiFile.FileRefId > 0)
                    {
                        if (!ExtendedTables.ContainsKey(file.ControlName))
                        {
                            ExtendedTables.Add(file.ControlName, new SingleTable());
                        }

                        SingleRow row = new SingleRow();
                        row.Columns.Add(new SingleColumn
                        {
                            Name = _apiFile.FileName,
                            Value = _apiFile.FileRefId
                        });

                        ExtendedTables[file.ControlName].Add(row);
                    }
                }
            }
        }

        public void FillUploadedFileRef(List<FileWrapper> files)
        {
            foreach (FileWrapper file in files)
            {
                if (ExtendedTables.ContainsKey(file.ControlName))
                {
                    var row = new SingleRow();
                    row.Columns.Add(new SingleColumn
                    {
                        Name = file.FileName,
                        Value = file.FileRefId
                    });

                    ExtendedTables[file.ControlName].Add(row);
                }
            }
        }
    }

    public class SingleTable : List<SingleRow>
    {
        public string ParentTable { get; set; }

        public string ParentRowId { get; set; }
    }

    public class SingleRow
    {
        public int RowId { get; set; }

        public int LocId { get; set; }

        public bool IsUpdate { get; set; }

        public bool IsDelete { get; set; }

        public List<SingleColumn> Columns { get; set; }

        public SingleRow()
        {
            Columns = new List<SingleColumn>();
        }

        public SingleColumn this[string columnname]
        {
            get
            {
                return this.Columns.Find(item => item.Name == columnname) ?? null;
            }
        }
    }

    public class SingleColumn
    {
        public string Name { get; set; }

        public object Value { get; set; }

        public int Type { get; set; }

        public bool AutoIncrement { get; set; }

        public SingleColumn() { }
    }

    public class PushResponse
    {
        public int RowAffected { get; set; }

        public int RowId { get; set; }

        public int LocalRowId { get; set; }//local db use

        public string FormData { get; set; }

        public WebformData FormDataObject
        {
            get
            {
                if (FormData == null)
                    return null;
                return JsonConvert.DeserializeObject<WebformData>(this.FormData);
            }
        }

        public int Status { get; set; }

        public string Message { get; set; }

        public string MessageInt { get; set; }

        public string StackTraceInt { get; set; }

        public string AffectedEntries { get; set; }

        public Dictionary<string, string> MetaData { set; get; }

        public EbSignUpUserInfo GetSignUpUserInfo()
        {
            if (MetaData != null || MetaData.Count > 0)
            {
                if (MetaData.TryGetValue(FormMetaDataKeys.signup_user, out string signupUser))
                {
                    return JsonConvert.DeserializeObject<EbSignUpUserInfo>(signupUser);
                }
            }
            return null;
        }
    }

    public static class FormMetaDataKeys
    {
        public const string signup_user = "signup_user";
    }
}
