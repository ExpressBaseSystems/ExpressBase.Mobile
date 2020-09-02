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

        private string actionCount;

        public string ActionsCount
        {
            get => actionCount;
            set
            {
                actionCount = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        public Command ItemSelectedCommand => new Command(async (obj) => await ItemSelected(obj));

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
            ActionsCount = $"({Actions.Count})";
            ShowEmptyLabel = Actions.Count <= 0;
        }

        private async Task ItemSelected(object selected)
        {
            try
            {
                EbMyAction action = (EbMyAction)selected;

                if (Utils.HasInternet)
                {
                    await App.RootMaster.Detail.Navigation.PushAsync(new DoAction(action));
                }
                else
                    Utils.Alert_NoInternet();
            }
            catch (Exception ex)
            {
                EbLog.Error("AppSelect_ItemSelected---" + ex.Message);
            }
        }

        #endregion
    }
}
