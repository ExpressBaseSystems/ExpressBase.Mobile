using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Structures;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
namespace ExpressBase.Mobile
{
    public abstract class EbDataField : EbReportField
    {
        public virtual void NotifyNewPage(bool status) { }

        public string SummaryOf { get; set; }

        public int DbType { get; set; }

        public int TableIndex { get; set; }

        public string ColumnName { get; set; }

        public override string Title { set; get; }

        public string Prefix { get; set; }

        public string Suffix { get; set; }

        public Boolean RenderInMultiLine { get; set; } = true;

        public string LinkRefId { get; set; }

        public EbScript AppearExpression { get; set; }

        private string[] _dataFieldsUsed;
        public string[] DataFieldsUsedAppearance
        {
            get
            {
                if (_dataFieldsUsed == null || _dataFieldsUsed.Count() <= 0)
                {
                    if (this.AppearExpression != null && this.AppearExpression.Code != null)
                    {
                        IEnumerable<string> matches = Regex.Matches(this.AppearExpression.GetCode(), @"T[0-9]{1}\.\w+").OfType<Match>()
                               .Select(m => m.Groups[0].Value)
                               .Distinct();

                        _dataFieldsUsed = new string[matches.Count()];
                        int i = 0;
                        foreach (string match in matches)
                            _dataFieldsUsed[i++] = match;
                    }
                }
                else
                {
                    _dataFieldsUsed = new string[0];
                }

                return _dataFieldsUsed;
            }
        }

        public override void DrawMe(float printingTop, EbReport Rep, List<Param> Params, int slno)
        {

            ColumnText ct = new ColumnText(Rep.Canvas);
            string column_val = Rep.GetDataFieldValue(ColumnName, slno, TableIndex);
            if (Prefix != "" || Suffix != "")
                column_val = Prefix + " " + column_val + " " + Suffix;

            Phrase phrase = GetPhrase(column_val, (DbType)DbType, Rep.Font);

            if (!string.IsNullOrEmpty(LinkRefId))
            {
                Anchor a = CreateLink(phrase, LinkRefId, Rep.Doc, Params);
                Paragraph p = new Paragraph { a };
                p.Font = GetItextFont(this.Font, Rep.Font);
                ct.AddText(p);
            }
            else
                ct.AddText(phrase);
            float ury = Rep.HeightPt - (printingTop + TopPt + Rep.detailprintingtop);
            float lly = Rep.HeightPt - (printingTop + TopPt + HeightPt + Rep.detailprintingtop);
            ct.SetSimpleColumn(Llx, lly, Urx, ury, Leading, (int)TextAlign);
            ct.Go();
        }

        public Phrase GetPhrase(string column_val, DbType column_type, EbFont _reportFont)
        {

            Phrase phrase = GetFormattedPhrase(this.Font, _reportFont, column_val);

            if (column_val != string.Empty)
            {
                try
                {
                    BaseFont calcbasefont = phrase.Font.GetCalculatedBaseFont(false);
                    float stringwidth = calcbasefont.GetWidthPoint(column_val, phrase.Font.CalculatedSize);
                    if (stringwidth > 0)
                    {
                        float charwidth = stringwidth / column_val.Length;
                        int numberofCharsInALine = Convert.ToInt32(Math.Floor(WidthPt / charwidth));
                        if (numberofCharsInALine < column_val.Length)
                        {
                            if (column_type == System.Data.DbType.Int32 || column_type == System.Data.DbType.Decimal || column_type == System.Data.DbType.Double || column_type == System.Data.DbType.Int16 || column_type == System.Data.DbType.Int64 || column_type == System.Data.DbType.VarNumeric)
                                column_val = "###";
                            else if (!this.RenderInMultiLine)
                                column_val = column_val.Substring(0, numberofCharsInALine - 2) + "...";
                        }
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
            return GetFormattedPhrase(this.Font, _reportFont, column_val);
        }

        public Anchor CreateLink(Phrase phrase, string LinkRefid, Document doc, List<Param> Params)
        {
            string _ref = string.Empty;
            byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(JsonConvert.SerializeObject(Params));
            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);
            int type = Convert.ToInt32(LinkRefid.Split('-')[2]);
            if (type == 3)
                _ref = "/ReportRender/Renderlink?refid=" + LinkRefid + "&_params=" + returnValue;
            else if (type == 16 || type == 17)
                _ref = "/DV/renderlink?_refid=" + LinkRefid + "&Params=" + returnValue;
            Anchor anchor = new Anchor(phrase)
            {
                Reference = _ref
            };
            return anchor;
        }

        public string FormatDecimals(string column_val, bool _inWords, int _decimalPlaces, NumberFormatInfo _numberFormat, bool formatUsingCulture)
        {
            if (_inWords)
            {
                //NumberToEnglishOld numToE = new NumberToEnglishOld();
                //column_val = numToE.changeCurrencyToWords(column_val); 
                column_val = NumberToWords.ConvertNumber(column_val);
            }
            else
            {
                if (formatUsingCulture)
                    column_val = Convert.ToDecimal(column_val).ToString("N", _numberFormat);
                if (_numberFormat.NumberDecimalDigits != _decimalPlaces)
                    column_val = Convert.ToDecimal(column_val).ToString("F" + _decimalPlaces);
            }
            return column_val;
        }

        public void DoRenderInMultiLine(string column_val, EbReport Report)
        {
            //Report.RowHeight = 0;
            Report.MultiRowTop = 0;
            DbType datatype = (DbType)DbType;
            int val_length = column_val.Length;
            Phrase phrase = new Phrase(column_val, this.GetItextFont(this.Font, this.Font));
            float calculatedValueSize = phrase.Font.CalculatedSize * val_length;
            if (calculatedValueSize > this.WidthPt)
            {
                int rowsneeded;
                if (datatype == System.Data.DbType.Decimal || datatype == System.Data.DbType.Double || datatype == System.Data.DbType.Int16 || datatype == System.Data.DbType.Int32 || datatype == System.Data.DbType.Int64 || datatype == System.Data.DbType.VarNumeric)
                    rowsneeded = 1;
                else
                    rowsneeded = Convert.ToInt32(Math.Floor(calculatedValueSize / this.WidthPt));
                if (rowsneeded > 1)
                {
                    if (Report.MultiRowTop == 0)
                    {
                        Report.MultiRowTop = this.TopPt;
                    }
                    float k = (phrase.Font.CalculatedSize) * (rowsneeded - 1);
                    if (k > Report.RowHeight)
                    {
                        Report.RowHeight = k;
                    }
                }
            }
        }
    }

    public class EbDataFieldText : EbDataField
    {
        public bool RenderAsHtml { get; set; }

        public override void DrawMe(float printingTop, EbReport Rep, List<Param> Params, int slno)
        {
            ColumnText ct = new ColumnText(Rep.Canvas);
            string column_val = Rep.GetDataFieldValue(ColumnName, slno, TableIndex);
            if (Prefix != "" || Suffix != "")
                column_val = Prefix + " " + column_val + " " + Suffix;
            Phrase phrase = GetPhrase(column_val, (DbType)DbType, Rep.Font);

            if (RenderAsHtml)
            {
                using (StringReader sr = new StringReader(column_val))
                {
                    var elements = HTMLWorker.ParseToList(sr, null);
                    foreach (IElement e in elements)
                    {
                        ct.AddElement(e);
                    }
                }
            }
            else if (!string.IsNullOrEmpty(LinkRefId))
            {
                Anchor a = CreateLink(phrase, LinkRefId, Rep.Doc, Params);
                Paragraph p = new Paragraph { a };
                p.Font = GetItextFont(this.Font, Rep.Font);
                ct.AddText(p);
            }
            else
                ct.AddText(phrase);

            float ury = Rep.HeightPt - (printingTop + TopPt + Rep.detailprintingtop);
            float lly = Rep.HeightPt - (printingTop + TopPt + HeightPt + Rep.detailprintingtop);
            ct.SetSimpleColumn(Llx, lly, Urx, ury, Leading, (int)TextAlign);
            ct.Go();
        }

    }

    public class EbDataFieldDateTime : EbDataField
    {
        public DateFormatReport Format { get; set; } = DateFormatReport.from_culture;

        public override void DrawMe(float printingTop, EbReport Rep, List<Param> Params, int slno)
        {
            ColumnText ct = new ColumnText(Rep.Canvas);
            string column_val = Rep.GetDataFieldValue(ColumnName, slno, TableIndex);
            column_val = FormatDate(column_val, Format, Rep);
            if (column_val != string.Empty)
            {
                if (Prefix != "" || Suffix != "")
                    column_val = Prefix + " " + column_val + " " + Suffix;
                Phrase phrase = GetPhrase(column_val, (DbType)DbType, Rep.Font);
                if (!string.IsNullOrEmpty(LinkRefId))
                {
                    Anchor a = CreateLink(phrase, LinkRefId, Rep.Doc, Params);
                    Paragraph p = new Paragraph
                {
                    a
                };
                    p.Font = GetItextFont(this.Font, Rep.Font);
                    ct.AddText(p);
                }
                else
                {
                    ct.AddText(phrase);
                }
                float ury = Rep.HeightPt - (printingTop + TopPt + Rep.detailprintingtop);
                float lly = Rep.HeightPt - (printingTop + TopPt + HeightPt + Rep.detailprintingtop);
                ct.SetSimpleColumn(Llx, lly, Urx, ury, Leading, (int)TextAlign);
                ct.Go();
            }
        }
    }

    public class EbDataFieldBoolean : EbDataField
    {
        public override void DrawMe(float printingTop, EbReport Rep, List<Param> Params, int slno)
        {

            ColumnText ct = new ColumnText(Rep.Canvas);
            string column_val = Rep.GetDataFieldValue(ColumnName, slno, TableIndex);
            if (Prefix != "" || Suffix != "")
                column_val = Prefix + " " + column_val + " " + Suffix;
            Phrase phrase = GetFormattedPhrase(this.Font, Rep.Font, column_val);
            if (!string.IsNullOrEmpty(LinkRefId))
            {
                Anchor a = CreateLink(phrase, LinkRefId, Rep.Doc, Params);
                Paragraph p = new Paragraph { a };
                p.Font = GetItextFont(this.Font, Rep.Font);
                ct.AddText(p);
            }
            else
                ct.AddText(phrase);
            float ury = Rep.HeightPt - (printingTop + TopPt + Rep.detailprintingtop);
            float lly = Rep.HeightPt - (printingTop + TopPt + HeightPt + Rep.detailprintingtop);
            ct.SetSimpleColumn(Llx, lly, Urx, ury, Leading, (int)TextAlign);
            ct.Go();
        }
    }

    public class EbDataFieldNumeric : EbDataField
    {
        public bool AmountInWords { get; set; }

        public bool SuppressIfZero { get; set; }

        public bool FormatUsingCulture { get; set; } = true;

        public int DecimalPlaces { get; set; } = 2;


        public override void DrawMe(float printingTop, EbReport Rep, List<Param> Params, int slno)
        {
            ColumnText ct = new ColumnText(Rep.Canvas);
            string column_val = Rep.GetDataFieldValue(ColumnName, slno, TableIndex);
            float ury = Rep.HeightPt - (printingTop + TopPt + Rep.detailprintingtop);
            float lly = Rep.HeightPt - (printingTop + TopPt + HeightPt + Rep.detailprintingtop);

            if (SuppressIfZero && !(Convert.ToDecimal(column_val) > 0))
                column_val = String.Empty;
            else
            {
                column_val = FormatDecimals(column_val, AmountInWords, DecimalPlaces, Rep.CultureInfo?.NumberFormat, FormatUsingCulture);
                if (Prefix != "" || Suffix != "")
                    column_val = Prefix + " " + column_val + " " + Suffix;
            }
            Phrase phrase = GetPhrase(column_val, (DbType)DbType, Rep.Font);
            if (!string.IsNullOrEmpty(LinkRefId))
            {
                Anchor a = CreateLink(phrase, LinkRefId, Rep.Doc, Params);
                Paragraph p = new Paragraph { a };
                p.Font = GetItextFont(this.Font, Rep.Font);
                ct.AddText(p);
            }
            else
                ct.AddText(phrase);
            ct.Canvas.SetColorFill(GetColor(ForeColor));
            ct.SetSimpleColumn(Llx, lly, Urx, ury, Leading, (int)TextAlign);
            ct.Go();
        }

    }

    public interface IEbDataFieldSummary
    {
        object SummarizedValue { get; }
        void Summarize(object value);
        void ResetSummary();
    }

    public class EbDataFieldNumericSummary : EbDataFieldNumeric, IEbDataFieldSummary
    {
        public SummaryFunctionsNumeric Function { get; set; }

        public bool ResetOnNewPage { get; set; }

        private int Count { get; set; }

        private decimal Sum { get; set; }

        private decimal Max { get; set; }

        private decimal Min { get; set; }

        public object SummarizedValue
        {
            get
            {
                if (Function == SummaryFunctionsNumeric.Sum)
                    return Sum;
                else if (Function == SummaryFunctionsNumeric.Average && this.Count > 0)
                    return Sum / Count;
                else if (Function == SummaryFunctionsNumeric.Count)
                    return Count;
                else if (Function == SummaryFunctionsNumeric.Max)
                    return Max;
                else if (Function == SummaryFunctionsNumeric.Min)
                    return Min;

                return 0;
            }
        }

        public void Summarize(object value)
        {
            Count++;
            decimal myvalue = Convert.ToDecimal(value);

            if (Function == SummaryFunctionsNumeric.Sum || Function == SummaryFunctionsNumeric.Average)
            {
                if (Function == SummaryFunctionsNumeric.Sum || Function == SummaryFunctionsNumeric.Average)
                    Sum += myvalue;
            }

            if (Count > 1)
            {
                if (Function == SummaryFunctionsNumeric.Max)
                    Max = (Max > myvalue) ? Max : myvalue;
                else if (Function == SummaryFunctionsNumeric.Min)
                    Min = (Min < myvalue) ? Min : myvalue;
            }
            else
            {
                if (Function == SummaryFunctionsNumeric.Max)
                    Max = myvalue;
                else if (Function == SummaryFunctionsNumeric.Min)
                    Min = myvalue;
            }
        }

        public void ResetSummary()
        {
            if (Function == SummaryFunctionsNumeric.Sum)
                Sum = 0;
            else if (Function == SummaryFunctionsNumeric.Average && this.Count > 0)
                Sum = Count = 0;
            else if (Function == SummaryFunctionsNumeric.Count)
                Count = 0;
            else if (Function == SummaryFunctionsNumeric.Max)
                Max = 0;
            else if (Function == SummaryFunctionsNumeric.Min)
                Min = 0;
        }
        public override void NotifyNewPage(bool status)
        {
            if (status && ResetOnNewPage)
                Sum = 0;
        }

        public override void DrawMe(float printingTop, EbReport Rep, List<Param> Params, int slno)
        {
            float ury = Rep.HeightPt - (printingTop + TopPt + Rep.detailprintingtop);
            float lly = Rep.HeightPt - (printingTop + TopPt + HeightPt + Rep.detailprintingtop);
            string column_val = SummarizedValue.ToString();
            ResetSummary();
            column_val = FormatDecimals(column_val, AmountInWords, DecimalPlaces, Rep.CultureInfo?.NumberFormat, FormatUsingCulture);

            if (Rep.SummaryValInRow.ContainsKey(Title))
                Rep.SummaryValInRow[Title] = new PdfNTV { Name = Title.Replace(".", "_"), Type = PdfEbDbTypes.Int32, Value = column_val };
            else
                Rep.SummaryValInRow.Add(Title, new PdfNTV { Name = Title.Replace(".", "_"), Type = PdfEbDbTypes.Int32, Value = column_val });

            Phrase phrase = GetPhrase(column_val, (DbType)DbType, Rep.Font);
            ColumnText ct = new ColumnText(Rep.Canvas);
            ct.SetSimpleColumn(phrase, Llx, lly, Urx, ury, Leading, (int)TextAlign);
            ct.Go();
        }
    }

    public class EbDataFieldTextSummary : EbDataFieldText, IEbDataFieldSummary
    {
        public SummaryFunctionsText Function { get; set; }

        public bool ResetOnNewPage { get; set; }

        private int Count { get; set; }

        private string Max { get; set; } = "";

        private string Min { get; set; } = "";

        public object SummarizedValue
        {
            get
            {
                if (Function == SummaryFunctionsText.Count)
                    return Count;
                else if (Function == SummaryFunctionsText.Max)
                    return Max;
                else if (Function == SummaryFunctionsText.Min)
                    return Min;

                return 0;
            }
        }

        public void Summarize(object value)
        {
            string myvalue = value.ToString();
            Count++;
            if (Count > 1)
            {
                if (Function == SummaryFunctionsText.Max)
                    Max = (Max.CompareTo(myvalue) > 0) ? Max : myvalue;
                else if (Function == SummaryFunctionsText.Min)
                    Min = (Min.CompareTo(myvalue) > 0) ? myvalue : Min;
            }
            else
            {
                if (Function == SummaryFunctionsText.Max)
                    Max = myvalue;
                else if (Function == SummaryFunctionsText.Min)
                    Min = myvalue;
            }
        }

        public void ResetSummary()
        {
            if (Function == SummaryFunctionsText.Count)
                Count = 0;
            else if (Function == SummaryFunctionsText.Max)
                Max = string.Empty;
            else if (Function == SummaryFunctionsText.Min)
                Min = string.Empty;
        }

        public override void DrawMe(float printingTop, EbReport Rep, List<Param> Params, int slno)
        {
            float ury = Rep.HeightPt - (printingTop + TopPt + Rep.detailprintingtop);
            float lly = Rep.HeightPt - (printingTop + TopPt + HeightPt + Rep.detailprintingtop);
            string column_val = SummarizedValue.ToString();
            ResetSummary();
            Phrase phrase = GetPhrase(column_val, (DbType)DbType, Rep.Font);
            ColumnText ct = new ColumnText(Rep.Canvas);
            ct.SetSimpleColumn(phrase, Llx, lly, Urx, ury, Leading, (int)TextAlign);
            ct.Go();
        }
    }

    public class EbDataFieldDateTimeSummary : EbDataFieldDateTime, IEbDataFieldSummary
    {
        public SummaryFunctionsDateTime Function { get; set; }

        public bool ResetOnNewPage { get; set; }

        private int Count { get; set; }

        private DateTime Max { get; set; }

        private DateTime Min { get; set; }

        public object SummarizedValue
        {
            get
            {
                if (Function == SummaryFunctionsDateTime.Count)
                    return Count;
                else if (Function == SummaryFunctionsDateTime.Max)
                    return Max;
                else if (Function == SummaryFunctionsDateTime.Min)
                    return Min;

                return 0;
            }
        }

        public void Summarize(object value)
        {
            DateTime myvalue = Convert.ToDateTime(value);
            Count++;
            if (Count > 1)
            {
                if (Function == SummaryFunctionsDateTime.Max)
                    Max = (DateTime.Compare(Max, myvalue) > 0) ? Max : myvalue;
                if (Function == SummaryFunctionsDateTime.Min)
                    Min = (DateTime.Compare(Min, myvalue) > 0) ? myvalue : Min;
            }
            else
            {
                if (Function == SummaryFunctionsDateTime.Max)
                    Max = myvalue;
                if (Function == SummaryFunctionsDateTime.Min)
                    Min = myvalue;
            }
        }

        public void ResetSummary()
        {
            if (Function == SummaryFunctionsDateTime.Count)
                Count = 0;
            else if (Function == SummaryFunctionsDateTime.Max)
                Max = DateTime.MinValue;
            else if (Function == SummaryFunctionsDateTime.Min)
                Min = DateTime.MinValue;
        }

        public override void DrawMe(float printingTop, EbReport Rep, List<Param> Params, int slno)
        {
            ColumnText ct = new ColumnText(Rep.Canvas);
            string column_val = FormatDate(SummarizedValue.ToString(), Format, Rep);
            ResetSummary();
            if (column_val != string.Empty)
            {
                if (Prefix != "" || Suffix != "")
                    column_val = Prefix + " " + column_val + " " + Suffix;
                Phrase phrase = GetPhrase(column_val, (DbType)DbType, Rep.Font);

                ct.AddText(phrase);
                float ury = Rep.HeightPt - (printingTop + TopPt + Rep.detailprintingtop);
                float lly = Rep.HeightPt - (printingTop + TopPt + HeightPt + Rep.detailprintingtop);
                ct.SetSimpleColumn(Llx, lly, Urx, ury, Leading, (int)TextAlign);
                ct.Go();
            }
        }
    }

    public class EbDataFieldBooleanSummary : EbDataFieldBoolean, IEbDataFieldSummary
    {
        public SummaryFunctionsBoolean Function { get; set; }

        public bool ResetOnNewPage { get; set; }

        private int Count { get; set; }

        public object SummarizedValue
        {
            get
            {
                if (Function == SummaryFunctionsBoolean.Count)
                    return Count;
                return 0;
            }
        }

        public void Summarize(object value)
        {
            Count++;
        }

        public void ResetSummary()
        {
            if (Function == SummaryFunctionsBoolean.Count)
                Count = 0;
        }


        public override void DrawMe(float printingTop, EbReport Rep, List<Param> Params, int slno)
        {

            ColumnText ct = new ColumnText(Rep.Canvas);
            string column_val = SummarizedValue.ToString();
            ResetSummary();
            if (Prefix != "" || Suffix != "")
                column_val = Prefix + " " + column_val + " " + Suffix;
            Phrase phrase = GetFormattedPhrase(this.Font, Rep.Font, column_val);

            if (!string.IsNullOrEmpty(LinkRefId))
            {
                Anchor a = CreateLink(phrase, LinkRefId, Rep.Doc, Params);
                Paragraph p = new Paragraph { a };
                p.Font = GetItextFont(this.Font, Rep.Font);
                ct.AddText(p);
            }
            else
                ct.AddText(phrase);
            float ury = Rep.HeightPt - (printingTop + TopPt + Rep.detailprintingtop);
            float lly = Rep.HeightPt - (printingTop + TopPt + HeightPt + Rep.detailprintingtop);
            ct.SetSimpleColumn(Llx, lly, Urx, ury, Leading, (int)TextAlign);
            ct.Go();
        }
    }

    public class EbCalcField : EbDataField
    {
        public EbScript ValExpression { get; set; }

        public string CalcFieldType { get; set; }

        public int CalcFieldIntType { get; set; }

        public int DecimalPlaces { get; set; } = 2;

        public bool AmountInWords { get; set; }

        public bool SuppressIfZero { get; set; }

        public bool FormatUsingCulture { get; set; } = true;

        private string[] _dataFieldsUsed;
        public string[] DataFieldsUsedInCalc
        {
            get
            {
                if (_dataFieldsUsed == null)
                    if (ValExpression?.Code != null)
                    {
                        IEnumerable<string> matches = Regex.Matches(ValExpression.GetCode(), @"T[0-9]{1}\.\w+").OfType<Match>()
                             .Select(m => m.Groups[0].Value)
                             .Distinct();


                        _dataFieldsUsed = new string[matches.Count()];
                        int i = 0;
                        foreach (string match in matches)
                            _dataFieldsUsed[i++] = match;
                    }
                return _dataFieldsUsed;
            }
        }

        public override void DrawMe(float printingTop, EbReport Rep, List<Param> Params, int slno)
        {
            ColumnText ct = new ColumnText(Rep.Canvas);
            string column_val = string.Empty;
            EbDbTypes dbtype = EbDbTypes.String;
            EbPdfGlobals globals = new EbPdfGlobals();

            try
            {
                column_val = Rep.ExecuteExpression(Rep.ValueScriptCollection[Name], slno, globals, DataFieldsUsedInCalc, true).ToString();
                dbtype = (EbDbTypes)CalcFieldIntType;

                if (Rep.CalcValInRow.ContainsKey(Title))
                    Rep.CalcValInRow[Title] = new PdfNTV { Name = Title, Type = (PdfEbDbTypes)(int)dbtype, Value = column_val };
                else
                    Rep.CalcValInRow.Add(Title, new PdfNTV { Name = Title, Type = (PdfEbDbTypes)(int)dbtype, Value = column_val });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + e.StackTrace);

            }
            if (SuppressIfZero && !(Convert.ToDecimal(column_val) > 0))
                column_val = String.Empty;
            else
            {
                if (dbtype == EbDbTypes.Decimal)
                    column_val = FormatDecimals(column_val, AmountInWords, DecimalPlaces, Rep.CultureInfo?.NumberFormat, FormatUsingCulture);
                if (Prefix != "" || Suffix != "")
                {
                    column_val = Prefix + " " + column_val + " " + Suffix;
                }
            }
            Phrase phrase = GetPhrase(column_val, (DbType)DbType, Rep.Font);

            if (!string.IsNullOrEmpty(LinkRefId))
            {
                Anchor a = CreateLink(phrase, LinkRefId, Rep.Doc, Params);
                Paragraph p = new Paragraph { a };
                p.Font = GetItextFont(this.Font, Rep.Font);
                ct.AddText(p);
            }
            else
            {
                ct.AddText(phrase);
            }

            float ury = Rep.HeightPt - (printingTop + TopPt + Rep.detailprintingtop);
            float lly = Rep.HeightPt - (printingTop + TopPt + HeightPt + Rep.detailprintingtop);
            ct.SetSimpleColumn(Llx, lly, Urx, ury, Leading, (int)TextAlign);
            ct.Go();
        }

        public string GetCalcFieldValue(EbPdfGlobals globals, EbDataSet DataSet, int serialnumber, EbReport Rep)
        {
            return Rep.ExecuteExpression(Rep.ValueScriptCollection[this.Name], serialnumber, globals, DataFieldsUsedInCalc, true)?.ToString();
        }
    }

    public class EbCalcFieldNumericSummary : EbCalcField, IEbDataFieldSummary
    {
        public SummaryFunctionsNumeric Function { get; set; }

        public bool ResetOnNewPage { get; set; }

        private int Count { get; set; }

        private decimal Sum { get; set; }

        private decimal Max { get; set; }

        private decimal Min { get; set; }

        public object SummarizedValue
        {
            get
            {
                if (Function == SummaryFunctionsNumeric.Sum)
                    return Sum;
                else if (Function == SummaryFunctionsNumeric.Average && Count > 0)
                    return Sum / Count;
                else if (Function == SummaryFunctionsNumeric.Count)
                    return Count;
                else if (Function == SummaryFunctionsNumeric.Max)
                    return Max;
                else if (Function == SummaryFunctionsNumeric.Min)
                    return Min;

                return 0;
            }
        }

        public void Summarize(object value)
        {
            Count++;
            decimal myvalue = Convert.ToDecimal(value);

            if (Function == SummaryFunctionsNumeric.Sum || Function == SummaryFunctionsNumeric.Average)
            {
                if (Function == SummaryFunctionsNumeric.Sum || Function == SummaryFunctionsNumeric.Average)
                    Sum += myvalue;
            }

            if (Count > 1)
            {
                if (Function == SummaryFunctionsNumeric.Max)
                    Max = (Max > myvalue) ? Max : myvalue;
                else if (Function == SummaryFunctionsNumeric.Min)
                    Min = (Min < myvalue) ? Min : myvalue;
            }
            else
            {
                if (Function == SummaryFunctionsNumeric.Max)
                    Max = myvalue;
                else if (Function == SummaryFunctionsNumeric.Min)
                    Min = myvalue;
            }
        }

        public void ResetSummary()
        {
            if (Function == SummaryFunctionsNumeric.Sum)
                Sum = 0;
            else if (Function == SummaryFunctionsNumeric.Average && Count > 0)
                Sum = Count = 0;
            else if (Function == SummaryFunctionsNumeric.Count)
                Count = 0;
            else if (Function == SummaryFunctionsNumeric.Max)
                Max = 0;
            else if (Function == SummaryFunctionsNumeric.Min)
                Min = 0;
        }
        public override void NotifyNewPage(bool status)
        {
            if (status && ResetOnNewPage)
                Sum = 0;
        }

        public override void DrawMe(float printingTop, EbReport Rep, List<Param> Params, int slno)
        {
            float ury = Rep.HeightPt - (printingTop + TopPt + Rep.detailprintingtop);
            float lly = Rep.HeightPt - (printingTop + TopPt + HeightPt + Rep.detailprintingtop);
            string column_val = SummarizedValue.ToString();
            ResetSummary();

            if (SuppressIfZero && !(Convert.ToDecimal(column_val) > 0))
                column_val = string.Empty;
            else
                column_val = FormatDecimals(column_val, AmountInWords, DecimalPlaces, Rep.CultureInfo?.NumberFormat, FormatUsingCulture);

            if (Rep.SummaryValInRow.ContainsKey(Title))
                Rep.SummaryValInRow[Title] = new PdfNTV { Name = Title.Replace(".", "_"), Type = PdfEbDbTypes.Int32, Value = column_val };
            else
                Rep.SummaryValInRow.Add(Title, new PdfNTV { Name = Title.Replace(".", "_"), Type = PdfEbDbTypes.Int32, Value = column_val });
            Phrase phrase = GetPhrase(column_val, (DbType)DbType, Rep.Font);
            ColumnText ct = new ColumnText(Rep.Canvas);
            if (!string.IsNullOrEmpty(LinkRefId))
            {
                Anchor a = CreateLink(phrase, LinkRefId, Rep.Doc, Params);
                Paragraph p = new Paragraph { a };
                p.Font = GetItextFont(this.Font, Rep.Font);
                ct.AddText(p);
            }
            else
                ct.AddText(phrase);
            ct.SetSimpleColumn(Llx, lly, Urx, ury, Leading, (int)TextAlign);
            ct.Go();
        }
    }

    public class EbCalcFieldTextSummary : EbCalcField, IEbDataFieldSummary
    {
        public SummaryFunctionsText Function { get; set; }

        public bool ResetOnNewPage { get; set; }

        private int Count { get; set; }

        private string Max { get; set; } = "";

        private string Min { get; set; } = "";

        public object SummarizedValue
        {
            get
            {
                if (Function == SummaryFunctionsText.Count)
                    return Count;
                else if (Function == SummaryFunctionsText.Max)
                    return Max;
                else if (Function == SummaryFunctionsText.Min)
                    return Min;

                return 0;
            }
        }

        public void Summarize(object value)
        {
            string myvalue = value.ToString();
            Count++;
            if (Count > 1)
            {
                if (Function == SummaryFunctionsText.Max)
                    Max = (Max.CompareTo(myvalue) > 0) ? Max : myvalue;
                else if (Function == SummaryFunctionsText.Min)
                    Min = (Min.CompareTo(myvalue) > 0) ? myvalue : Min;
            }
            else
            {
                if (Function == SummaryFunctionsText.Max)
                    Max = myvalue;
                else if (Function == SummaryFunctionsText.Min)
                    Min = myvalue;
            }
        }

        public void ResetSummary()
        {
            if (Function == SummaryFunctionsText.Count)
                Count = 0;
            else if (Function == SummaryFunctionsText.Max)
                Max = string.Empty;
            else if (Function == SummaryFunctionsText.Min)
                Min = string.Empty;
        }

        public override void DrawMe(float printingTop, EbReport Rep, List<Param> Params, int slno)
        {
            float ury = Rep.HeightPt - (printingTop + TopPt + Rep.detailprintingtop);
            float lly = Rep.HeightPt - (printingTop + TopPt + HeightPt + Rep.detailprintingtop);
            string column_val = SummarizedValue.ToString();
            ResetSummary();
            Phrase phrase = GetPhrase(column_val, (DbType)DbType, Rep.Font);
            ColumnText ct = new ColumnText(Rep.Canvas);
            if (!string.IsNullOrEmpty(LinkRefId))
            {
                Anchor a = CreateLink(phrase, LinkRefId, Rep.Doc, Params);
                Paragraph p = new Paragraph { a };
                p.Font = GetItextFont(this.Font, Rep.Font);
                ct.AddText(p);
            }
            else
                ct.AddText(phrase);
            ct.SetSimpleColumn(Llx, lly, Urx, ury, Leading, (int)TextAlign);
            ct.Go();
        }
    }

    public class EbCalcFieldDateTimeSummary : EbCalcField, IEbDataFieldSummary
    {
        public SummaryFunctionsDateTime Function { get; set; }

        public bool ResetOnNewPage { get; set; }

        private int Count { get; set; }

        private DateTime Max { get; set; }

        private DateTime Min { get; set; }

        public object SummarizedValue
        {
            get
            {
                if (Function == SummaryFunctionsDateTime.Count)
                    return Count;
                else if (Function == SummaryFunctionsDateTime.Max)
                    return Max;
                else if (Function == SummaryFunctionsDateTime.Min)
                    return Min;

                return 0;
            }
        }

        public void Summarize(object value)
        {
            DateTime myvalue = Convert.ToDateTime(value);
            Count++;
            if (Count > 1)
            {
                if (Function == SummaryFunctionsDateTime.Max)
                    Max = (DateTime.Compare(Max, myvalue) > 0) ? Max : myvalue;
                if (Function == SummaryFunctionsDateTime.Min)
                    Min = (DateTime.Compare(Min, myvalue) > 0) ? myvalue : Min;
            }
            else
            {
                if (Function == SummaryFunctionsDateTime.Max)
                    Max = myvalue;
                if (Function == SummaryFunctionsDateTime.Min)
                    Min = myvalue;
            }
        }

        public void ResetSummary()
        {
            if (Function == SummaryFunctionsDateTime.Count)
                Count = 0;
            else if (Function == SummaryFunctionsDateTime.Max)
                Max = DateTime.MinValue;
            else if (Function == SummaryFunctionsDateTime.Min)
                Min = DateTime.MinValue;
        }

        public override void DrawMe(float printingTop, EbReport Rep, List<Param> Params, int slno)
        {
            float ury = Rep.HeightPt - (printingTop + TopPt + Rep.detailprintingtop);
            float lly = Rep.HeightPt - (printingTop + TopPt + HeightPt + Rep.detailprintingtop);
            string column_val = SummarizedValue.ToString();
            ResetSummary();
            Phrase phrase = GetPhrase(column_val, (DbType)DbType, Rep.Font);
            ColumnText ct = new ColumnText(Rep.Canvas);
            if (!string.IsNullOrEmpty(LinkRefId))
            {
                Anchor a = CreateLink(phrase, LinkRefId, Rep.Doc, Params);
                Paragraph p = new Paragraph { a };
                p.Font = GetItextFont(this.Font, Rep.Font);
                ct.AddText(p);
            }
            else
                ct.AddText(phrase);
            ct.SetSimpleColumn(Llx, lly, Urx, ury, Leading, (int)TextAlign);
            ct.Go();
        }
    }

    public class EbCalcFieldBooleanSummary : EbCalcField, IEbDataFieldSummary
    {
        public SummaryFunctionsBoolean Function { get; set; }

        public bool ResetOnNewPage { get; set; }

        private int Count { get; set; }

        public object SummarizedValue
        {
            get
            {
                if (Function == SummaryFunctionsBoolean.Count)
                    return Count;
                return 0;
            }
        }

        public void Summarize(object value)
        {
            Count++;
        }

        public void ResetSummary()
        {
            if (Function == SummaryFunctionsBoolean.Count)
                Count = 0;
        }

        public override void DrawMe(float printingTop, EbReport Rep, List<Param> Params, int slno)
        {
            float ury = Rep.HeightPt - (printingTop + TopPt + Rep.detailprintingtop);
            float lly = Rep.HeightPt - (printingTop + TopPt + HeightPt + Rep.detailprintingtop);
            string column_val = SummarizedValue.ToString();
            ResetSummary();
            Phrase phrase = GetFormattedPhrase(this.Font, Rep.Font, column_val);
            ColumnText ct = new ColumnText(Rep.Canvas);
            if (!string.IsNullOrEmpty(LinkRefId))
            {
                Anchor a = CreateLink(phrase, LinkRefId, Rep.Doc, Params);
                Paragraph p = new Paragraph { a };
                p.Font = GetItextFont(this.Font, Rep.Font);
                ct.AddText(p);
            }
            else
                ct.AddText(phrase);
            ct.SetSimpleColumn(Llx, lly, Urx, ury, Leading, (int)TextAlign);
            ct.Go();
        }
    }
}