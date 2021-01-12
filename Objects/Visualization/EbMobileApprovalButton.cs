using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Views;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbMobileApprovalButton : EbMobileButton
    {
        public string FormRefid { get; set; }

        public override View Draw(EbDataRow row)
        {
            Button xbutton = (Button)base.Draw();

            string columnName = Name + "_stage_name";

            object stageName = row[columnName];

            if (stageName != null)
                xbutton.Text = stageName.ToString();
            else
                xbutton.IsVisible = false;

            return xbutton;
        }

        public override async Task OnControlAction(EbDataRow row)
        {
            string columnName = Name + "_action_id";

            object actionId = row[columnName];

            if (actionId != null)
            {
                int id = Convert.ToInt32(actionId);

                if (id == 0)
                {
                    EbLog.Info("action_id is zero");
                    return;
                }
                await App.Navigation.NavigateMasterAsync(new DoAction(id));
            }
        }
    }
}
