using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Services;
using ExpressBase.Mobile.ViewModels.BaseModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressBase.Mobile.ViewModels
{
    class MyActionsViewModel : StaticBaseViewModel
    {
        public List<EbMyAction> Actions { get; set; }

        public MyActionsViewModel(MyActionsResponse actionResp)
        {
            Actions = actionResp.Actions;
        }
    }
}
