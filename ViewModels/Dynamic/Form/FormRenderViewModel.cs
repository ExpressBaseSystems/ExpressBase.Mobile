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

        public FormMode Mode { set; get; }

        protected IFormDataService FormDataService;

        protected int RowId { set; get; }

        public string SubmitButtonText { set; get; }

        public bool HasWebFormRef => !string.IsNullOrEmpty(this.Form.WebFormRefId);

        public Command SaveCommand => new Command(async () => await this.FormSubmitClicked());

        public FormRenderViewModel(EbMobilePage page) : base(page)
        {
            this.Mode = FormMode.NEW;
            this.Form = (EbMobileForm)this.Page.Container;
            this.Controls = this.Form.ChildControls;

            this.FormDataService = new FormDataServices();
            this.SubmitButtonText = this.Form.GetSubmitButtonText();
        }

        public override async Task InitializeAsync()
        {
            this.Form.InitializeControlDict();
            EbFormHelper.Initialize(this.Form.ControlDictionary);

            if (!this.IsOnline())
            {
                //creating offline tables if not exist
                await Task.Run(() => this.Form.CreateTableSchema());
            }

            if (this.Mode == FormMode.NEW)
                this.SetValues();
        }

        protected virtual void SetValues()
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

        public async Task FormSubmitClicked()
        {
            if (!Utils.IsNetworkReady(this.NetworkType))
            {
                Utils.Alert_NoInternet();
                return;
            }

            if (this.Form.Validate())
            {
                EbLog.Info($"Form '{this.PageName}' validation success ready to submit");

                this.Form.NetworkType = this.NetworkType;

                await this.Submit();
            }
            else
            {
                EbLog.Info($"Form '{this.PageName}' validation failed, some fields are required");
                Utils.Toast("Fields required");
            }
        }

        private async Task Submit()
        {
            try
            {
                Device.BeginInvokeOnMainThread(() => IsBusy = true);

                FormSaveResponse response = await this.Form.Save(this.RowId);

                Device.BeginInvokeOnMainThread(async () =>
                {
                    if (response.Status)
                    {
                        NavigationService.UpdateViewStack();
                        await App.RootMaster.Detail.Navigation.PopAsync(true);
                    }

                    IsBusy = false;
                    Utils.Toast(response.Message);

                    EbLog.Info($"{this.PageName} save status '{response.Status}'");
                    EbLog.Info(response.Message);
                });
            }
            catch (Exception ex)
            {
                EbLog.Info($"Submit() raised some error");
                EbLog.Error(ex.Message);

                Device.BeginInvokeOnMainThread(() => IsBusy = false);
            }
        }

        public void EnableControls()
        {
            foreach (var ctrl in this.Form.ControlDictionary.Values)
            {
                ctrl.SetAsReadOnly(false);
            }
        }
    }
}
