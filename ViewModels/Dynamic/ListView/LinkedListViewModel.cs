using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Views.Dynamic;
using ExpressBase.Mobile.Views.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels.Dynamic
{
    public class LinkedListViewModel : ListViewBaseViewModel
    {
        public EbMobileVisualization Context { set; get; }

        public bool ContextVisibilty => !this.Visualization.HideContext;

        public Command EditCommand => new Command(async () => await EditButtonClicked());

        public LinkedListViewModel(EbMobilePage page, EbMobileVisualization context, EbDataRow row) : base(page)
        {
            this.Context = context;
            this.ContextRecord = row;
        }

        public override async Task InitializeAsync()
        {
            ContextParams = this.Visualization.GetContextParams(ContextRecord, this.NetworkType);
            await GetDataAsync();
        }

        protected override async Task NavigateToFabLink()
        {
            if (IsTapped || EbPageHelper.IsShortTap())
                return;
            IsTapped = true;
            Loader msgLoader = EbLayout.GetMessageLoader();
            msgLoader.IsVisible = true;
            msgLoader.Message = "Loading...";
            string linkRefID = Visualization.UseLinkSettings ? Visualization.LinkRefId : Visualization.FabLinkRefId;

            EbMobilePage page = EbPageHelper.GetPage(linkRefID);

            if (page != null && page.Container is EbMobileForm form)
            {
                form.NetworkType = page.NetworkMode;
                string failMsg = await EbPageHelper.ValidateFormRendering(form, msgLoader, this.ContextRecord);

                if (failMsg == null)
                    await App.Navigation.NavigateMasterAsync(new FormRender(page, Visualization, ContextRecord));
                else
                    await App.Navigation.NavigateMasterAsync(new Redirect(failMsg));
            }
            msgLoader.IsVisible = false;
            IsTapped = false;
        }

        private async Task EditButtonClicked()
        {
            EbMobilePage page = EbPageHelper.GetPage(Context.SourceFormRefId);

            if (page != null)
            {
                int id = Convert.ToInt32(ContextRecord["id"]);
                if (id != 0)
                {
                    FormRender Renderer = new FormRender(page, id);
                    await App.Navigation.NavigateMasterAsync(Renderer);
                }
            }
        }

        protected override List<DbParameter> GetFilterParameters()
        {
            return FilterParams == null ? ContextParams : ContextParams.Union(FilterParams).ToList();
        }
    }
}
