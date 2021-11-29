using System.Collections.Generic;

namespace ExpressBase.Mobile
{
    public abstract class EbReportFieldShape : EbReportField
    {
        public override EbTextAlign TextAlign { get; set; }

        public override string Title { get; set; }

        public override string ForeColor { get; set; }

        public override EbFont Font { get; set; }


        public override bool Dotted { get; set; }
    }


    public class EbCircle : EbReportFieldShape
    {
        public override void DrawMe(float printingTop, EbReport Rep, List<Param> Linkparams, int slno)
        {
            if (Height == Width)
            {
                float radius = WidthPt / 2;
                float xval = LeftPt + radius;
                float yval = Rep.HeightPt - (printingTop + TopPt + radius + Rep.detailprintingtop);

                Rep.Canvas.SetColorStroke(GetColor(BorderColor));
                Rep.Canvas.SetColorFill(GetColor(BackColor));
                Rep.Canvas.SetLineWidth(Border);
                if (Dotted)
                    Rep.Canvas.SetLineDash(5, 5, 3);
                Rep.Canvas.Circle(xval, yval, radius);
                Rep.Canvas.FillStroke();
            }
            else
            {
                float y1 = Rep.HeightPt - (printingTop + TopPt + HeightPt + Rep.detailprintingtop);
                float y2 = Rep.HeightPt - (printingTop + TopPt + Rep.detailprintingtop);
                Rep.Canvas.SetColorStroke(GetColor(BorderColor));
                Rep.Canvas.SetColorFill(GetColor(BackColor));
                Rep.Canvas.SetLineWidth(Border);
                if (Dotted)
                    Rep.Canvas.SetLineDash(5, 5, 3);
                Rep.Canvas.Ellipse(Llx, y1, Urx, y2);
                Rep.Canvas.FillStroke();
            }
        }
    }

    public class EbRect : EbReportFieldShape
    {
        public override void DrawMe(float printingTop, EbReport Rep, List<Param> Linkparams, int slno)
        {
            float y = Rep.HeightPt - (printingTop + TopPt + HeightPt + Rep.detailprintingtop);
            Rep.Canvas.SetColorStroke(GetColor(BorderColor));
            Rep.Canvas.SetColorFill(GetColor(BackColor));
            Rep.Canvas.SetLineWidth(Border);
            if (Dotted)
                Rep.Canvas.SetLineDash(5, 5, 3);
            Rep.Canvas.Rectangle(Llx, y, WidthPt, HeightPt);
            Rep.Canvas.FillStroke();
        }
    }

    public class EbArrR : EbHl
    {
        public override void DrawMe(float printingTop, EbReport Rep, List<Param> Linkparams, int slno)
        {
            base.DrawMe(printingTop, Rep, Linkparams, slno);
            float y = Rep.HeightPt - (printingTop + TopPt + Rep.detailprintingtop);
            Rep.Canvas.SetColorStroke(GetColor(BorderColor));
            Rep.Canvas.SetColorFill(GetColor(BorderColor));
            Rep.Canvas.SetLineWidth(Border);
            if (Dotted)
                Rep.Canvas.SetLineDash(5, 5, 3);
            Rep.Canvas.MoveTo(Urx, y);
            Rep.Canvas.LineTo(Urx - 3, y - 3);
            Rep.Canvas.LineTo(Urx - 3, y + 3);
            Rep.Canvas.ClosePathFillStroke();
        }
    }

    public class EbArrL : EbHl
    {
        public override void DrawMe(float printingTop, EbReport Rep, List<Param> Linkparams, int slno)
        {
            base.DrawMe(printingTop, Rep, Linkparams, slno);
            float y = Rep.HeightPt - (printingTop + TopPt + Rep.detailprintingtop);
            Rep.Canvas.SetColorStroke(GetColor(BorderColor));
            Rep.Canvas.SetColorFill(GetColor(BorderColor));
            Rep.Canvas.SetLineWidth(Border);
            if (Dotted)
                Rep.Canvas.SetLineDash(5, 5, 3);
            Rep.Canvas.MoveTo(Llx, y);
            Rep.Canvas.LineTo(Llx + 3, y + 3);
            Rep.Canvas.LineTo(Llx + 3, y - 3);
            Rep.Canvas.ClosePathFillStroke();
        }
    }


    public class EbArrD : EbVl
    {
        public override void DrawMe(float printingTop, EbReport Rep, List<Param> Linkparams, int slno)
        {
            base.DrawMe(printingTop, Rep, Linkparams, slno);
            float y = Rep.HeightPt - (printingTop + TopPt + HeightPt + Rep.detailprintingtop);
            Rep.Canvas.SetColorStroke(GetColor(BorderColor));
            Rep.Canvas.SetColorFill(GetColor(BorderColor));
            Rep.Canvas.SetLineWidth(Border);
            if (Dotted)
                Rep.Canvas.SetLineDash(5, 5, 3);
            Rep.Canvas.MoveTo(Llx, y);
            Rep.Canvas.LineTo(Llx - 3, y + 3);
            Rep.Canvas.LineTo(Llx + 3, y + 3);
            Rep.Canvas.ClosePathFillStroke();
        }
    }
    public class EbArrU : EbVl
    {
        public override void DrawMe(float printingTop, EbReport Rep, List<Param> Linkparams, int slno)
        {
            base.DrawMe(printingTop, Rep, Linkparams, slno);
            float y = Rep.HeightPt - (printingTop + TopPt + Rep.detailprintingtop);
            Rep.Canvas.SetColorStroke(GetColor(BorderColor));
            Rep.Canvas.SetColorFill(GetColor(BorderColor));
            Rep.Canvas.SetLineWidth(Border);
            if (Dotted)
                Rep.Canvas.SetLineDash(5, 5, 3);
            Rep.Canvas.MoveTo(Llx, y);
            Rep.Canvas.LineTo(Llx + 3, y - 3);
            Rep.Canvas.LineTo(Llx - 3, y - 3);
            Rep.Canvas.ClosePathFillStroke();
        }
    }

    public class EbByArrH : EbHl
    {
        public override void DrawMe(float printingTop, EbReport Rep, List<Param> Linkparams, int slno)
        {
            base.DrawMe(printingTop, Rep, Linkparams, slno);
            float y1 = Rep.HeightPt - (printingTop + TopPt + Rep.detailprintingtop);
            Rep.Canvas.SetColorStroke(GetColor(BorderColor));
            Rep.Canvas.SetColorFill(GetColor(BorderColor));
            Rep.Canvas.SetLineWidth(Border);
            if (Dotted)
                Rep.Canvas.SetLineDash(5, 5, 3);
            Rep.Canvas.MoveTo(Urx, y1);
            Rep.Canvas.LineTo(Urx - 3, y1 - 3);
            Rep.Canvas.LineTo(Urx - 3, y1 + 3);
            Rep.Canvas.ClosePathFillStroke();
            float y2 = y1;
            Rep.Canvas.MoveTo(Llx, y2);
            Rep.Canvas.LineTo(Llx + 3, y2 + 3);
            Rep.Canvas.LineTo(Llx + 3, y2 - 3);
            Rep.Canvas.ClosePathFillStroke();
        }
    }
    public class EbByArrV : EbVl
    {
        public override void DrawMe(float printingTop, EbReport Rep, List<Param> Linkparams, int slno)
        {
            float rowH = (TopPt > Rep.MultiRowTop) ? Rep.RowHeight : 0;
            base.DrawMe(printingTop, Rep, Linkparams, slno);
            float y1 = Rep.HeightPt - (printingTop + TopPt + Rep.detailprintingtop);
            Rep.Canvas.SetColorStroke(GetColor(BorderColor));
            Rep.Canvas.SetColorFill(GetColor(BorderColor));
            Rep.Canvas.SetLineWidth(Border);
            if (Dotted)
                Rep.Canvas.SetLineDash(5, 5, 3);
            Rep.Canvas.MoveTo(Llx, y1);
            Rep.Canvas.LineTo(Llx + 3, y1 - 3);
            Rep.Canvas.LineTo(Llx - 3, y1 - 3);
            Rep.Canvas.ClosePathFillStroke();
            float y2 = Rep.HeightPt - (printingTop + TopPt + HeightPt + Rep.detailprintingtop + rowH);
            Rep.Canvas.MoveTo(Llx, y2);
            Rep.Canvas.LineTo(Llx - 3, y2 + 3);
            Rep.Canvas.LineTo(Llx + 3, y2 + 3);
            Rep.Canvas.ClosePathFillStroke();
        }
    }

    public class EbHl : EbReportFieldShape
    {
        public override void DrawMe(float printingTop, EbReport Rep, List<Param> Linkparams, int slno)
        {
            float rowH = (TopPt > Rep.MultiRowTop) ? Rep.RowHeight : 0;
            float y1 = Rep.HeightPt - (printingTop + TopPt + Rep.detailprintingtop + rowH);
            float y2 = y1;
            Rep.Canvas.SetColorStroke(GetColor(BorderColor));
            Rep.Canvas.SetLineWidth(Border);
            if (Dotted)
                Rep.Canvas.SetLineDash(5, 5, 3);
            Rep.Canvas.MoveTo(Llx, y1);
            Rep.Canvas.LineTo(Urx, y2);
            Rep.Canvas.Stroke();
        }
    }

    public class EbVl : EbReportFieldShape
    {
        public override void DrawMe(float printingTop, EbReport Rep, List<Param> Linkparams, int slno)
        {
            float y1 = Rep.HeightPt - (printingTop + TopPt + Rep.detailprintingtop);
            float y2 = Rep.HeightPt - (printingTop + TopPt + HeightPt + Rep.detailprintingtop);
            Rep.Canvas.SetColorStroke(GetColor(BorderColor));
            Rep.Canvas.SetLineWidth(Border);
            if (Dotted)
                Rep.Canvas.SetLineDash(5, 5, 3);
            Rep.Canvas.MoveTo(Llx, y1);
            Rep.Canvas.LineTo(Llx, y2);
            Rep.Canvas.Stroke();
        }
    }

    public class EbTable_Layout : EbReportFieldShape
    {
        public int ColoumCount { get; set; }

        public int RowCount { get; set; }

        public List<EbTableLayoutCell> CellCollection { get; set; }

    }
}
