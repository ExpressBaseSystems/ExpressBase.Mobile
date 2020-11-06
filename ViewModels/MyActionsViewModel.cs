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

        private readonly IMyActionsService actionService;     

        public List<EbMyAction> actions;

        public List<EbMyAction> Actions
        {
            get => actions;
            set
            {
                actions = value;
                NotifyPropertyChanged();
            }
        }

        private const string title = "Action Required ";

        #endregion

        public Command RefreshListCommand => new Command(async () => await UpdateAsync());

        public Command ItemSelectedCommand => new Command<EbMyAction>(async (obj) => await ItemSelected(obj));

        public MyActionsViewModel()
        {
            actionService = new MyActionsService();
        }

        #region Methods

        public override async Task InitializeAsync()
        {
            MyActionsResponse resp = await actionService.GetMyActionsAsync();

            Actions = resp.Actions.Where(item => item.ActionType == MyActionTypes.Approval).ToList();

            this.SetPageTitle();
        }

        public override async Task UpdateAsync()
        {
            IsRefreshing = true;
            await InitializeAsync();
            IsRefreshing = false;
            Utils.Toast("Refreshed");
        }

        private void SetPageTitle()
        {
            PageTitle = $"{title}({Actions.Count})";
            IsEmpty = Actions.Count <= 0;
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
                    await App.Navigation.NavigateMasterAsync(new DoAction(action));
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
