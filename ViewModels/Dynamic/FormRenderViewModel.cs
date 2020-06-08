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

        private readonly EbMobileForm parentForm;

        private EbDataSet dataOnEdit;

        private Dictionary<string, List<FileMetaInfo>> filesOnEdit;

        private readonly EbDataRow preFillRecord;

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
        public FormRenderViewModel(EbMobilePage page, EbDataRow currentRow) : base(page)
        {
            this.Mode = FormMode.PREFILL;
            this.Form = (EbMobileForm)page.Container;
            preFillRecord = currentRow;

            InitializeService();
        }

        //referenced mode
        public FormRenderViewModel(EbMobilePage page, EbMobilePage parentPage, int parentId) : base(page)
        {
            this.Mode = FormMode.REF;
            this.parentForm = (EbMobileForm)parentPage.Container;
            rowId = parentId;
            this.Form = (EbMobileForm)page.Container;

            InitializeService();
        }

        private void InitializeService()
        {
            try
            {
                //service initialize
                formDataService = new FormDataServices();

                Controls = Form.ChildControls;
                Form.ControlDictionary = Form.ChildControls.ToControlDictionary();
                this.DeployTables();
            }
            catch (Exception ex)
            {
                EbLog.Write(ex.Message);
            }
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
            try
            {
                if (this.Page.NetworkMode == NetworkMode.Offline)
                {
                    this.dataOnEdit = await formDataService.GetFormLocalDataAsync(this.Form, this.rowId);
                }
                else if (this.Page.NetworkMode == NetworkMode.Online)
                {
                    if (string.IsNullOrEmpty(this.Form.WebFormRefId))
                        throw new Exception("webform refid is empty");

                    WebformData data = await formDataService.GetFormLiveDataAsync(this.Page.RefId, this.rowId, App.Settings.CurrentLocId);
                    this.dataOnEdit = data.ToDataSet();
                    this.filesOnEdit = data.ToFilesMeta();
                }
            }
            catch (Exception e)
            {
                EbLog.Write("form_SetDataOnEdit---" + e.Message);
                this.dataOnEdit = new EbDataSet();
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
                FormSaveResponse saveResponse;

                Device.BeginInvokeOnMainThread(() => IsBusy = true);

                if (this.Mode == FormMode.REF)
                    saveResponse = await this.Form.SaveFormWParent(this.rowId, parentForm.TableName);
                else if (this.Mode == FormMode.EDIT)
                    saveResponse = await this.Form.SaveForm(this.rowId);
                else
                    saveResponse = await this.Form.SaveForm(0);

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

        public void FillControlsValues()
        {
            try
            {
                EbDataTable masterData = dataOnEdit.Tables.Find(table => table.TableName == this.Form.TableName);
                if (masterData == null && !masterData.Rows.Any()) return;
                EbDataRow masterRow = masterData.Rows.FirstOrDefault();

                foreach (KeyValuePair<string, EbMobileControl> pair in this.Form.ControlDictionary)
                {
                    object data = masterRow[pair.Value.Name];

                    if (pair.Value is EbMobileFileUpload)
                    {
                        var fup = new FUPSetValueMeta
                        {
                            TableName = this.Form.TableName,
                            RowId = this.rowId
                        };

                        if (this.filesOnEdit != null && this.filesOnEdit.ContainsKey(pair.Value.Name))
                        {
                            fup.Files.AddRange(this.filesOnEdit[pair.Value.Name]);
                        }
                        pair.Value.SetValue(fup);
                    }
                    else if (pair.Value is ILinesEnabled)
                    {
                        EbDataTable lines = dataOnEdit.Tables.Find(table => table.TableName == (pair.Value as ILinesEnabled).TableName);
                        pair.Value.SetValue(lines);
                    }
                    else
                        pair.Value.SetValue(data);

                    pair.Value.SetAsReadOnly(true);
                }
            }
            catch (Exception ex)
            {
                EbLog.Write(ex.Message);
            }
        }

        public void FillControlsFlat()
        {
            try
            {
                foreach (KeyValuePair<string, EbMobileControl> pair in this.Form.ControlDictionary)
                {
                    object data = preFillRecord[pair.Value.Name];
                    if (pair.Value is INonPersistControl || pair.Value is ILinesEnabled) continue;
                    else
                        pair.Value.SetValue(data);
                }
            }
            catch (Exception ex)
            {
                EbLog.Write(ex.Message);
            }
        }
    }
}
