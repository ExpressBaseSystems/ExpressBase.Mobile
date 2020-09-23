
using Xamarin.Forms;

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
}
