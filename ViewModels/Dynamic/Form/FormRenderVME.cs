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

        public FormRenderVME(EbMobilePage page, int rowid) : base(page)
        {
            this.Mode = FormMode.EDIT;
            this.RowId = rowid;
        }

        public override async Task InitializeAsync()
        {
            if (!HasWebFormRef())
            {
                EbLog.Info("Web form refid is empty");
                return;
            }
            await this.InitializeFormData();
            this.SetValues();
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
                    WebformData webform = await FormDataService.GetFormLiveDataAsync(this.Page.RefId, this.RowId, App.Settings.CurrentLocId);

                    this.formData = webform?.ToDataSet();
                    this.filesData = webform?.ToFilesMeta();
                }
            }
            catch (Exception ex)
            {
                EbLog.Error($"InitializeFormData error in form edit '{this.Page.DisplayName}'");
                EbLog.Error(ex.Message);
            }
        }

        private void SetValues()
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
        }

        private void SetControlValues(EbDataRow masterRow)
        {
            foreach (var pair in this.Form.ControlDictionary)
            {
                EbMobileControl ctrl = pair.Value;

                object data = masterRow[ctrl.Name];

                try
                {
                    if (ctrl is EbMobileFileUpload)
                    {
                        this.SetFileData(ctrl as EbMobileFileUpload, data);
                    }
                    else if (ctrl is ILinesEnabled line)
                    {
                        EbDataTable lines = this.formData.Tables.Find(table => table.TableName == line.TableName);
                        ctrl.SetValue(lines);
                    }
                    else
                        ctrl.SetValue(data);

                    ctrl.SetAsReadOnly(true);
                }
                catch (Exception ex)
                {
                    EbLog.Error("Error when setting value to controls on edit");
                    EbLog.Error(ex.Message);
                }
            }
        }

        private void SetFileData(EbMobileFileUpload ctrl, object data)
        {
            FUPSetValueMeta fup = new FUPSetValueMeta
            {
                TableName = this.Form.TableName,
                RowId = this.RowId
            };

            if (ctrl is EbMobileDisplayPicture)
            {
                fup.Files.Add(new FileMetaInfo
                {
                    FileCategory = EbFileCategory.Images,
                    FileRefId = data != null ? Convert.ToInt32(data) : 0
                });
            }
            else
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
