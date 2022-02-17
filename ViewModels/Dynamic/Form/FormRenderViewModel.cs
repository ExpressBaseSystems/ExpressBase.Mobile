using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels.BaseModels;
using ExpressBase.Mobile.Views;
using ExpressBase.Mobile.Views.Dynamic;
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

        private bool IsTapped { get; set; }

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

        public Command SaveCommand => new Command(async () =>
        {
            if (IsTapped || EbPageHelper.IsShortTap())
            {
                EbLog.Warning($"More than 1 click in save: ({IsTapped}, {EbPageHelper.IsShortTap()})");
                return;
            }
            IsTapped = true;
            MsgLoader.IsVisible = true;
            MsgLoader.Message = "Saving record...";
            IsTapped = await FormSubmitClicked(false);
            MsgLoader.IsVisible = false;
        });

        public Command PrintCommand => new Command(async () =>
        {
            if (IsTapped || EbPageHelper.IsShortTap())
            {
                EbLog.Warning($"More than 1 click in print: ({IsTapped}, {EbPageHelper.IsShortTap()})");
                return;
            }
            IsTapped = true;
            MsgLoader.IsVisible = true;
            if (this.RowId > 0 || this.Form.RenderAsFilterDialog)
            {
                MsgLoader.Message = "Loading...";
                this.Form.NetworkType = this.NetworkType;//
                await this.Form.Print(this.RowId);
                IsTapped = false;
            }
            else
            {
                MsgLoader.Message = "Saving record...";
                IsTapped = await FormSubmitClicked(true);
            }
            MsgLoader.IsVisible = false;
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

        public async Task<bool> FormSubmitClicked(bool Print)
        {
            bool success = false;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            if (!Utils.IsNetworkReady(this.NetworkType))
            {
                Utils.Alert_NoInternet();
                return false;
            }

            string invalidMsg = EbFormHelper.Validate();
            if (invalidMsg == null)
            {
                EbLog.Info($"Form '{this.PageName}' validation success ready to submit");
                this.Form.NetworkType = this.NetworkType;
                success = await Submit(Print);
            }
            else
            {
                EbLog.Info($"Form '{this.PageName}' validation failed: " + invalidMsg);
                Utils.Toast(invalidMsg);
            }
            sw.Stop();
            if (invalidMsg == null && sw.ElapsedMilliseconds < 1500)
                await Task.Delay(1500 - (int)sw.ElapsedMilliseconds);
            return success;
        }

        protected virtual async Task<bool> Submit(bool Print)
        {
            bool success = false;
            try
            {
                bool pullAfter = false;
                string popupTitle = null, popupMsg = null;

                if (this.Form.AutoSyncOnLoad && this.NetworkType == NetworkMode.Online &&
                    !(Print || this.Form.RenderAsFilterDialog))
                {
                    pullAfter = true;
                    App.Settings.SyncInfo.PullSuccess = false;
                    await Store.SetJSONAsync(AppConst.LAST_SYNC_INFO, App.Settings.SyncInfo);
                }

                FormSaveResponse response = await this.Form.Save(this.RowId, this.Page.RefId);
                success = response.Status;

                if (response.Status && Print && !this.Form.RenderAsFilterDialog)
                {
                    await this.Form.Print(response.PushResponse.RowId);
                }

                if (response.Status && pullAfter)
                {
                    MsgLoader.Message = "Fetching data from server...";
                    SyncResponse resp = await App.Settings.GetSolutionDataAsyncV2(MsgLoader);
                    if (resp.Message != null)
                    {
                        popupTitle = resp.Status ? "Warning" : "Sync required";
                        popupMsg = resp.Message;
                    }
                }

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

                    if (popupTitle != null)
                    {
                        if (App.RootMaster.Detail is NavigationPage nav && nav.CurrentPage != null)
                        {
                            EbCPLayout layout = null;
                            if (nav.CurrentPage is Home hom)
                                layout = hom.GetCurrentLayout();
                            else if (nav.CurrentPage is FormRender fom)
                                layout = fom.GetCurrentLayout();
                            else if (nav.CurrentPage is ListRender lis)
                                layout = lis.GetCurrentLayout();
                            if (layout != null)
                                layout.ShowMessage(popupTitle, popupMsg);
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
            return success;
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
