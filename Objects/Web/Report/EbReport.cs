using System.Collections.Generic;
namespace ExpressBase.Mobile
{
    public enum EbReportSectionType
    {
        ReportHeader,
        PageHeader,
        Detail,
        PageFooter,
        ReportFooter,
        ReportGroups
    }
    public enum PaperSize
    {
        A2,
        A3,
        A4,
        A5,
        Letter,
        Custom
    }
    public enum SummaryFunctionsText
    {
        Count,
        Max,
        Min
    }
    public enum EbTextAlign
    {
        Left = 0,
        Center = 1,
        Right = 2,
        Justify = 3,
        Top = 4,
        Middle = 5,
        Bottom = 6,
        Baseline = 7,
        JustifiedAll = 8,
        Undefined = -1
    }

    public enum DateFormatReport
    {
        M_d_yyyy,
        MM_dd_yyyy,
        ddd_MMM_d_yyyy,
        dddd_MMMM_d_yyyy,
        MM_dd_yy,
        dd_MM_yyyy,
        dd_MM_yyyy_slashed,
        from_culture,
        dd_MMMM_yyyy
        //Year_Month_Date,
        //Year_Month,
        //Year,
    }

    public enum SummaryFunctionsNumeric
    {
        Average,
        Count,
        Max,
        Min,
        Sum
    }
    public enum SummaryFunctionsDateTime
    {
        Count,
        Max,
        Min
    }
    public enum SummaryFunctionsBoolean
    {
        Count
    }

    public class Margin
    {
        public float Left { get; set; }

        public float Right { get; set; }

        public float Top { get; set; }

        public float Bottom { get; set; }
    }

    public class EbReport : EbReportObject
    {
        public PaperSize PaperSize { get; set; }

        public float CustomPaperHeight { get; set; }

        public float CustomPaperWidth { get; set; }

        public Margin Margin { get; set; }

        public float DesignPageHeight { get; set; }

        public string UserPassword { get; set; }

        public string OwnerPassword { get; set; }

        public bool IsLandscape { get; set; }

        public string BackgroundImage { get; set; }

        public List<EbReportField> ReportObjects { get; set; }

        public List<EbReportHeader> ReportHeaders { get; set; }

        public List<EbReportFooter> ReportFooters { get; set; }

        public List<EbPageHeader> PageHeaders { get; set; }

        public List<EbPageFooter> PageFooters { get; set; }

        public List<EbReportDetail> Detail { get; set; }

        public List<EbReportGroup> ReportGroups { set; get; }

        public string DataSourceRefId { get; set; }

        public EbFont Font { get; set; }

        public EbReport()
        {
            ReportHeaders = new List<EbReportHeader>();

            PageHeaders = new List<EbPageHeader>();

            Detail = new List<EbReportDetail>();

            PageFooters = new List<EbPageFooter>();

            ReportFooters = new List<EbReportFooter>();

            ReportGroups = new List<EbReportGroup>();
        }
    }

    public class EbTableLayoutCell : EbReportObject
    {
        public int RowIndex { set; get; }

        public int CellIndex { set; get; }

        public List<EbReportField> ControlCollection { set; get; }
    }

    public class EbReportSection : EbReportObject
    {
        public string SectionHeight { get; set; }

        public List<EbReportField> Fields { get; set; }
    }

    public class EbReportHeader : EbReportSection
    {

    }

    public class EbPageHeader : EbReportSection
    {
    }

    public class EbReportDetail : EbReportSection
    {

    }

    public class EbPageFooter : EbReportSection
    {

    }

    public class EbReportFooter : EbReportSection
    {
    }

    public class EbReportGroup : EbReportObject
    {
        public EbGroupHeader GroupHeader { set; get; }

        public EbGroupFooter GroupFooter { set; get; }
    }

    public class EbGroupHeader : EbReportSection
    {
        public int Order { set; get; }
    }

    public class EbGroupFooter : EbReportSection
    {
        public int Order { set; get; }
    }

    public class ReportGroupItem
    {
        public EbDataField field;
        public string PreviousValue;
        public int order;
    }
}
