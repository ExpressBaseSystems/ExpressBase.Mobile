using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels.BaseModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels.Dynamic
{
    public class FormRenderViewModel : DynamicBaseViewModel
    {
        public EbMobileForm Form { set; get; }

        public List<EbMobileControl> Controls { set; get; }

        public EbDataRow Context { set; get; }

        public FormMode Mode { set; get; }

        protected IFormService FormDataService;

        protected int RowId { set; get; }

        public string SubmitButtonText { set; get; }

        private bool isEditBtnVisible = false;

        private bool isSaveBtnVisible = true;

        public bool IsSaveButtonVisible
        {
            get => isSaveBtnVisible;
            set
            {
                this.isSaveBtnVisible = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsEditButtonVisible
        {
            get => isEditBtnVisible;
            set
            {
                isEditBtnVisible = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsHomeButtonVisibile => !this.Form.RenderingAsExternal;

        public bool HasWebFormRef => !string.IsNullOrEmpty(this.Form.WebFormRefId);

        public Command SaveCommand => new Command(async () => await FormSubmitClicked());

        public FormRenderViewModel() { }

        public FormRenderViewModel(EbMobilePage page) : base(page)
        {
            Mode = FormMode.NEW;
            Form = (EbMobileForm)this.Page.Container;
            Controls = this.Form.ChildControls;

            FormDataService = new FormService();
            SubmitButtonText = this.Form.GetSubmitButtonText();
        }

        public override async Task InitializeAsync()
        {
            this.Form.InitializeControlDict();

            EbFormHelper.Initialize(this.Form, this.Mode);

            if (IsOffline())
            {
                await Task.Run(() => this.Form.CreateTableSchema());
            }

            if (this.Mode == FormMode.NEW) SetValues();
        }

        protected virtual void SetValues()
        {
            if (this.Mode == FormMode.NEW || this.Mode == FormMode.PREFILL || this.Mode == FormMode.REF)
                InitDefaultValueExpressions();

            InitOnLoadExpressions();
        }

        public async Task FormSubmitClicked()
        {
            if (!Utils.IsNetworkReady(this.NetworkType))
            {
                Utils.Alert_NoInternet();
                return;
            }

            if (EbFormHelper.Validate())
            {
                EbLog.Info($"Form '{this.PageName}' validation success ready to submit");
                this.Form.NetworkType = this.NetworkType;
                await Submit();
            }
            else
            {
                EbLog.Info($"Form '{this.PageName}' validation failed, some fields are required");
                Utils.Toast("Fields required");
            }
        }

        protected virtual async Task Submit()
        {
            try
            {
                Device.BeginInvokeOnMainThread(() => IsBusy = true);

                FormSaveResponse response = await this.Form.Save(this.RowId, this.Page.RefId);

                Device.BeginInvokeOnMainThread(async () =>
                {
                    if (response.Status)
                    {
                        if (Form.RenderingAsExternal)
                        {
                            Page contentPage = (Page)Activator.CreateInstance(Form.RAERedirectionType, Form.RAERedirectionParams);
                            await App.Navigation.NavigateByRenderer(contentPage);
                        }
                        else
                        {
                            App.Navigation.UpdateViewStack();
                            await App.Navigation.PopMasterAsync(true);
                        }
                    }
                    Utils.Toast(response.Message);

                    EbLog.Info($"{this.PageName} save status '{response.Status}'");
                    EbLog.Info(response.Message);
                });
            }
            catch (Exception ex)
            {
                EbLog.Info($"Submit() raised some error");
                EbLog.Error(ex.Message);
            }
            Device.BeginInvokeOnMainThread(() => IsBusy = false);
        }

        protected void InitDefaultValueExpressions()
        {
            foreach (EbMobileControl ctrl in this.Form.ControlDictionary.Values)
            {
                if (!ctrl.DefaultExprEvaluated)
                {
                    EbScript defExp = ctrl.DefaultValueExpression;

                    if (defExp != null && !defExp.IsEmpty())
                    {
                        EbFormHelper.SetDefaultValue(ctrl.Name);
                    }
                }
            }
        }

        protected void InitOnLoadExpressions()
        {
            foreach (EbMobileControl ctrl in this.Form.ControlDictionary.Values)
            {
                EbFormHelper.EvaluateExprOnLoad(ctrl, this.Mode);
            }
        }
    }
}
