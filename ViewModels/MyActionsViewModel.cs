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
        public ObservableCollection<EbMyAction> Actions { get; set; }

        public Command ItemSelectedCommand => new Command(async (obj) => await ItemSelected(obj));

        public MyActionsViewModel(MyActionsResponse actionResp)
        {
            Actions = new ObservableCollection<EbMyAction>(actionResp.Actions);
            PageTitle = $"My Actions ({Actions.Count})";
            this.FillRandomColor();
        }

        private async Task ItemSelected(object selected)
        {
            try
            {
                if (selected == null) return;

                EbMyAction action = selected as EbMyAction;

                if (Settings.HasInternet)
                {
                    IsBusy = true;
                    EbStageInfo stageInfo = await RestServices.Instance.GetMyActionInfoAsync(action.StageId, action.WebFormRefId, action.WebFormDataId);

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

        public void FillRandomColor()
        {
            //fill by randdom colors
            Random random = new Random();
            foreach (EbMyAction action in this.Actions)
            {
                var randomColor = ColorSet.Colors[random.Next(6)];
                action.BackgroundColor = Color.FromHex(randomColor.BackGround);
                action.TextColor = Color.FromHex(randomColor.TextColor);
            }
        }
    }
}
