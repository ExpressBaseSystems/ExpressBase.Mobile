using ExpressBase.Mobile.CustomControls.Layout;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.ViewModels.BaseModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels.Dynamic.ListView
{
    public class StaticListViewModel : DynamicBaseViewModel
    {
        public EbMobileVisualization Visualization { set; get; }

        public SeparatorVisibility ShowRowSeperator => Visualization.XFSeperator();

        public List<EbMobileStaticListItem> Items { set; get; }

        public IntRef ListItemIndex { set; get; }

        private bool isSearchable;

        public bool IsSearchVisible
        {
            get => isSearchable;
            set
            {
                isSearchable = value;
                NotifyPropertyChanged();
            }
        }

        protected bool IsTapped { set; get; }

        public Command ItemTappedCommand => new Command<StaticLSFrame>(async (o) => await NavigateToLink(o));

        public Command SearchCommand => new Command<string>(async (query) => await SearchData(query));

        public StaticListViewModel(EbMobilePage page) : base(page)
        {
            Visualization = (EbMobileVisualization)page.Container;
            Items = Visualization.Items;

            InitSearchable();
        }

        private void InitSearchable()
        {
            if (Items == null) return;
        }

        private async Task NavigateToLink(StaticLSFrame item)
        {
            if (IsTapped)
                return;
            try
            {
                EbMobilePage page = EbPageFinder.GetPage(item.StaticItem.LinkRefId);

                if (page != null)
                {
                    IsTapped = true;
                    ContentPage renderer = EbPageFinder.GetPageByContainer(page);
                    if (renderer != null)
                        await App.RootMaster.Detail.Navigation.PushAsync(renderer);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            IsTapped = false;
        }

        public async Task SearchData(string search)
        {
            if (search == null)
                return;
        }
    }
}
