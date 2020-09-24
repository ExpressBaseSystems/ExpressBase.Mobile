using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels.BaseModels;
using ExpressBase.Mobile.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels
{
    public class MyActionsViewModel : StaticBaseViewModel
    {
        #region Properties

        private readonly IMyActionsService myActionService;

        private bool _showEmptyLabel;

        public bool ShowEmptyLabel
        {
            get => this._showEmptyLabel;
            set
            {
                this._showEmptyLabel = value;
                this.NotifyPropertyChanged();
            }
        }

        public List<EbMyAction> _actions;

        public List<EbMyAction> Actions
        {
            get => _actions;
            set
            {
                _actions = value;
                NotifyPropertyChanged();
            }
        }

        private const string title = "My Actions ";

        #endregion

        public Command ItemSelectedCommand => new Command<EbMyAction>(async (obj) => await ItemSelected(obj));

        public MyActionsViewModel()
        {
            myActionService = new MyActionsService();
        }

        #region Methods

        public override async Task InitializeAsync()
        {
            MyActionsResponse resp = await myActionService.GetMyActionsAsync();

            Actions = resp.Actions.Where(item => item.ActionType == MyActionTypes.Approval).ToList();

            this.SetPageTitle();
        }

        private void SetPageTitle()
        {
            PageTitle = $"{title}({Actions.Count})";
            ShowEmptyLabel = Actions.Count <= 0;
        }

        private bool IsTapped;

        private async Task ItemSelected(EbMyAction action)
        {
            if (IsTapped || action == null)
                return;

            try
            {
                if (Utils.HasInternet)
                {
                    IsTapped = true;
                    await App.RootMaster.Detail.Navigation.PushAsync(new DoAction(action));
                }
                else
                    Utils.Alert_NoInternet();
            }
            catch (Exception ex)
            {
                EbLog.Info("Error at Item selected in myactions");
                EbLog.Error(ex.Message);
            }
            IsTapped = false;
        }

        #endregion
    }
}
