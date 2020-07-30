using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels.BaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels.Dynamic
{
    public class FormRenderViewModel : DynamicBaseViewModel
    {
        public EbMobileForm Form { set; get; }

        public List<EbMobileControl> Controls { set; get; }

        public FormMode Mode { set; get; }

        private IFormDataService formDataService;

        private readonly int rowId;

        private EbDataSet dataOnEdit;

        private Dictionary<string, List<FileMetaInfo>> filesOnEdit;

        private readonly EbDataRow contextRow;

        private readonly EbMobileVisualization context;

        public Command SaveCommand => new Command(async () => await OnSaveClicked());

        //new mode
        public FormRenderViewModel(EbMobilePage page) : base(page)
        {
            this.Mode = FormMode.NEW;
            this.Form = (EbMobileForm)this.Page.Container;

            InitializeService();
        }

        //edit
        public FormRenderViewModel(EbMobilePage page, int rowid) : base(page)
        {
            this.Mode = FormMode.EDIT;
            rowId = rowid;
            this.Form = (EbMobileForm)page.Container;

            InitializeService();
        }

        //prefill mode
        public FormRenderViewModel(EbMobilePage page, EbMobileVisualization source, EbDataRow contextrow) : base(page)
        {
            this.Mode = FormMode.PREFILL;
            this.Form = (EbMobileForm)page.Container;

            context = source;
            contextRow = contextrow;

            InitializeService();
        }

        //referenced mode
        public FormRenderViewModel(EbMobilePage page, EbMobileVisualization source, EbDataRow contextrow, int unused) : base(page)
        {
            this.Mode = FormMode.REF;
            this.Form = (EbMobileForm)page.Container;

            context = source;
            contextRow = contextrow;

            InitializeService();
        }

        private void InitializeService()
        {
            formDataService = new FormDataServices();

            Controls = Form.ChildControls;
            Form.ControlDictionary = Form.ChildControls.ToControlDictionary();
            this.DeployTables();
        }

        public void DeployTables()
        {
            Task.Run(() =>
            {
                if (this.NetworkType != NetworkMode.Online)
                    this.Form.CreateTableSchema();
            });
        }

        public async Task SetDataOnEdit()
        {
            if (this.Page.NetworkMode == NetworkMode.Offline)
            {
                this.dataOnEdit = await formDataService.GetFormLocalDataAsync(this.Form, this.rowId);
            }
            else if (this.Page.NetworkMode == NetworkMode.Online)
            {
                if (string.IsNullOrEmpty(this.Form.WebFormRefId))
                {
                    EbLog.Write("Web form refid is empty");
                }

                WebformData data = await formDataService.GetFormLiveDataAsync(this.Page.RefId, this.rowId, App.Settings.CurrentLocId);
                this.dataOnEdit = data?.ToDataSet();
                this.filesOnEdit = data?.ToFilesMeta();
            }
        }

        public async Task OnSaveClicked()
        {
            IToast toast = DependencyService.Get<IToast>();

            if (this.NetworkType == NetworkMode.Online && !Utils.HasInternet)
                toast.Show("you are not connected to Internet");

            if (Form.Validate())
            {
                this.Form.NetworkType = this.NetworkType;

                Device.BeginInvokeOnMainThread(() => IsBusy = true);

                FormSaveResponse saveResponse = await this.Form.Save(this.rowId);

                Device.BeginInvokeOnMainThread(async () =>
                {
                    IsBusy = false;
                    toast.Show(saveResponse.Message);
                    await (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PopAsync(true);
                });
            }
            else
                toast.Show("Fields required");
        }

        public async Task FillValues()
        {
            if (this.Mode == FormMode.EDIT)
            {
                await this.SetDataOnEdit();
                this.SetDataForEdit();
            }
            else if (this.Mode == FormMode.PREFILL)
            {
                this.SetDataForPrefill();
            }
            else if (this.Mode == FormMode.REF)
            {
                this.SetDataForRef();
            }
        }

        private void SetDataForEdit()
        {
            try
            {
                EbDataTable masterData = dataOnEdit.Tables.Find(table => table.TableName == this.Form.TableName);

                if (masterData == null) throw new Exception($"data not found for table:'{this.Form.TableName}'");

                EbDataRow masterRow = masterData.Rows.FirstOrDefault();

                if (masterRow == null) throw new Exception("no record found");

                foreach (var pair in this.Form.ControlDictionary)
                {
                    EbMobileControl ctrl = pair.Value;

                    object data = masterRow[ctrl.Name];

                    if (ctrl is EbMobileFileUpload)
                    {
                        this.SetFileDataForEdit(ctrl as EbMobileFileUpload, data);
                    }
                    else if (ctrl is ILinesEnabled)
                    {
                        EbDataTable lines = dataOnEdit.Tables.Find(table => table.TableName == (ctrl as ILinesEnabled).TableName);
                        ctrl.SetValue(lines);
                    }
                    else
                        ctrl.SetValue(data);

                    ctrl.SetAsReadOnly(true);
                }
            }
            catch (Exception ex)
            {
                EbLog.Write(ex.Message);
            }
        }

        private void SetFileDataForEdit(EbMobileFileUpload ctrl, object data)
        {
            FUPSetValueMeta fup = new FUPSetValueMeta
            {
                TableName = this.Form.TableName,
                RowId = this.rowId
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
                if (this.filesOnEdit != null && this.filesOnEdit.ContainsKey(ctrl.Name))
                {
                    fup.Files.AddRange(this.filesOnEdit[ctrl.Name]);
                }
            }

            ctrl.SetValue(fup);
        }

        private void SetDataForPrefill()
        {
            if (context.LinkFormParameters == null) return;

            foreach (EbMobileDataColToControlMap map in context.LinkFormParameters)
            {
                object value = contextRow[map.ColumnName];

                if (map.FormControl == null)
                    continue;

                if (this.Form.ControlDictionary.TryGetValue(map.FormControl.ControlName, out EbMobileControl ctrl))
                {
                    ctrl.SetValue(value);
                }
            }
        }

        private void SetDataForRef()
        {
            if (context.ContextToControlMap == null) return;

            foreach (var map in context.ContextToControlMap)
            {
                object value = contextRow[map.ColumnName];

                if (this.Form.ControlDictionary.TryGetValue(map.ControlName, out EbMobileControl ctrl))
                {
                    if (ctrl is INonPersistControl || ctrl is ILinesEnabled)
                        continue;
                    else
                        ctrl.SetValue(value);
                }
            }
        }
    }
}
