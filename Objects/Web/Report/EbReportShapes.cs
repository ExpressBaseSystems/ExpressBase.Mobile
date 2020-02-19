using System.Collections.Generic;

namespace ExpressBase.Mobile
{
    public abstract class EbReportFieldShape : EbReportField
    {
        public override EbTextAlign TextAlign { get; set; }

        public override string Title { get; set; }

        public override string ForeColor { get; set; }

        public override EbFont Font { get; set; }
    }

    public class EbCircle : EbReportFieldShape
    {
    }

    public class EbRect : EbReportFieldShape
    {
    }

    public class EbArrR : EbHl
    {
    }

    public class EbArrL : EbHl
    {
    }

    public class EbArrD : EbVl
    {

    }

    public class EbArrU : EbVl
    {
    }

    public class EbByArrH : EbHl
    {
    }

    public class EbByArrV : EbVl
    {
    }

    public class EbHl : EbReportFieldShape
    {

    }

    public class EbVl : EbReportFieldShape
    {
    }

    public class EbTable_Layout : EbReportFieldShape
    {
        public int ColoumCount { get; set; }

        public int RowCount { get; set; }

        public List<EbTableLayoutCell> CellCollection { get; set; }
    }
}
