namespace ExpressBase.Mobile
{
    public class EbReportObject : EbObject
    {
        public string EbSid { get; set; }

        public override string Name { get; set; }

        public virtual string Title { get; set; }

        public virtual string Left { get; set; }

        public virtual float LeftPt { get; set; }

        public virtual string Width { get; set; }

        public virtual float WidthPt { get; set; }

        public virtual string Top { get; set; }

        public virtual float TopPt { get; set; }

        public virtual string Height { get; set; }

        public virtual float HeightPt { get; set; }

        public virtual string BackColor { get; set; }
    }
}
