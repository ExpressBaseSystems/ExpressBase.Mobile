using ExpressBase.Mobile.Common.Structures;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Views.Shared;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Views
{
    class RenderMaster : MasterDetailPage
    {
        SideBar Sidebar;

        public RenderMaster(EbObjectWrapper wraper)
        {
            Sidebar = new SideBar();
            Master = Sidebar;
            
            if (wraper.EbObjectType == (int)EbObjectTypes.WebForm)
            {
                Detail = new NavigationPage(new FormRender(wraper));
            }
        }
    }
}
