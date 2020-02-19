namespace ExpressBase.Mobile
{
    public abstract class EbReportField : EbReportObject
    {
        public virtual string ForeColor { get; set; }

        public virtual EbTextAlign TextAlign { get; set; }

        public string ParentName { get; set; }//only for builder

        public int Border { get; set; }

        public string BorderColor { get; set; }

        public virtual EbFont Font { get; set; }
    }

    public class EbImg : EbReportField
    {
        public string Source { get; set; }

        public override EbTextAlign TextAlign { get; set; }

        public override string Title { get; set; }

        public override EbFont Font { get; set; }

        public int ImageRefId { get; set; }

        public string ImageColName { get; set; }
    }

    public class EbWaterMark : EbReportField
    {
        public string Source { get; set; }

        public int ImageRefId { get; set; }

        public string WaterMarkText { get; set; }

        public int Rotation { get; set; }

        public new EbTextAlign TextAlign { get; set; } = EbTextAlign.Center;
    }

    public class EbDateTime : EbReportField
    {
        public DateFormatReport Format { get; set; } = DateFormatReport.from_culture;

        public override string Title { set; get; }
    }

    public class EbPageNo : EbReportField
    {
        public override string Title { set; get; }
    }

    public class EbPageXY : EbReportField
    {
        public override string Title { set; get; }
    }

    public class EbUserName : EbReportField
    {
        public override string Title { set; get; }
    }

    public class EbText : EbReportField
    {

    }

    public class EbParameter : EbReportField
    {

    }

    public class EbParamNumeric : EbReportField
    {

    }

    public class EbParamText : EbReportField
    {

    }

    public class EbParamDateTime : EbReportField
    {
        public DateFormatReport Format { get; set; } = DateFormatReport.from_culture;
    }

    public class EbParamBoolean : EbReportField
    {

    }

    public class EbBarcode : EbReportField
    {
        public string Source { get; set; }

        public string Code { get; set; }

        public int Type { get; set; }

        public bool GuardBars { get; set; }

        public float BaseLine { get; set; }
    }

    public class EbQRcode : EbReportField
    {
        public string Source { get; set; }

        public string Code { get; set; }
    }

    public class EbSerialNumber : EbReportField
    {

    }

    public class EbLocFieldImage : EbReportField
    {

    }

    public class EbLocFieldText : EbReportField
    {

    }
}

