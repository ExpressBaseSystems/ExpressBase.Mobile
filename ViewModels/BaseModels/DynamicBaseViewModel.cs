using ExpressBase.Mobile.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ExpressBase.Mobile.ViewModels.BaseModels
{
    public class DynamicBaseViewModel : BaseViewModel
    {
        public View XView { set; get; }

        public EbMobilePage Page { set; get; }

        public EbDataTable ContextData { set; get; }

        public int ContextRecordIndex { set; get; }

        public NetworkMode NetworkType { set; get; }

        public DynamicBaseViewModel() { }

        public DynamicBaseViewModel(EbMobilePage page)
        {
            this.Page = page;
            this.PageTitle = page.DisplayName;
            this.NetworkType = page.NetworkMode;
        }

        public DynamicBaseViewModel(EbMobilePage page, EbDataTable contextData,int contextRI)
        {
            this.Page = page;
            this.PageTitle = page.DisplayName;
            this.NetworkType = page.NetworkMode;
            this.ContextData = contextData;
            this.ContextRecordIndex = contextRI;
        }
    }
}
