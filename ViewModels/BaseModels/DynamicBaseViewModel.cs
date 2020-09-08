using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels.BaseModels
{
    public class DynamicBaseViewModel : BaseViewModel
    {
        public View XView { set; get; }

        public EbMobilePage Page { set; get; }

        public NetworkMode NetworkType { set; get; }

        public string PageName => this.Page.DisplayName;

        public DynamicBaseViewModel() { }

        public DynamicBaseViewModel(EbMobilePage page)
        {
            this.Page = page;
            this.PageTitle = page.DisplayName;
            this.NetworkType = page.NetworkMode;
        }
    }
}
