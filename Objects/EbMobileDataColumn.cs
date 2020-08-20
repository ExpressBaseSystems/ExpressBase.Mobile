using ExpressBase.Mobile.Structures;

namespace ExpressBase.Mobile
{
    public class EbMobileDataColumn : EbMobileControl, INonPersistControl, IMobileDataPart
    {
        public int TableIndex { get; set; }

        public int ColumnIndex { get; set; }

        public string ColumnName { get; set; }

        public EbDbTypes Type { get; set; }

        public DataColumnRenderType RenderAs { set; get; }

        public string TextFormat { get; set; }

        public EbFont Font { get; set; }

        public int RowSpan { set; get; }

        public int ColumnSpan { set; get; }

        public MobileHorrizontalAlign HorrizontalAlign { set; get; }

        public MobileVerticalAlign VerticalAlign { set; get; }

        public string GetContent(object value)
        {
            if (!string.IsNullOrEmpty(TextFormat))
                return TextFormat.Replace("{value}", value?.ToString());
            else
                return value?.ToString();
        }

        public override void InitXControl()
        {
            base.InitXControl();
        }
    }
}
