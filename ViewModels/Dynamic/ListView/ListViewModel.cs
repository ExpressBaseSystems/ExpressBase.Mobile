using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Views.Dynamic;
using ExpressBase.Mobile.Views.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

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
            await SetDataAsync();
        }

        protected override async Task NavigateToLinkForm()
        {
            EbMobilePage page = EbPageFinder.GetPage(Visualization.LinkRefId);

            if (page != null && page.Container is EbMobileForm form)
            {
                Device.BeginInvokeOnMainThread(() => IsBusy = true);
                bool validation = await EbPageFinder.ValidateFormRendering(form, this.ContextRecord);
                Device.BeginInvokeOnMainThread(() => IsBusy = false);

                if (validation)
                    await App.RootMaster.Detail.Navigation.PushAsync(new FormRender(page));
                else
                    await App.RootMaster.Detail.Navigation.PushAsync(new Redirect(form.MessageOnFailed));
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
