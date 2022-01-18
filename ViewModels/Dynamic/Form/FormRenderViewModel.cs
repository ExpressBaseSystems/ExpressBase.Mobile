﻿using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels.BaseModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels.Dynamic
{
    public class FormRenderViewModel : DynamicBaseViewModel
    {
        public EbMobileForm Form { set; get; }

        public List<EbMobileControl> Controls { set; get; }

        public EbDataRow Context { set; get; }

        public EbDataRow PrefillData { set; get; }

        public FormMode Mode { set; get; }

        protected IFormService FormDataService;

        protected int RowId { set; get; }

        public string SubmitButtonText { set; get; }

        public Loader MsgLoader { get; set; }

        public string PrintButtonText { set; get; }

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

        public Command SaveCommand => new Command(async () => await FormSubmitClicked(false));

        public Command PrintCommand => new Command(async () =>
        {
            if (this.RowId > 0 || this.Form.RenderAsFilterDialog)
            {
                MsgLoader.IsVisible = true;
                this.Form.NetworkType = this.NetworkType;//
                await this.Form.Print(this.RowId);
                MsgLoader.IsVisible = false;
            }
            else
                await FormSubmitClicked(true);
        });

        public FormRenderViewModel() { }

        public FormRenderViewModel(EbMobilePage page) : base(page)
        {
            Mode = FormMode.NEW;
            Form = (EbMobileForm)this.Page.Container;
            Controls = this.Form.ChildControls;

            FormDataService = new FormService();
            SubmitButtonText = this.Form.GetSubmitButtonText();
            PrintButtonText = this.Form.GetPrintButtonText();
        }

        public override async Task InitializeAsync()
        {
            this.Form.InitializeControlDict();

            EbFormHelper.Initialize(this.Form, this.Mode);

            //if (IsOffline())
            //{
            //    await Task.Run(() => this.Form.CreateTableSchema());
            //}

            if (this.Mode == FormMode.NEW || this.Mode == FormMode.PREFILL || this.Mode == FormMode.REF)
            {
                await ExpandContextIfConfigured();
                InitAutoId();
            }

            if (this.Mode == FormMode.NEW) SetValues();
        }

        protected virtual void SetValues()
        {
            if (this.Mode == FormMode.NEW)
                this.SetValuesFormContext();

            if (this.Mode == FormMode.NEW || this.Mode == FormMode.PREFILL || this.Mode == FormMode.REF)
                InitDefaultValueExpressions();

            InitOnLoadExpressions();
        }

        public async Task FormSubmitClicked(bool Print)
        {
            MsgLoader.IsVisible = true;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            if (!Utils.IsNetworkReady(this.NetworkType))
            {
                Utils.Alert_NoInternet();
                MsgLoader.IsVisible = false;
                return;
            }

            if (EbFormHelper.Validate())
            {
                EbLog.Info($"Form '{this.PageName}' validation success ready to submit");
                this.Form.NetworkType = this.NetworkType;
                await Submit(Print);
            }
            else
            {
                EbLog.Info($"Form '{this.PageName}' validation failed, some fields are required");
                Utils.Toast("Fields required");
            }
            sw.Stop();
            if (sw.ElapsedMilliseconds < 1500)
                await Task.Delay(1500 - (int)sw.ElapsedMilliseconds);
            MsgLoader.IsVisible = false;
        }

        protected virtual async Task Submit(bool Print)
        {
            try
            {
                FormSaveResponse response = await this.Form.Save(this.RowId, this.Page.RefId);

                if (response.Status && Print && !this.Form.RenderAsFilterDialog)
                    await this.Form.Print(response.PushResponse.RowId);

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

        private async Task ExpandContextIfConfigured()
        {
            if (this.Form.ContextToFormControlMap?.Count > 0)
            {
                bool tryOffline = false;
                if (this.Page.NetworkMode == NetworkMode.Online)
                {
                    if (!string.IsNullOrWhiteSpace(this.Form.ContextOnlineData))
                    {

                        if (!Utils.IsNetworkReady(this.NetworkType))
                        {
                            Utils.Alert_NoInternet();
                            return;
                        }

                        List<Param> cParams = new List<Param>();

                        cParams.Add(new Param
                        {
                            Name = "eb_loc_id",
                            Type = "11",
                            Value = App.Settings.CurrentLocation.LocId.ToString()
                        });

                        MobileDataResponse data = await DataService.Instance.GetDataAsync(this.Form.ContextOnlineData, 0, 0, cParams, null, null, false);

                        if (data.HasData() && data.TryGetFirstRow(1, out EbDataRow row))
                        {
                            this.PrefillData = row;
                            EbLog.Info("ContextOnlineData api returned valid data");
                        }
                        else
                            EbLog.Info("ContextOnlineData api returned empty row collection");
                    }
                    else
                        tryOffline = true;
                }
                if (this.Page.NetworkMode == NetworkMode.Offline || tryOffline)
                {
                    if (!string.IsNullOrWhiteSpace(this.Form.ContextOfflineData?.Code))
                    {
                        EbDataTable dt = App.DataDB.DoQuery(this.Form.ContextOfflineData.GetCode());
                        if (dt.Rows.Count > 0)
                        {
                            this.PrefillData = dt.Rows[0];
                            EbLog.Info("ContextOfflineData query returned valid data");
                        }
                    }
                }
            }
        }

        private void InitAutoId()
        {
            EbMobileAutoId ctrl = (EbMobileAutoId)Controls.Find(e => e is EbMobileAutoId);
            if (ctrl != null)
                ctrl.InitAutoId(Form.TableName);
        }

        protected void SetValuesFormContext()
        {
            if (this.PrefillData != null && this.Form.ContextToFormControlMap?.Count > 0)
            {
                foreach (var map in this.Form.ContextToFormControlMap)
                {
                    object value = this.PrefillData[map.ColumnName];

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
}
