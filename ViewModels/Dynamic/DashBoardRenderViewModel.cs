using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.ViewModels.BaseModels;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels.Dynamic
{
    public class DashBoardRenderViewModel : DynamicBaseViewModel
    {
        private EbMobileDashBoard DashBoard { set; get; }

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

            foreach(EbMobileDashBoardControls ctrl in this.DashBoard.ChildControls)
            {
                ctrl.InitXControl(LinkedDataRow);
                stack.Children.Add(ctrl.XView);
            }

            XView = stack;
        }
    }
}
