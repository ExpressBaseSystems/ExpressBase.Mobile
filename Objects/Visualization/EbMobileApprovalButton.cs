using ExpressBase.Mobile.Data;
using System.Threading.Tasks;

namespace ExpressBase.Mobile
{
    public class EbMobileApprovalButton : EbMobileButton
    {
        public string FormRefid { get; set; }

        public string StageColumn { set; get; }

        public override async Task OnControlAction(EbDataRow row)
        {

        }
    }
}
