﻿using ExpressBase.Mobile.CustomControls;
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
    public class ListViewModel : ListViewBaseViewModel
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
                    await App.Navigation.NavigateMasterAsync(new FormRender(page));
                else
                    await App.Navigation.NavigateMasterAsync(new Redirect(failMsg));
            }
            msgLoader.IsVisible = false;
            IsTapped = false;
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
