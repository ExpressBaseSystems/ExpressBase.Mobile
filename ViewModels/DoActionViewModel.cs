using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.Structures;
using ExpressBase.Mobile.ViewModels.BaseModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels
{
    public class DoActionViewModel : StaticBaseViewModel
    {
        private EbStageActions _status;

        public EbStageActions Status
        {
            get { return this._status; }
            set
            {
                this._status = value;
                this.NotifyPropertyChanged();
            }
        }

        private string _comments;

        public string Comments
        {
            get { return this._comments; }
            set
            {
                this._comments = value;
                this.NotifyPropertyChanged();
            }
        }

        public List<Param> ActionData { set; get; }

        public EbMyAction Action { set; get; }

        public List<EbStageActions> StageActions { set; get; }

        public Command SubmitCommand => new Command(async () => await SubmitButton_Clicked());

        public DoActionViewModel(EbMyAction myAction)
        {
            PageTitle = myAction.Description;
            Action = myAction;

            if (Action.StageInfo != null)
            {
                StageActions = Action.StageInfo.StageActions ?? new List<EbStageActions>();
                ActionData = Action.StageInfo.Data ?? new List<Param>();
            }
        }

        public async Task SubmitButton_Clicked()
        {
            try
            {
                EbStageActions status = this.Status;
                if (status == null) return;

                string comment = this.Comments;
                int locid = App.Settings.CurrentLocId;

                Device.BeginInvokeOnMainThread(() => IsBusy = true);

                WebformData webformData = new WebformData("eb_approval_lines");

                SingleRow row = new SingleRow
                {
                    LocId = locid,
                    Columns =
                    {
                        new SingleColumn{ Name = "stage_unique_id", Type = (int)EbDbTypes.String, Value = this.Action.StageInfo.StageUniqueId },
                        new SingleColumn{ Name = "action_unique_id", Type = (int)EbDbTypes.String, Value = status.ActionUniqueId },
                        new SingleColumn{ Name = "eb_my_actions_id", Type = (int)EbDbTypes.Int32, Value = this.Action.Id },
                        new SingleColumn{ Name = "comments", Type = (int)EbDbTypes.String, Value = comment }
                    }
                };

                SingleTable st = new SingleTable { row };
                webformData.MultipleTables.Add("eb_approval_lines", st);

                PushResponse resp = await FormDataServices.Instance.SendFormDataAsync(webformData, this.Action.WebFormDataId, this.Action.WebFormRefId, locid);

                Device.BeginInvokeOnMainThread(() => IsBusy = false);

                IToast helper = DependencyService.Get<IToast>();
                if (resp.RowAffected > 0)
                    helper.Show("Action saved successfully :)");
                else
                    helper.Show("Unable to save action :( ");

                await (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PopAsync(true);
            }
            catch (Exception ex)
            {
                EbLog.Write(ex.Message);
            }
        }
    }
}
