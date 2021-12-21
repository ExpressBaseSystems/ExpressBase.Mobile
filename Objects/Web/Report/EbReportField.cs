using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using ExpressBase.Mobile.Enums;
using iTextSharp.text;
using iTextSharp.text.pdf;
using QRCoder;
using Xamarin.Essentials;

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

        public float Leading { get; set; } = 12;

        public virtual bool Dotted { get; set; }

        public float Llx
        {
            get
            {
                return LeftPt;
            }
        }


        public float Urx
        {
            get
            {
                return WidthPt + LeftPt;
            }
        }

        public EbScript LayoutExpression { get; set; }

        public EbScript HideExpression { get; set; }

        public bool IsHidden { get; set; }

        public iTextSharp.text.Color GetColor(string Color)
        {
            return new iTextSharp.text.Color(ColorConverters.FromHex(Color));
        }

        public Phrase GetFormattedPhrase(EbFont Font, EbFont _reportFont, string text)
        {
            iTextSharp.text.Font iTextFont = null;
            if (Font is null)
            {
                if (!(_reportFont is null))
                    Font = _reportFont;
                else
                    Font = (new EbFont { Color = "#000000", FontName = "Roboto", Caps = false, Size = 10, Strikethrough = false, Style = 0, Underline = false });
            }
            iTextFont = FontFactory.GetFont(Font.FontName, Font.Size, (int)Font.Style);
            iTextFont.Color = GetColor(Font.Color);
            if (Font.Caps)
                text = text.ToUpper();
            if (Font.Strikethrough)
                iTextFont.SetStyle(iTextSharp.text.Font.STRIKETHRU);
            if (Font.Underline)
                iTextFont.SetStyle(iTextSharp.text.Font.UNDERLINE);
            return new Phrase(text, iTextFont);
        }

        public iTextSharp.text.Font GetItextFont(EbFont Font, EbFont _reportFont)
        {
            iTextSharp.text.Font iTextFont = null;
            if (Font is null)
            {
                if (!(_reportFont is null))
                    Font = _reportFont;
                else
                    Font = (new EbFont { Color = "#000000", FontName = "Times-Roman", Caps = false, Size = 8, Strikethrough = false, Style = 0, Underline = false });
            }
            iTextFont = FontFactory.GetFont(Font.FontName, Font.Size, (int)Font.Style);
            iTextFont.Color = GetColor(Font.Color);
            if (Font.Caps)
                Title = Title.ToUpper();
            if (Font.Strikethrough)
                iTextFont.SetStyle(iTextSharp.text.Font.STRIKETHRU);
            if (Font.Underline)
                iTextFont.SetStyle(iTextSharp.text.Font.UNDERLINE);
            return iTextFont;
        }

        public virtual void DrawMe(float printingTop, EbReport Rep, List<Param> Linkparams, int slno) { }

        public string FormatDate(string column_val, DateFormatReport format, EbReport Rep)
        {
            DateTime dt = Convert.ToDateTime(column_val);
            if (dt > DateTime.MinValue)
            {
                if (format == DateFormatReport.dddd_MMMM_d_yyyy)
                    return String.Format("{0:dddd, MMMM d, yyyy}", dt);
                else if (format == DateFormatReport.M_d_yyyy)
                    return String.Format("{0:M/d/yyyy}", dt);
                else if (format == DateFormatReport.ddd_MMM_d_yyyy)
                    return String.Format("{0:ddd, MMM d, yyyy}", dt);
                else if (format == DateFormatReport.MM_dd_yy)
                    return String.Format("{0:MM/dd/yy}", dt);
                else if (format == DateFormatReport.MM_dd_yyyy)
                    return String.Format("{0:MM/dd/yyyy}", dt);
                else if (format == DateFormatReport.dd_MM_yyyy)
                    return String.Format("{0:dd-MM-yyyy}", dt);
                else if (format == DateFormatReport.dd_MM_yyyy_slashed)
                    return string.Format("{0:dd/MM/yyyy}", dt);
                else if (format == DateFormatReport.from_culture)
                    return dt.ToString(Rep.CultureInfo?.DateTimeFormat?.ShortDatePattern + " " + Rep.CultureInfo?.DateTimeFormat?.LongTimePattern, CultureInfo.InvariantCulture);
                else if (format == DateFormatReport.dd_MMMM_yyyy)
                    return string.Format("{0:dd MMMM yyyy}", dt);
                return column_val;
            }
            else
            {
                return string.Empty;
            }

        }

        public void SetValuesFromGlobals(PdfGReportField field)
        {
            LeftPt = field.Left;
            WidthPt = field.Width;
            TopPt = field.Top;
            HeightPt = field.Height;
            BackColor = field.BackColor;
            ForeColor = field.ForeColor;
            IsHidden = field.IsHidden;
            if (field.Font != null)
                Font = new EbFont
                {
                    Caps = field.Font.Caps,
                    Color = field.Font.color,
                    FontName = field.Font.FontName,
                    Size = field.Font.Size,
                    Strikethrough = field.Font.Strikethrough,
                    Style = (Enums.FontStyle)(int)field.Font.Style,
                    Underline = field.Font.Underline
                };

        }
    }

    public class EbImg : EbReportField
    {
        public string Source { get; set; }

        public override EbTextAlign TextAlign { get; set; }

        public override string Title { get; set; }

        public override EbFont Font { get; set; }

        public int ImageRefId { get; set; }

        public string ImageColName { get; set; }

        //public override void DrawMe(float printingTop, EbReport Rep, List<Param> Linkparams, int slno)
        //{
        //    byte[] fileByte = new byte[0]; ;
        //    if (ImageRefId != 0)
        //        fileByte = Rep.GetImage(ImageRefId);
        //    else if (!string.IsNullOrEmpty(ImageColName))
        //    {
        //        dynamic val = Rep.GetDataFieldValue(ImageColName.Split('.')[1], slno, Convert.ToInt32(ImageColName.Split('.')[0].Substring(1)));
        //        Console.WriteLine("Image DrawMe val = " + ImageColName + ":" + val);
        //        if (val != null)
        //            if (val is byte[])
        //                fileByte = val;
        //            else if (val is string && val.ToString() != string.Empty && Convert.ToInt32(val) != 0)
        //                fileByte = Rep.GetImage(Convert.ToInt32(val));
        //    }

        //    if (fileByte.Length != 0)
        //    {
        //        iTextSharp.text.Image myImage = iTextSharp.text.Image.GetInstance(fileByte);
        //        myImage.ScaleToFit(WidthPt, HeightPt);
        //        myImage.SetAbsolutePosition(LeftPt, Rep.HeightPt - (printingTop + TopPt + HeightPt + Rep.detailprintingtop));
        //        myImage.Alignment = (int)TextAlign;
        //        Rep.Doc.Add(myImage);
        //    }
        //}
    }


    public class EbWaterMark : EbReportField
    {
        public string Source { get; set; }

        public override string Title { get; set; }

        public int ImageRefId { get; set; }

        public string WaterMarkText { get; set; }

        public int Rotation { get; set; }

        public new EbTextAlign TextAlign { get; set; } = EbTextAlign.Center;


        public override void DrawMe(float printingTop, EbReport Rep, List<Param> Linkparams, int slno)
        {
            Phrase phrase = null;
            if (WaterMarkText != string.Empty)
            {
                phrase = GetFormattedPhrase(this.Font, Rep.Font, WaterMarkText);
                PdfContentByte canvas;
                canvas = Rep.Writer.DirectContentUnder;
                ColumnText.ShowTextAligned(canvas, (int)TextAlign, phrase, Rep.Doc.PageSize.Width / 2, Rep.Doc.PageSize.Height / 2, Rotation);
            }
            if (ImageRefId != 0)
            {
                iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(Rep.WatermarkImages[ImageRefId]);
                img.RotationDegrees = Rotation;
                img.ScaleToFit(WidthPt, HeightPt);
                img.SetAbsolutePosition(LeftPt, Rep.HeightPt - TopPt - HeightPt);
                PdfGState _state = new PdfGState() { FillOpacity = 0.1F, StrokeOpacity = 0.1F };
                PdfContentByte cb = Rep.Writer.DirectContentUnder;
                cb.SaveState();
                cb.SetGState(_state);
                cb.AddImage(img);
                cb.RestoreState();
            }
        }
    }


    public class EbDateTime : EbReportField
    {
        public DateFormatReport Format { get; set; } = DateFormatReport.from_culture;

        public override string Title { set; get; }

        public override void DrawMe(float printingTop, EbReport Rep, List<Param> Linkparams, int slno)
        {

            float ury = Rep.HeightPt - (printingTop + TopPt + Rep.detailprintingtop);
            float lly = Rep.HeightPt - (printingTop + TopPt + HeightPt + Rep.detailprintingtop);
            string column_val = FormatDate(Rep.CurrentTimestamp.ToString(), Format, Rep);
            if (column_val != string.Empty)
            {
                Phrase phrase = GetFormattedPhrase(this.Font, Rep.Font, column_val);
                ColumnText ct = new ColumnText(Rep.Canvas);
                ct.SetSimpleColumn(phrase, Llx, lly, Urx, ury, Leading, (int)TextAlign);
                ct.Go();
            }
        }
    }


    public class EbPageNo : EbReportField
    {
        public override string Title { set; get; }

        public override void DrawMe(float printingTop, EbReport Rep, List<Param> Linkparams, int slno)
        {

            float ury = Rep.HeightPt - (printingTop + TopPt + Rep.detailprintingtop);
            float lly = Rep.HeightPt - (printingTop + TopPt + HeightPt + Rep.detailprintingtop + Rep.RowHeight);

            ColumnText ct = new ColumnText(Rep.Canvas);
            Phrase phrase = GetFormattedPhrase(this.Font, Rep.Font, Rep.PageNumber.ToString());
            ct.SetSimpleColumn(phrase, Llx, lly, Urx, ury, Leading, (int)TextAlign);
            ct.Go();
        }
    }


    public class EbPageXY : EbReportField
    {
        public override string Title { set; get; }

        public override void DrawMe(float printingTop, EbReport Rep, List<Param> Linkparams, int slno)
        {
            float ury = Rep.HeightPt - (printingTop + TopPt + Rep.detailprintingtop);
            float lly = Rep.HeightPt - (printingTop + TopPt + HeightPt + Rep.detailprintingtop + Rep.RowHeight);

            ColumnText ct = new ColumnText(Rep.Canvas);
            Phrase phrase = GetFormattedPhrase(this.Font, Rep.Font, Rep.PageNumber + "/"/* + writer.PageCount*/);
            ct.SetSimpleColumn(phrase, Llx, lly, Urx, ury, Leading, (int)TextAlign);
            ct.Go();
        }
    }


    public class EbUserName : EbReportField
    {
        public override string Title { set; get; }

        public override void DrawMe(float printingTop, EbReport Rep, List<Param> Linkparams, int slno)
        {
            float ury = Rep.HeightPt - (printingTop + TopPt + Rep.detailprintingtop);
            float lly = Rep.HeightPt - (printingTop + TopPt + HeightPt + Rep.detailprintingtop + Rep.RowHeight);

            ColumnText ct = new ColumnText(Rep.Canvas);
            Phrase phrase = GetFormattedPhrase(this.Font, Rep.Font, App.Settings.CurrentUser?.FullName ?? "Machine User");
            ct.SetSimpleColumn(phrase, Llx, lly, Urx, ury, Leading, (int)TextAlign);
            ct.Go();
        }
    }

    public class EbText : EbReportField
    {
        public override void DrawMe(float printingTop, EbReport Rep, List<Param> Linkparams, int slno)
        {
            float ury = Rep.HeightPt - (printingTop + TopPt + Rep.detailprintingtop);
            float lly = Rep.HeightPt - (printingTop + TopPt + HeightPt + Rep.detailprintingtop + Rep.RowHeight);

            ColumnText ct = new ColumnText(Rep.Canvas);
            Phrase phrase = GetFormattedPhrase(this.Font, Rep.Font, Title);
            ct.SetSimpleColumn(phrase, Llx, lly, Urx, ury, Leading, (int)TextAlign);
            ct.Go();
        }
    }


    public class EbParameter : EbReportField
    {
        public override string Title { set; get; }
        public override void DrawMe(float printingTop, EbReport Rep, List<Param> Linkparams, int slno)
        {
            string column_val = "";
            foreach (Param p in Rep.Parameters)
                if (p.Name == Title)
                    column_val = p.Value;
            float ury = Rep.HeightPt - (printingTop + TopPt + Rep.detailprintingtop);
            float lly = Rep.HeightPt - (printingTop + TopPt + HeightPt + Rep.detailprintingtop);
            Phrase phrase = GetFormattedPhrase(this.Font, Rep.Font, column_val);
            ColumnText ct = new ColumnText(Rep.Canvas);
            ct.SetSimpleColumn(phrase, Llx, lly, Urx, ury, Leading, (int)TextAlign);
            ct.Go();
        }
    }


    public class EbParamNumeric : EbReportField
    {
        public override string Title { set; get; }

        public override void DrawMe(float printingTop, EbReport Rep, List<Param> Linkparams, int slno)
        {
            string column_val = "";
            foreach (Param p in Rep.Parameters)
                if (p.Name == Title)
                    column_val = p.Value;
            float ury = Rep.HeightPt - (printingTop + TopPt + Rep.detailprintingtop);
            float lly = Rep.HeightPt - (printingTop + TopPt + HeightPt + Rep.detailprintingtop);
            Phrase phrase = GetFormattedPhrase(this.Font, Rep.Font, column_val);
            ColumnText ct = new ColumnText(Rep.Canvas);
            ct.SetSimpleColumn(phrase, Llx, lly, Urx, ury, Leading, (int)TextAlign);
            ct.Go();
        }

    }

    public class EbParamText : EbReportField
    {
        public override string Title { set; get; }

        public override void DrawMe(float printingTop, EbReport Rep, List<Param> Linkparams, int slno)
        {
            string column_val = "";
            foreach (Param p in Rep.Parameters)
                if (p.Name == Title)
                    column_val = p.Value;
            float ury = Rep.HeightPt - (printingTop + TopPt + Rep.detailprintingtop);
            float lly = Rep.HeightPt - (printingTop + TopPt + HeightPt + Rep.detailprintingtop);
            Phrase phrase = GetFormattedPhrase(this.Font, Rep.Font, column_val);
            ColumnText ct = new ColumnText(Rep.Canvas);
            ct.SetSimpleColumn(phrase, Llx, lly, Urx, ury, Leading, (int)TextAlign);
            ct.Go();
        }
    }


    public class EbParamDateTime : EbReportField
    {
        public override string Title { set; get; }


        public DateFormatReport Format { get; set; } = DateFormatReport.from_culture;

        public override void DrawMe(float printingTop, EbReport Rep, List<Param> Linkparams, int slno)
        {
            string column_val = "";
            foreach (Param p in Rep.Parameters)
                if (p.Name == Title)
                    column_val = p.Value;
            column_val = FormatDate(column_val, Format, Rep);
            if (column_val != string.Empty)
            {
                float ury = Rep.HeightPt - (printingTop + TopPt + Rep.detailprintingtop);
                float lly = Rep.HeightPt - (printingTop + TopPt + HeightPt + Rep.detailprintingtop);
                Phrase phrase = GetFormattedPhrase(this.Font, Rep.Font, column_val);
                ColumnText ct = new ColumnText(Rep.Canvas);
                ct.SetSimpleColumn(phrase, Llx, lly, Urx, ury, Leading, (int)TextAlign);
                ct.Go();
            }
        }

    }


    public class EbParamBoolean : EbReportField
    {

        public override string Title { set; get; }

        public override void DrawMe(float printingTop, EbReport Rep, List<Param> Linkparams, int slno)
        {
            string column_val = "";
            foreach (Param p in Rep.Parameters)
                if (p.Name == Title)
                    column_val = p.Value;
            float ury = Rep.HeightPt - (printingTop + TopPt + Rep.detailprintingtop);
            float lly = Rep.HeightPt - (printingTop + TopPt + HeightPt + Rep.detailprintingtop);
            Phrase phrase = GetFormattedPhrase(this.Font, Rep.Font, column_val);
            ColumnText ct = new ColumnText(Rep.Canvas);
            ct.SetSimpleColumn(phrase, Llx, lly, Urx, ury, Leading, (int)TextAlign);
            ct.Go();
        }
    }

    public class EbBarcode : EbReportField
    {
        public override string ForeColor { get; set; }

        public override string Title { get; set; }

        public override EbFont Font { get; set; }

        public string Source { get; set; }

        public string Code { get; set; }

        public int Type { get; set; }

        public bool GuardBars { get; set; }

        public float BaseLine { get; set; }

        public override void DrawMe(float printingTop, EbReport Rep, List<Param> Linkparams, int slno)
        {
            int tableIndex = Convert.ToInt32(Code.Split('.')[0].Substring(1));
            string column_name = Code.Split('.')[1];
            string column_val = Rep.GetDataFieldValue(column_name, slno, tableIndex);

            iTextSharp.text.Image imageEAN = null;
            try
            {
                //if (Type >= 1 && Type <= 6)
                //{
                ////BarcodeEan codeEAN = new BarcodeEan
                ////{
                ////    Code = code_val.PadLeft(10, '0'),
                ////    CodeType = Type,
                ////    GuardBars = GuardBars,
                ////    Baseline = BaseLine
                ////};
                ////imageEAN = codeEAN.CreateImageWithBarcode(cb: canvas, barColor: null, textColor: null);
                //}
                //if (Type == 7 || Type == 8)
                //{
                //    BarcodePostnet codepost = new BarcodePostnet();
                //    codepost.Code = code_val;
                //    codepost.CodeType = Type;
                //    codepost.GuardBars = GuardBars;
                //    codepost.Baseline = BaseLine;
                //    imageEAN = codepost.CreateImageWithBarcode(cb: canvas, barColor: null, textColor: null);
                //}
                //if (Type >= 9 && Type <= 11)
                //{
                Barcode128 uccEan128 = new Barcode128();

                uccEan128.CodeType = Type;
                uccEan128.Code = column_val;
                uccEan128.GuardBars = GuardBars;
                uccEan128.Baseline = BaseLine;
                imageEAN = uccEan128.CreateImageWithBarcode(cb: Rep.Canvas, barColor: null, textColor: null);
                //}

                // imageEAN.ScaleAbsolute(Width, Height);
                imageEAN.SetAbsolutePosition(LeftPt, Rep.HeightPt - (printingTop + TopPt + HeightPt + Rep.detailprintingtop));
                Rep.Doc.Add(imageEAN);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.ToString());
                ColumnText ct = new ColumnText(Rep.Canvas);
                float x = Rep.HeightPt - (printingTop + TopPt + Rep.detailprintingtop);
                ct.SetSimpleColumn(new Phrase("Error in generating barcode"), LeftPt, x - HeightPt, LeftPt + WidthPt, x, Leading, (int)TextAlign);
                ct.Go();
            }
        }
    }

    public class EbQRcode : EbReportField
    {
        public override string ForeColor { get; set; }

        public override string Title { get; set; }

        public override EbFont Font { get; set; }

        public string Source { get; set; }

        public string Code { get; set; }


        public override void DrawMe(float printingTop, EbReport Rep, List<Param> Linkparams, int slno)
        {
            try
            {
                int tableIndex = Convert.ToInt32(Code.Split('.')[0].Substring(1));
                string column_name = Code.Split('.')[1];
                string column_val = Rep.GetDataFieldValue(column_name, slno, tableIndex);
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(column_val, QRCodeGenerator.ECCLevel.Q);
                BitmapByteQRCode qrCode = new BitmapByteQRCode(qrCodeData);
                byte[] qrCodeImage = qrCode.GetGraphic(20);
                iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(qrCodeImage);
                img.SetAbsolutePosition(LeftPt, Rep.HeightPt - (printingTop + TopPt + HeightPt + Rep.detailprintingtop));
                img.ScaleAbsolute(WidthPt, HeightPt);
                Rep.Doc.Add(img);
            }
            catch (Exception e)
            {
                ColumnText ct = new ColumnText(Rep.Canvas);
                float x = Rep.HeightPt - (printingTop + TopPt + Rep.detailprintingtop);
                ct.SetSimpleColumn(new Phrase("Error in generating barcode"), LeftPt, x - HeightPt, LeftPt + WidthPt, x, Leading, (int)TextAlign);
                ct.Go();
                Console.WriteLine("Exception: " + e.ToString());
            }
        }
    }


    public class EbSerialNumber : EbReportField
    {

        public override string Title { set; get; }

        public override void DrawMe(float printingTop, EbReport Rep, List<Param> Linkparams, int slno)
        {
            float ury = Rep.HeightPt - (printingTop + TopPt + Rep.detailprintingtop);
            float lly = Rep.HeightPt - (printingTop + TopPt + HeightPt + Rep.detailprintingtop);

            Phrase phrase = GetFormattedPhrase(this.Font, Rep.Font, (Rep.iDetailRowPos + 1).ToString());
            ColumnText ct = new ColumnText(Rep.Canvas);
            ct.SetSimpleColumn(phrase, Llx, lly, Urx, ury, Leading, (int)TextAlign);
            ct.Go();
        }
    }


    //public class EbLocFieldImage : EbReportField
    //{
    //    public override EbFont Font { get; set; }


    //    public override void DrawMe(float printingTop, EbReport Rep, List<Param> Linkparams, int slno)
    //    {
    //        byte[] fileByte = Rep.GetImage(Convert.ToInt32(Rep.Solution.Locations[42][Title]));
    //        if (!fileByte.IsEmpty())
    //        {
    //            iTextSharp.text.Image myImage = iTextSharp.text.Image.GetInstance(fileByte);
    //            myImage.ScaleToFit(WidthPt, HeightPt);
    //            myImage.SetAbsolutePosition(Llx, Rep.HeightPt - (printingTop + TopPt + HeightPt + Rep.detailprintingtop));
    //            myImage.Alignment = (int)TextAlign;
    //            Rep.Doc.Add(myImage);
    //        }
    //    }

    //}


    //public class EbLocFieldText : EbReportField
    //{

    //    public override string Title { set; get; }



    //    public override void DrawMe(float printingTop, EbReport Rep, List<Param> Linkparams, int slno)
    //    {
    //        string column_val = Rep.Solution.Locations[42][Title];
    //        float ury = Rep.HeightPt - (printingTop + TopPt + Rep.detailprintingtop);
    //        float lly = Rep.HeightPt - (printingTop + TopPt + HeightPt + Rep.detailprintingtop);

    //        Phrase phrase = GetFormattedPhrase(this.Font, Rep.Font, column_val);
    //        ColumnText ct = new ColumnText(Rep.Canvas);
    //        ct.SetSimpleColumn(phrase, Llx, lly, Urx, ury, Leading, (int)TextAlign);
    //        ct.Go();
    //    }
    //}
}

