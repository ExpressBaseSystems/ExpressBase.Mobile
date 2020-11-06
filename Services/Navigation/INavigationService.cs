using ExpressBase.Mobile.Models;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Services.Navigation
{
    public interface INavigationService
    {
        Task InitializeAppAsync(EbNFData payload);

        Task NavigateAsync(Page page);

        Task NavigateModalAsync(Page page);

        Task NavigateMasterAsync(Page page);

        Task NavigateMasterModalAsync(Page page);

        Task NavigateToLogin(bool new_navigation = false);

        Task PopAsync(bool animate);

        Task PopModalAsync(bool animate);

        Task PopMasterAsync(bool animate);

        Task PopMasterModalAsync(bool animate);

        void UpdateViewStack();

        void RefreshCurrentPage();
    }
}
