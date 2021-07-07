using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers;
using System;
using System.Linq;
using Xamarin.Forms;

namespace ExpressBase.Mobile
{
    public class EbMobileDashBoardControl : EbMobilePageBase
    {
        public EbThickness Margin { set; get; }

        public EbThickness Padding { set; get; }

        public int BorderThickness { get; set; }

        public virtual int BorderRadius { get; set; }

        public virtual string BorderColor { set; get; }

        public bool Transparent { get; set; }

        public virtual string BackgroundColor { set; get; }

        public virtual bool BoxShadow { set; get; }

        public virtual bool Hidden { set; get; }

        protected EbXFrame GetFrame()
        {
            var frame = new EbXFrame
            {
                BackgroundColor = Color.FromHex(this.BackgroundColor),
                HasShadow = BoxShadow,
                CornerRadius = BorderRadius,
                Padding = this.Padding == null ? 0 : this.Padding.ConvertToXValue(),
                Margin = this.Margin == null ? 0 : this.Margin.ConvertToXValue(),
                BorderWidth = BorderThickness,
                BorderColor = Color.FromHex(BorderColor),
                IsVisible = !Hidden
            };

            return frame;
        }

        public virtual View Draw() { return null; }

        public object GetBinding(EbDataSet dataSet, string bindingParam)
        {
            try
            {
                string[] parts = bindingParam.Split(CharConstants.DOT);

                if (parts.Length >= 2)
                {
                    string columnName = parts[1];
                    string tableExpr = parts[0];

                    int tableIndex = Convert.ToInt32(tableExpr.Substring(tableExpr.Length - 1));

                    if (dataSet.TryGetTable(tableIndex, out EbDataTable dt))
                    {
                        EbDataRow dr = dt.Rows?.FirstOrDefault();

                        if (dr != null)
                        {
                            return dr[columnName];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                EbLog.Error("dashboard control [GetBinding] error, " + ex.Message);
            }
            return null;
        }

        public virtual void SetBindingValue(EbDataSet dataSet) { }
    }
}
