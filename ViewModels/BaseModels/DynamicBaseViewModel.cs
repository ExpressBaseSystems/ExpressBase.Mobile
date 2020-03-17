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

        public ColumnColletion ContextColumns { set; get; }

        public EbDataRow ContextRow { set; get; }

        public NetworkMode NetworkType { set; get; }

        public DynamicBaseViewModel() { }

        public DynamicBaseViewModel(EbMobilePage page)
        {
            this.Page = page;
            this.PageTitle = page.DisplayName;
            this.NetworkType = page.NetworkMode;
        }

        public DynamicBaseViewModel(EbMobilePage page, ColumnColletion contextCollumn, EbDataRow contextRow)
        {
            this.Page = page;
            this.PageTitle = page.DisplayName;
            this.NetworkType = page.NetworkMode;
            this.ContextColumns = contextCollumn;
            this.ContextRow = contextRow;
        }
    }
}
