using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Views.Dynamic;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.ViewModels.Dynamic
{
    public class ListViewModel : ListViewBaseVM
    {
        public ListViewModel(EbMobilePage page) : base(page) { }

        public ListViewModel(EbMobilePage page, EbDataRow row) : base(page)
        {
            this.ContextRecord = row;
        }

        public override async Task InitializeAsync()
        {
            if (ContextRecord != null)
            {
                ContextParams = this.Visualization.GetContextParams(ContextRecord, this.NetworkType);
            }
            await this.SetDataAsync();
        }

        protected override async Task NavigateToLinkForm()
        {
            EbMobilePage page = EbPageFinder.GetPage(Visualization.LinkRefId);

            if (page != null && page.Container is EbMobileForm)
            {
                FormRender Renderer = new FormRender(page);
                await App.RootMaster.Detail.Navigation.PushAsync(Renderer);
            }
        }

        protected override List<DbParameter> GetFilterParameters()
        {
            List<DbParameter> temp;

            if (ContextParams != null)
                temp = FilterParams == null ? ContextParams : ContextParams.Union(FilterParams).ToList();
            else
                temp = FilterParams;

            return temp;
        }
    }
}
