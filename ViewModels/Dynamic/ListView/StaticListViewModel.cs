using ExpressBase.Mobile.ViewModels.BaseModels;
using System.Collections.Generic;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels.Dynamic.ListView
{
    public class StaticListViewModel : DynamicBaseViewModel
    {
        public EbMobileVisualization Visualization { set; get; }

        public SeparatorVisibility ShowRowSeperator => Visualization.XFSeperator();

        public List<EbMobileStaticListItem> Items { set; get; }

        public StaticListViewModel(EbMobilePage page) : base(page)
        {
            Visualization = (EbMobileVisualization)page.Container;
            Items = Visualization.Items;
        }
    }
}
