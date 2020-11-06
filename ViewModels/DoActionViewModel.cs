using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Structures;
using ExpressBase.Mobile.ViewModels.BaseModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels
{
    public class DoActionViewModel : StaticBaseViewModel
    {
        private readonly IMyActionsService myActionService;
        private EbStageActions status;
        private string comments;
        private string stagename;
        private List<Param> actiondata;
        private List<EbStageActions> stageactions;
        private EbMyAction action;
        private EbStageInfo stageInfo;
        private readonly int actionId;

        public EbStageActions Status
        {
            get => this.status;
            set
            {
                this.status = value;
                this.NotifyPropertyChanged();
            }
        }

        public string Comments
        {
            get => this.comments;
            set
            {
                this.comments = value;
                this.NotifyPropertyChanged();
            }
        }

        public string StageName
        {
            get => stagename;
            set
            {
                stagename = value;
                this.NotifyPropertyChanged();
            }
        }

        public List<Param> ActionData
        {
            get => actiondata;
            set
            {
                actiondata = value;
                this.NotifyPropertyChanged();
            }
        }

        public List<EbStageActions> StageActions
        {
            get => stageactions;
            set
            {
                stageactions = value;
                this.NotifyPropertyChanged();
            }
        }

        public Command SubmitCommand => new Command(async () => await SubmitAction());

        public DoActionViewModel(EbMyAction myAction) : base(myAction.Description)
        {
            myActionService = new MyActionsService();
            action = myAction;
        }

        public DoActionViewModel(int actionid)
        {
            actionId = actionid;
            myActionService = new MyActionsService();
        }

        public override async Task InitializeAsync()
        {
            if (action == null && actionId > 0)
            {
                ParticularActionResponse particular = await myActionService.GetParticularActionAsync(actionId);
                action = particular.Action;
                stageInfo = particular.ActionInfo;
                this.PageTitle = action.Description;
            }
            else
                stageInfo = await myActionService.GetMyActionInfoAsync(action.StageId, action.WebFormRefId, action.WebFormDataId);

            if (stageInfo != null)
            {
                this.StageName = stageInfo.StageName;
                this.StageActions = stageInfo.StageActions;
                this.ActionData = stageInfo.Data;
            }
        }

        public async Task SubmitAction()
        {
            try
            {
                if (this.Status == null) return;

                WebformData webformData = new WebformData("eb_approval_lines");

                SingleRow row = new SingleRow
                {
                    LocId = App.Settings.CurrentLocId,
                    Columns =
                    {
                        new SingleColumn{ Name = "stage_unique_id", Type = (int)EbDbTypes.String, Value = stageInfo.StageUniqueId },
                        new SingleColumn{ Name = "action_unique_id", Type = (int)EbDbTypes.String, Value = this.Status.ActionUniqueId },
                        new SingleColumn{ Name = "eb_my_actions_id", Type = (int)EbDbTypes.Int32, Value = action.Id },
                        new SingleColumn{ Name = "comments", Type = (int)EbDbTypes.String, Value = this.Comments }
                    }
                };

                SingleTable st = new SingleTable { row };
                webformData.MultipleTables.Add("eb_approval_lines", st);

                await SendWebFormData(webformData, action.WebFormDataId, action.WebFormRefId);
                await App.Navigation.PopAsync(true);
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
        }

        private async Task SendWebFormData(WebformData webForm, int rowid, string webformRefid)
        {
            try
            {
                Device.BeginInvokeOnMainThread(() => IsBusy = true);

                PushResponse resp = await FormDataServices.Instance.SendFormDataAsync(webForm, rowid, webformRefid, App.Settings.CurrentLocId);

                Device.BeginInvokeOnMainThread(() => IsBusy = false);

                if (resp.RowAffected > 0)
                {
                    Utils.Toast("Action saved successfully :)");
                    App.Navigation.UpdateViewStack();
                }
                else
                    Utils.Toast("Unable to save action :( ");
            }
            catch (Exception ex)
            {
                EbLog.Info("Failed to submit form data in doaction");
                EbLog.Error(ex.Message);
                Device.BeginInvokeOnMainThread(() => IsBusy = false);
            }
        }
    }
}
