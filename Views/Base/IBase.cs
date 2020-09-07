using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressBase.Mobile.Views.Base
{
    public interface IRefreshable
    {
        void RefreshPage();

        void UpdateRenderStatus();

        bool CanRefresh();
    }
}
