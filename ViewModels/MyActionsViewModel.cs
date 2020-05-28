using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels.BaseModels;
using ExpressBase.Mobile.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels
{
    public class MyActionsViewModel : StaticBaseViewModel
    {
        private readonly IMyActionsService myActionService;

        private bool _showEmptyLabel;

        public bool ShowEmptyLabel
        {
            get { return this._showEmptyLabel; }
            set
            {
                this._showEmptyLabel = value;
                this.NotifyPropertyChanged();
            }
        }

        public ObservableCollection<EbMyAction> _actions;

        public ObservableCollection<EbMyAction> Actions
        {
            get { return _actions; }
            set
            {
                _actions = value;
                NotifyPropertyChanged();
            }
        }

        public Command ItemSelectedCommand => new Command(async (obj) => await ItemSelected(obj));

        public MyActionsViewModel()
        {
            myActionService = new MyActionsService();
        }

        public override async Task InitializeAsync()
        {
            MyActionsResponse resp = await myActionService.GetMyActionsAsync();

            Actions = new ObservableCollection<EbMyAction>(resp.Actions);
            this.FillRandomColor(Actions);
            this.SetPageTitle();
        }

        public async Task RefreshMyActions()
        {
            try
            {
                MyActionsResponse actionResp = await myActionService.GetMyActionsAsync();
                FillRandomColor(actionResp.Actions);
                Actions.Clear();
                Actions.AddRange(actionResp.Actions);
                SetPageTitle();
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message);
            }
        }

        private void SetPageTitle()
        {
            PageTitle = $"My Actions ({Actions.Count})";
            if (Actions.Count <= 0)
                ShowEmptyLabel = true;
        }

        private async Task ItemSelected(object selected)
        {
            try
            {
                EbMyAction action = (EbMyAction)selected;

                if (Utils.HasInternet)
                {
                    IsBusy = true;
                    EbStageInfo stageInfo = await myActionService.GetMyActionInfoAsync(action.StageId, action.WebFormRefId, action.WebFormDataId);

                    if (stageInfo != null)
                    {
                        action.StageInfo = stageInfo;
                        await (Application.Current.MainPage as MasterDetailPage).Detail.Navigation.PushAsync(new DoAction(action));
                    }
                    IsBusy = false;
                }
                else
                    DependencyService.Get<IToast>().Show("You are not connected to internet");
            }
            catch (Exception ex)
            {
                Log.Write("AppSelect_ItemSelected---" + ex.Message);
            }
        }

        public void FillRandomColor(IEnumerable<EbMyAction> actions)
        {
            //fill by randdom colors
            Random random = new Random();
            foreach (EbMyAction action in actions)
            {
                var randomColor = ColorSet.Colors[random.Next(6)];
                action.BackgroundColor = Color.FromHex(randomColor.BackGround);
                action.TextColor = Color.FromHex(randomColor.TextColor);
            }
        }

        public async void DoActionPoped(bool isRowInserted)
        {
            if (isRowInserted) await RefreshMyActions();
        }
    }
}
