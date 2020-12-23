using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.CustomControls;
using ExpressBase.Mobile.Data;
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

        public virtual string BackgroundColor { set; get; }

        public virtual bool BoxShadow { set; get; }

        protected EbXFrame GetFrame()
        {
            var frame = new EbXFrame
            {
                BackgroundColor = Color.FromHex(this.BackgroundColor),
                HasShadow = this.BoxShadow,
                CornerRadius = this.BorderRadius,
                Padding = this.Padding == null ? 0 : this.Padding.ConvertToXValue(),
                Margin = this.Margin == null ? 0 : this.Margin.ConvertToXValue(),
                BorderWidth = this.BorderThickness,
                BorderColor = Color.FromHex(BorderColor)
            };

            return frame;
        }

        public virtual View Draw() { return null; }

        public object GetBinding(EbDataRow row, string bindingParam)
        {
            string bindingColumn = GetBindingColumn(bindingParam);

            if (!string.IsNullOrEmpty(bindingColumn))
            {
                return row[bindingColumn];
            }
            return null;
        }

        private string GetBindingColumn(string binding)
        {
            string[] parts = binding.Split(CharConstants.DOT);

            if (parts.Length >= 2)
            {
                return parts[1];
            }
            return null;
        }

        public virtual void SetBindingValue(EbDataRow row) { }
    }
}
