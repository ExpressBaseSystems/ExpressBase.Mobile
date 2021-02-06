using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.ViewModels.Dynamic
{
    public class FormRenderVME : FormRenderViewModel
    {
        private EbDataSet formData;

        private Dictionary<string, List<FileMetaInfo>> filesData;

        private WebformData webFormData;

        public FormRenderVME(EbMobilePage page, int rowid) : base(page)
        {
            this.Mode = FormMode.EDIT;
            this.RowId = rowid;

            SubmitButtonText = "Save Changes";
            this.IsEditButtonVisible = true;
        }

        public FormRenderVME(EbMobilePage page, int rowId, WebformData data) : base(page)
        {
            this.Mode = FormMode.EDIT;
            this.RowId = rowId;
            webFormData = data;
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            await InitializeFormData();
            SetValues();
        }

        private async Task InitializeFormData()
        {
            try
            {
                if (IsOffline())
                {
                    this.formData = await FormDataService.GetFormLocalDataAsync(this.Form, this.RowId);
                }
                else if (IsOnline())
                {
                    if (webFormData == null)
                    {
                        webFormData = await FormDataService.GetFormLiveDataAsync(this.Page.RefId, this.RowId, App.Settings.CurrentLocId);
                    }
                    this.formData = webFormData?.ToDataSet();
                    this.filesData = webFormData?.ToFilesMeta();
                }
            }
            catch (Exception ex)
            {
                EbLog.Error($"InitializeFormData error in form edit '{this.Page.DisplayName}'");
                EbLog.Error(ex.Message);
            }
        }

        protected override void SetValues()
        {
            EbDataTable masterData = formData.Tables.Find(table => table.TableName == this.Form.TableName);

            if (masterData == null)
            {
                EbLog.Info($"data not found for table:'{this.Form.TableName}'");
                return;
            }

            EbDataRow masterRow = masterData.Rows.FirstOrDefault();

            if (masterRow != null)
                this.SetControlValues(masterRow);
            else
                EbLog.Info($"master row not found in table:'{this.Form.TableName}'");

            base.SetValues();
        }

        private void SetControlValues(EbDataRow masterRow)
        {
            foreach (var pair in this.Form.ControlDictionary)
            {
                EbMobileControl ctrl = pair.Value;
                object data = masterRow[ctrl.Name];
                try
                {
                    if (ctrl is IFileUploadControl)
                    {
                        this.SetFileData(ctrl, data);
                    }
                    else if (ctrl is ILinesEnabled line)
                    {
                        EbDataTable lines = this.formData.Tables.Find(table => table.TableName == line.TableName);
                        ctrl.SetValue(lines);
                    }
                    else
                        ctrl.SetValue(data);

                    if (this.IsEditButtonVisible)
                    {
                        ctrl.SetAsReadOnly(true);
                    }
                }
                catch (Exception ex)
                {
                    EbLog.Error("Error when setting value to controls on edit");
                    EbLog.Error(ex.Message);
                }
            }
        }

        private void SetFileData(EbMobileControl ctrl, object data)
        {
            FUPSetValueMeta fup = new FUPSetValueMeta
            {
                TableName = this.Form.TableName,
                RowId = this.RowId,
                FileRefIds = data?.ToString()
            };

            if (ctrl is EbMobileFileUpload)
            {
                if (this.filesData != null && this.filesData.ContainsKey(ctrl.Name))
                {
                    fup.Files.AddRange(this.filesData[ctrl.Name]);
                }
            }
            ctrl.SetValue(fup);
        }
    }
}
