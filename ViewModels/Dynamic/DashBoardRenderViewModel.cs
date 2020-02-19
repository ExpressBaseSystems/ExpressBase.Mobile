using ExpressBase.Mobile.Data;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels.Dynamic
{
    public class DashBoardRenderViewModel : BaseViewModel
    {
        private EbMobileDashBoard DashBoard { set; get; }

        public View View { set; get; }

        public EbDataRow LinkedDataRow { set; get; }

        public DashBoardRenderViewModel() { }

        public DashBoardRenderViewModel(EbMobilePage page) : base(page)
        {
            DashBoard = this.Page.Container as EbMobileDashBoard;
            CreateView();
        }

        public DashBoardRenderViewModel(EbMobilePage Page, EbDataRow Row)
        {
            PageTitle = Page.DisplayName;
            DashBoard = Page.Container as EbMobileDashBoard;
            LinkedDataRow = Row;
            CreateView();
        }

        private void CreateView()
        {
            StackLayout stack = new StackLayout();

            foreach(EbMobileDashBoardControls ctrl in this.DashBoard.ChiledControls)
            {
                ctrl.InitXControl(LinkedDataRow);
                stack.Children.Add(ctrl.XView);
            }

            View = stack;
        }
    }
}
