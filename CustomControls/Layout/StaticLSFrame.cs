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

        protected override View GetViewByRenderType(EbMobileControl ctrl)
        {
            View view = null;

            if (ctrl is EbMobileLabel label)
            {
                EbXLabel xlabel = label.Draw();

                if (label.RenderAsIcon)
                {
                    string icon = (label.BindingParam == null ? label.Icon : GetStaticData(label)) ?? "f128";
                    xlabel.Text = icon.ToFontIcon();
                }
                else
                {
                    xlabel.Text = GetStaticData(label);
                }
                xlabel.SetFont(label.Font);
                xlabel.SetTextWrap(label.TextWrap);
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
