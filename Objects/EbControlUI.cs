using System.ComponentModel;

namespace ExpressBase.Mobile
{
    public class EbControlUI : EbControl
    {
        public virtual string BackColor { get; set; }

        public virtual string ForeColor { get; set; }

        public virtual string LabelBackColor { get; set; }

        public virtual UISides Margin { get; set; }

        public virtual string LabelForeColor { get; set; }

        public virtual float FontSize { get; set; }
    }

    public class UISides
    {
        public UISides() { }

        public virtual int Top { get; set; }

        public virtual int Right { get; set; }

        public virtual int Bottom { get; set; }

        public virtual int Left { get; set; }
    }
}
