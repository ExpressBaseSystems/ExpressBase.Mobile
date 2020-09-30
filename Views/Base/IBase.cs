using System.Collections.Generic;

namespace ExpressBase.Mobile.Views.Base
{
    public interface IRefreshable
    {
        void RefreshPage();

        void UpdateRenderStatus();

        bool CanRefresh();
    }

    public interface IToolBarHandler
    {
        bool DeviceBackButtonPressed();
    }

    public interface IDynamicContent
    {
        Dictionary<string, string> PageContent { get; }

        void SetContentFromConfig();
    }
}
