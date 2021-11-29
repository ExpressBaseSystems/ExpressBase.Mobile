using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.ViewModels.Dynamic
{
    public class FormRenderVMPRE : FormRenderViewModel
    {
        private readonly List<EbMobileDataColToControlMap> linkFormParameters;

        public FormRenderVMPRE(EbMobilePage page, List<EbMobileDataColToControlMap> linkMap, EbDataRow contextrow) : base(page)
        {
            this.Mode = FormMode.PREFILL;

            linkFormParameters = linkMap;
            Context = contextrow;
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            this.SetValues();
        }

        protected override void SetValues()
        {
            if (this.linkFormParameters == null)
            {
                EbLog.Info($"linkFormParameters empty in '{this.Page.DisplayName}'");
                return;
            }

            foreach (EbMobileDataColToControlMap map in this.linkFormParameters)
            {
                object value = Context[map.ColumnName];

                if (map.FormControl == null)
                {
                    EbLog.Info($"form control not found for column name {map.ColumnName} in page '{this.Page.DisplayName}'");
                    continue;
                }

                if (this.Form.ControlDictionary.TryGetValue(map.FormControl.ControlName, out EbMobileControl ctrl))
                {
                    ctrl.SetValue(value);
                }
            }

            base.SetValues();
        }
    }
}
