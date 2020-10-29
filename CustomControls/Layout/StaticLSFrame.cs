using ExpressBase.Mobile.CustomControls.XControls;
using ExpressBase.Mobile.Extensions;
using Xamarin.Forms;

namespace ExpressBase.Mobile.CustomControls.Layout
{
    public class StaticLSFrame : DynamicFrame
    {
        public EbMobileStaticListItem StaticItem { set; get; }

        public StaticLSFrame(EbMobileStaticListItem item, EbMobileVisualization viz, bool isHeader = false)
        {
            StaticItem = item;
            IsHeader = isHeader;
            Initialize(viz);
        }

        protected override View ResolveControlType(EbMobileControl ctrl)
        {
            View view = null;

            if (ctrl is EbMobileLabel label)
            {
                EbXLabel xlabel = label.CreateXControl();

                if (label.RenderAsIcon)
                {
                    string icon = (label.BindingParam == null ? label.Icon : GetStaticData(label)) ?? "f128";
                    xlabel.Text = icon.ToFontIcon();
                }
                else
                {
                    xlabel.Text = GetStaticData(label);
                }
                ApplyLabelStyle(xlabel, label.Font);
                view = xlabel;
            }

            return view;
        }

        private string GetStaticData(EbMobileLabel label)
        {
            if (label.BindingParam == null || this.StaticItem.Parameters == null)
                return label.Text;

            EbMobileStaticParameter param = this.StaticItem.Parameters.Find(x => x.Name == label.BindingParam.Name);

            return param?.Value;
        }
    }
}
