﻿using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.ViewModels.Dynamic
{
    public class FormRenderVMR : FormRenderViewModel
    {
        private readonly EbDataRow contextRow;

        private readonly EbMobileVisualization context;

        public FormRenderVMR(EbMobilePage page, EbMobileVisualization source, EbDataRow contextrow) : base(page)
        {
            this.Mode = FormMode.REF;

            context = source;
            contextRow = contextrow;
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            this.SetValues();
        }

        protected override void SetValues()
        {
            if (context.ContextToControlMap == null)
            {
                EbLog.Warning($"column to control map empty in context visualization in page '{this.Page.DisplayName}'");
                return;
            };

            foreach (var map in context.ContextToControlMap)
            {
                object value = contextRow[map.ColumnName];

                if (this.Form.ControlDictionary.TryGetValue(map.ControlName, out EbMobileControl ctrl))
                {
                    if (ctrl is INonPersistControl || ctrl is ILinesEnabled)
                        continue;
                    else
                        ctrl.SetValue(value);
                }
            }
            base.SetValues();
        }
    }
}
