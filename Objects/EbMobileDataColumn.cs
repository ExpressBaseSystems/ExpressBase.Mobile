using ExpressBase.Mobile.Structures;

namespace ExpressBase.Mobile
{
    public class EbMobileDataColumn : EbMobileControl, INonPersistControl, IGridAlignment, IMobileUIControl
    {
        public int ColumnIndex { get; set; }

        public string ColumnName { get; set; }

        public EbDbTypes Type { get; set; }

        public DataColumnRenderType RenderAs { set; get; }

        public string BackgroundColor { get; set; }

        public int BorderRadius { get; set; }

        public int BorderThickness { get; set; }

        public string BorderColor { get; set; }

        public string TextFormat { get; set; }

        public MobileTextWrap TextWrap { set; get; }

        public EbFont Font { get; set; }

        public int RowSpan { set; get; }

        public int ColumnSpan { set; get; }

        public MobileHorrizontalAlign HorrizontalAlign { set; get; }

        public MobileVerticalAlign VerticalAlign { set; get; }

        public EbThickness Padding { set; get; }

        public int Height { set; get; }

        public int Width { set; get; }

        public bool HideInContext { set; get; }

        public string GetContent(object value)
        {
            if (!string.IsNullOrEmpty(TextFormat))
                return TextFormat.Replace("{value}", value?.ToString());
            else
                return value?.ToString();
        }
    }
}
