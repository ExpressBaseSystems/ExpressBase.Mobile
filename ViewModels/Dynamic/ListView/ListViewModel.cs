using ExpressBase.Mobile.Data;
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
