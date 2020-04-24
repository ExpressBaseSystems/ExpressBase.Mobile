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

        public View DataView { set; get; }

        public List<Param> ActionData { set; get; }

        public EbMyAction Action { set; get; }

        public List<EbStageActions> StageActions { set; get; }

        public Command SubmitCommand { set; get; }

        private readonly MyActionsViewModel MyActionVM;

        public DoActionViewModel(EbMyAction myAction, MyActionsViewModel myActionVM)
        {
            PageTitle = myAction.Description;
            Action = myAction;
            MyActionVM = myActionVM;

            if (Action.StageInfo != null)
            {
                StageActions = Action.StageInfo.StageActions ?? new List<EbStageActions>();
                ActionData = Action.StageInfo.Data ?? new List<Param>();
                BuildView();
            }
            SubmitCommand = new Command(async () => await SubmitButton_Clicked());
        }

        private void BuildView()
        {
            try
            {
                Grid gd = new Grid
                {
                    RowSpacing = 1,
                    ColumnSpacing = 1,
                    ColumnDefinitions =
                    {
                        new ColumnDefinition{ Width = GridLength.Star },
                        new ColumnDefinition{ Width = GridLength.Star },
                    }
                };

                int rowCounter = 0;
                foreach (var p in this.ActionData)
                {
                    if (string.IsNullOrEmpty(p.Name)) continue;
                    gd.RowDefinitions.Add(new RowDefinition());
                    gd.Children.Add(new Label
                    {
                        Padding = 8,
                        Text = p.Name,
                        BackgroundColor = Color.White,
                        VerticalOptions = LayoutOptions.FillAndExpand,
                        FontFamily = (OnPlatform<string>)HelperFunctions.GetResourceValue("Roboto-Regular")
                    }, 0, rowCounter);
                    gd.Children.Add(new Label
                    {
                        Padding = 8,
                        Text = p.Value,
                        BackgroundColor = Color.White,
                        VerticalOptions = LayoutOptions.FillAndExpand
                    }, 1, rowCounter);
                    rowCounter++;
                }
                DataView = gd;
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }

        public async Task SubmitButton_Clicked()
        {
            try
            {
                EbStageActions status = this.Status;
                if (status == null) return;

                string comment = this.Comments;
                int locid = Settings.LocationId;

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

                PushResponse resp = await RestServices.Instance.PushAsync(webformData, this.Action.WebFormDataId, this.Action.WebFormRefId, locid);

                Device.BeginInvokeOnMainThread(() => IsBusy = false);

                IToast helper = DependencyService.Get<IToast>();
                if (resp.RowAffected > 0)
                    helper.Show("Action saved successfully :)");
                else
                    helper.Show("Unable to save action :( ");

                await (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PopAsync(true);
                if (MyActionVM != null) MyActionVM.DoActionPoped(resp.RowAffected > 0);
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }
    }
}
