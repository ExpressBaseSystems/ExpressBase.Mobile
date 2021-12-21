using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Models;
using ExpressBase.Mobile.Structures;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static ExpressBase.Mobile.PdfGEbFont;
using RowColletion = ExpressBase.Mobile.Data.RowColletion;

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

    public class EbReport : EbReportObject/*, IEBRootObject*/
    {
        public override string RefId { get; set; }

        public override string DisplayName { get; set; }

        public override string Name { get; set; }

        public override string Description { get; set; }

        public override string VersionNumber { get; set; }

        public EbScript OfflineQuery { set; get; }

        public bool HideInMenu { get; set; }

        public override string Status { get; set; }

        public PaperSize PaperSize { get; set; }

        public float CustomPaperHeight { get; set; }

        public float CustomPaperWidth { get; set; }

        public Margin Margin { get; set; }

        public float DesignPageHeight { get; set; }

        public string UserPassword { get; set; }

        public string OwnerPassword { get; set; }

        private string _docName = null;

        public string DocumentName
        {
            get
            {
                if (DocumentNameString != string.Empty && _docName == null)
                {
                    _docName = DocumentNameString;
                    string pattern = @"\{{(.*?)\}}";
                    IEnumerable<string> matches = Regex.Matches(DocumentNameString, pattern).OfType<Match>().Select(m => m.Groups[0].Value).Distinct();
                    foreach (string _col in matches)
                    {
                        string str = _col.Replace("{{", "").Replace("}}", "");
                        int tbl = Convert.ToInt32(str.Split('.')[0].Replace("T", ""));
                        string colval = string.Empty;
                        if (DataSet?.Tables[tbl]?.Rows.Count > 0)
                            colval = DataSet?.Tables[tbl]?.Rows[0][str.Split('.')[1]].ToString();
                        _docName = _docName.Replace(_col, colval);
                    }
                }
                else if (_docName == null)
                {
                    _docName = DisplayName;
                }
                return _docName;
            }
        }

        public string DocumentNameString { get; set; }

        public override string Width { get; set; }

        public override string Height { get; set; }

        public override string Left { get; set; }

        public override string Top { get; set; }

        public override string Title { get; set; }

        public override float LeftPt { get; set; }

        public override float TopPt { get; set; }

        public bool IsLandscape { get; set; }

        public bool RenderReportFooterInBottom { get; set; }

        public string BackgroundImage { get; set; }

        public List<EbReportField> ReportObjects { get; set; }

        public List<EbReportHeader> ReportHeaders { get; set; }

        public List<EbReportFooter> ReportFooters { get; set; }

        public List<EbPageHeader> PageHeaders { get; set; }

        public List<EbPageFooter> PageFooters { get; set; }

        public List<EbReportDetail> Detail { get; set; }

        public List<EbReportGroup> ReportGroups { set; get; }

        //public EbDataReader EbDataSource { get; set; }

        public string DataSourceRefId { get; set; }

        public EbFont Font { get; set; }

        public int DetailTableIndex { get; set; } = 0;

        public int MaxRowCount { get; set; } = 0;

        public bool HasRows = false;

        public int iDetailRowPos { get; set; }

        public Dictionary<string, List<EbDataField>> PageSummaryFields { get; set; }

        public Dictionary<string, List<EbDataField>> ReportSummaryFields { get; set; }

        public Dictionary<string, List<EbDataField>> GroupSummaryFields { get; set; }

        public Dictionary<int, byte[]> WatermarkImages { get; set; }

        public List<object> WaterMarkList { get; set; }

        public Dictionary<string, string> ValueScriptCollection { get; set; }

        public Dictionary<string, string> AppearanceScriptCollection { get; set; }

        public Dictionary<string, ReportGroupItem> Groupheaders { get; set; }

        public Dictionary<string, ReportGroupItem> GroupFooters { get; set; }

        public EbDataSet DataSet { get; set; }

        public bool IsLastpage { get; set; }

        public int PageNumber { get; set; }

        private DateTime _currentTimeStamp = DateTime.MinValue;

        public DateTime CurrentTimestamp
        {
            get
            {
                if (_currentTimeStamp == DateTime.MinValue)
                {
                    _currentTimeStamp = DateTime.Now;
                }
                return _currentTimeStamp;
            }
        }


        public User ReadingUser { get; set; }

        public CultureInfo CultureInfo { get; set; }

        public PdfContentByte Canvas { get; set; }

        public PdfWriter Writer { get; set; }

        public Document Doc { get; set; }

        public PdfReader PdfReader { get; set; }

        public PdfStamper Stamp { get; set; }

        public MemoryStream Ms1 { get; set; }

        public Eb_Solution Solution { get; set; }

        private float _rhHeight = 0;

        public float ReportHeaderHeight
        {
            get
            {
                if (_rhHeight == 0)
                {
                    foreach (EbReportHeader r_header in ReportHeaders)
                        _rhHeight += r_header.HeightPt;
                }

                return _rhHeight;
            }
        }

        private float _phHeight = 0;
        public float PageHeaderHeight
        {
            get
            {
                if (_phHeight == 0)
                {
                    foreach (EbPageHeader p_header in PageHeaders)
                        _phHeight += p_header.HeightPt;
                }
                return _phHeight;
            }
        }

        private float _pfHeight = 0;
        public float PageFooterHeight
        {
            get
            {
                if (_pfHeight == 0)
                {

                    foreach (EbPageFooter p_footer in PageFooters)
                        _pfHeight += p_footer.HeightPt;
                }
                return _pfHeight;
            }
        }

        private float _rfHeight = 0;
        public float ReportFooterHeight
        {
            get
            {
                if (_rfHeight == 0)
                {
                    foreach (EbReportFooter r_footer in ReportFooters)
                        _rfHeight += r_footer.HeightPt;
                }
                return _rfHeight;
            }
        }

        private float _dtHeight = 0;
        public float DetailHeight
        {
            get
            {
                if (_dtHeight == 0)
                {
                    foreach (EbReportDetail detail in Detail)
                        _dtHeight += detail.HeightPt;
                }
                return _dtHeight + RowHeight;
            }
        }

        private float _ghHeight = 0;
        public float GroupHeaderHeight
        {
            get
            {
                if (_ghHeight == 0)
                {
                    foreach (EbReportGroup grp in ReportGroups)
                    {
                        _ghHeight += grp.GroupHeader.HeightPt;
                    }
                }
                return _ghHeight;
            }
        }

        private float _gfHeight = 0;
        public float GroupFooterHeight
        {
            get
            {
                if (_gfHeight == 0)
                {
                    foreach (EbReportGroup grp in ReportGroups)
                    {
                        _gfHeight += grp.GroupFooter.HeightPt;
                    }
                }
                return _gfHeight;
            }
        }

        private float dt_fillheight = 0;
        public float DT_FillHeight
        {
            get
            {
                Data.RowColletion rows = (DataSourceRefId != string.Empty) ? DataSet.Tables[0].Rows : null;

                if (PageNumber == 1 && IsLastpage)
                    dt_fillheight = HeightPt - (PageHeaderHeight + PageFooterHeight + ReportHeaderHeight + ReportFooterHeight + Margin.Top + Margin.Bottom);
                else if (PageNumber == 1)
                    dt_fillheight = HeightPt - (ReportHeaderHeight + Margin.Top + PageHeaderHeight + PageFooterHeight + Margin.Bottom);
                else if (IsLastpage == true)
                    dt_fillheight = HeightPt - (PageHeaderHeight + PageFooterHeight + Margin.Bottom + Margin.Top + ReportFooterHeight);
                else
                    dt_fillheight = HeightPt - (PageHeaderHeight + PageFooterHeight + Margin.Bottom + Margin.Top);
                return dt_fillheight;
            }
        }

        // public EbBaseService ReportService { get; set; }

        //public EbStaticFileClient FileClient { get; set; }

        public string SolutionId { get; set; }

        public float RowHeight { get; set; }

        public float MultiRowTop { get; set; }

        public List<Param> Parameters { get; set; }

        private float rh_Yposition;
        private float rf_Yposition;
        private float pf_Yposition;
        private float ph_Yposition;
        private float dt_Yposition;

        public float detailprintingtop = 0;

        public double detailEnd = 0;

        public bool FooterDrawn = false;

        private int PreviousGheadersSlNo { get; set; }

        //public Dictionary<string, List<EbControl>> LinkCollection { get; set; }

        public Dictionary<string, PdfNTV> CalcValInRow { get; set; } = new Dictionary<string, PdfNTV>();

        public Dictionary<string, PdfNTV> SummaryValInRow { get; set; } = new Dictionary<string, PdfNTV>();

        public dynamic GetDataFieldValue(string column_name, int i, int tableIndex)
        {
            dynamic value = null;
            int index;
            if (DataSet != null && DataSet.Tables.Count > 0 && DataSet.Tables[tableIndex].Rows != null)
            {
                index = (DataSet.Tables[tableIndex].Rows.Count > 1) ? i : 0;
                EbDbTypes type = (DataSet.Tables[tableIndex].Columns[column_name].Type);
                value = (type == EbDbTypes.Bytea) ? DataSet.Tables[tableIndex].Rows[index][column_name] : DataSet.Tables[tableIndex].Rows[index][column_name].ToString();
            }
            return value;
        }

        public void DrawWaterMark(Document d, PdfWriter writer)
        {
            if (ReportObjects != null)
            {
                foreach (EbReportField field in ReportObjects)
                {
                    DrawFields(field, 0, 0);
                }
            }
        }

        public void CallSummerize(EbDataField field, int serialnumber)
        {
            string column_val = string.Empty;
            EbPdfGlobals globals = new EbPdfGlobals();
            AddParamsNCalcsInGlobal(globals);

            if (field is EbCalcField)
            {
                column_val = (field as EbCalcField).GetCalcFieldValue(globals, DataSet, serialnumber, this);
            }
            else
            {
                column_val = GetDataFieldValue(field.ColumnName, serialnumber, field.TableIndex);
            }
            List<EbDataField> SummaryList;
            if (GroupSummaryFields.ContainsKey(field.Name))
            {
                SummaryList = GroupSummaryFields[field.Name];
                foreach (EbDataField item in SummaryList)
                {
                    (item as IEbDataFieldSummary).Summarize(column_val);
                }
            }
            if (PageSummaryFields.ContainsKey(field.Name))
            {
                SummaryList = PageSummaryFields[field.Name];
                foreach (EbDataField item in SummaryList)
                {
                    (item as IEbDataFieldSummary).Summarize(column_val);
                }
            }
            if (ReportSummaryFields.ContainsKey(field.Name))
            {
                SummaryList = ReportSummaryFields[field.Name];
                foreach (EbDataField item in SummaryList)
                {
                    (item as IEbDataFieldSummary).Summarize(column_val);
                }
            }

        }

        public void DrawReportHeader()
        {
            RowHeight = 0;
            MultiRowTop = 0;
            rh_Yposition = this.Margin.Top;
            detailprintingtop = 0;
            foreach (EbReportHeader r_header in ReportHeaders)
            {
                foreach (EbReportField field in r_header.GetFields())
                {
                    DrawFields(field, rh_Yposition, 0);
                }
                rh_Yposition += r_header.HeightPt;
            }
        }

        public void DrawPageHeader()
        {
            RowHeight = 0;
            MultiRowTop = 0;
            detailprintingtop = 0;
            ph_Yposition = (PageNumber == 1) ? ReportHeaderHeight + this.Margin.Top : this.Margin.Top;
            foreach (EbPageHeader p_header in PageHeaders)
            {
                foreach (EbReportField field in p_header.GetFields())
                {
                    DrawFields(field, ph_Yposition, 0);
                }
                ph_Yposition += p_header.HeightPt;
            }
        }

        public void DrawDetail()
        {
            RowColletion rows = (DataSourceRefId != string.Empty) ? DataSet.Tables[DetailTableIndex].Rows : null;

            ph_Yposition = (PageNumber == 1) ? ReportHeaderHeight + this.Margin.Top : this.Margin.Top;
            dt_Yposition = ph_Yposition + PageHeaderHeight;
            if (rows != null && HasRows == true)
            {
                for (iDetailRowPos = 0; iDetailRowPos < rows.Count; iDetailRowPos++)
                {
                    if (Groupheaders?.Count > 0)
                        DrawGroup();
                    if (detailprintingtop < DT_FillHeight && DT_FillHeight - detailprintingtop >= DetailHeight)
                    {
                        DoLoopInDetail(iDetailRowPos);
                    }
                    else
                    {
                        AddNewPage();
                        DoLoopInDetail(iDetailRowPos);
                    }
                }
                if (GroupFooters?.Count > 0)
                    EndGroups();
                IsLastpage = true;
            }
            else
            {
                IsLastpage = true;
                DoLoopInDetail(0);
            }
        }
        public void DrawGroup()
        {
            foreach (KeyValuePair<string, ReportGroupItem> grp in Groupheaders)
            {
                ReportGroupItem grpitem = grp.Value;
                string column_val = GetDataFieldValue(grpitem.field.ColumnName, iDetailRowPos, grpitem.field.TableIndex);

                if (grpitem.PreviousValue != column_val)
                {
                    if (iDetailRowPos > 0)
                        DrawGroupFooter(grpitem.order, iDetailRowPos);
                    DrawGroupHeader(grpitem.order, iDetailRowPos);
                    grpitem.PreviousValue = column_val;
                    int i;
                    for (i = grpitem.order + 1; i < Groupheaders.Count; i++)
                        Groupheaders.Values.ElementAt(i).PreviousValue = string.Empty;
                }
            }
        }
        public void EndGroups()
        {
            foreach (EbReportGroup grp in this.ReportGroups)
            {
                DrawGroupFooter(grp.GroupFooter.Order, iDetailRowPos);
                detailEnd += ReportGroups[grp.GroupFooter.Order].GroupFooter.HeightPt;
            }
        }
        private Dictionary<EbReportDetail, EbDataField[]> __fieldsNotSummaryPerDetail = null;
        private Dictionary<EbReportDetail, EbDataField[]> FieldsNotSummaryPerDetail
        {
            get
            {
                if (__fieldsNotSummaryPerDetail == null)
                {
                    __fieldsNotSummaryPerDetail = new Dictionary<EbReportDetail, EbDataField[]>();
                    foreach (EbReportDetail detail in Detail)
                        __fieldsNotSummaryPerDetail[detail] = detail.GetFields().Where(x => (x is EbDataField && !(x is IEbDataFieldSummary))).OrderBy(o => o.Top).Cast<EbDataField>().ToArray();
                }

                return __fieldsNotSummaryPerDetail;
            }
        }

        private Dictionary<EbReportDetail, EbReportField[]> __reportFieldsSortedPerDetail = null;
        private Dictionary<EbReportDetail, EbReportField[]> ReportFieldsSortedPerDetail
        {
            get
            {
                if (__reportFieldsSortedPerDetail == null)
                {
                    __reportFieldsSortedPerDetail = new Dictionary<EbReportDetail, EbReportField[]>();
                    foreach (EbReportDetail detail in Detail)
                        __reportFieldsSortedPerDetail[detail] = detail.GetFields().OrderBy(o => o.Left).ToArray();
                }

                return __reportFieldsSortedPerDetail;
            }
        }
        private Dictionary<EbReportFooter, EbReportField[]> __reportFieldsSortedPerRFooter = null;
        private Dictionary<EbReportFooter, EbReportField[]> ReportFieldsSortedPerRFooter
        {
            get
            {
                if (__reportFieldsSortedPerRFooter == null)
                {
                    __reportFieldsSortedPerRFooter = new Dictionary<EbReportFooter, EbReportField[]>();
                    foreach (EbReportFooter _footer in ReportFooters)
                    {
                        __reportFieldsSortedPerRFooter[_footer] = _footer.GetFields().OrderBy(o => o.TopPt).ToArray();
                    }
                }

                return __reportFieldsSortedPerRFooter;
            }
        }

        public void AddNewPage()
        {
            detailprintingtop = 0;
            Doc.NewPage();
            ph_Yposition = this.Margin.Top;
            PageNumber = Writer.PageNumber;
        }

        public void DoLoopInDetail(int serialnumber)
        {
            ph_Yposition = (PageNumber == 1) ? ReportHeaderHeight + this.Margin.Top : this.Margin.Top;
            dt_Yposition = ph_Yposition + PageHeaderHeight;
            foreach (EbReportDetail detail in Detail)
            {
                string column_val;
                RowHeight = 0;
                MultiRowTop = 0;
                EbDataField[] SortedList = FieldsNotSummaryPerDetail[detail];
                EbPdfGlobals globals = new EbPdfGlobals();
                for (int iSortPos = 0; iSortPos < SortedList.Length; iSortPos++)
                {
                    EbDataField field = SortedList[iSortPos];
                    if (field is EbCalcField)
                    {
                        globals.CurrentField = field;
                        column_val = (field as EbCalcField).GetCalcFieldValue(globals, DataSet, serialnumber, this);
                        EbDbTypes dbtype = (EbDbTypes)((field as EbCalcField).CalcFieldIntType);

                        if (CalcValInRow.ContainsKey(field.Title))
                            CalcValInRow[field.Title] = new PdfNTV { Name = field.Title, Type = (PdfEbDbTypes)(int)dbtype, Value = column_val };
                        else
                            CalcValInRow.Add(field.Title, new PdfNTV { Name = field.Title, Type = (PdfEbDbTypes)(int)dbtype, Value = column_val });
                        AddParamsNCalcsInGlobal(globals);
                    }
                    else
                    {
                        column_val = GetDataFieldValue(field.ColumnName, serialnumber, field.TableIndex);
                    }

                    if (field.RenderInMultiLine)
                        field.DoRenderInMultiLine(column_val, this);
                }
                EbReportField[] SortedReportFields = this.ReportFieldsSortedPerDetail[detail];
                if (SortedReportFields.Length > 0)
                {
                    for (int iSortPos = 0; iSortPos < SortedReportFields.Length; iSortPos++)
                    {
                        EbReportField field = SortedReportFields[iSortPos];
                        if (field is EbDataField)
                            field.HeightPt += RowHeight;
                        DrawFields(field, dt_Yposition, serialnumber);
                    }
                    detailprintingtop += detail.HeightPt + RowHeight;
                    detailEnd = detailprintingtop;
                }
                else
                {
                    detailEnd = detailprintingtop;
                    IsLastpage = true;
                    Writer.PageEvent.OnEndPage(Writer, Doc);
                    return;
                }
            }
        }

        public void DrawGroupHeader(int order, int serialnumber)
        {
            if ((PreviousGheadersSlNo != serialnumber && GroupHeaderHeight + DetailHeight > DT_FillHeight - detailprintingtop) || (ReportGroups[order].GroupHeader.GroupInNewPage && serialnumber > 0))
            {
                AddNewPage();
                dt_Yposition = PageHeaderHeight + this.Margin.Top;
            }

            foreach (EbReportField field in ReportGroups[order].GroupHeader.GetFields())
            {
                DrawFields(field, dt_Yposition, serialnumber);
            }
            detailprintingtop += ReportGroups[order].GroupHeader.HeightPt;
            PreviousGheadersSlNo = serialnumber;
        }

        public void DrawGroupFooter(int order, int serialnumber)
        {
            foreach (EbReportField field in ReportGroups[order].GroupFooter.GetFields())
            {
                DrawFields(field, dt_Yposition, serialnumber);
            }
            detailprintingtop += ReportGroups[order].GroupFooter.HeightPt;
        }

        public void DrawPageFooter()
        {
            RowHeight = 0;
            MultiRowTop = 0;
            detailprintingtop = 0;
            ph_Yposition = (PageNumber == 1) ? ReportHeaderHeight + this.Margin.Top : this.Margin.Top;
            dt_Yposition = ph_Yposition + PageHeaderHeight;
            //  pf_Yposition = dt_Yposition + DT_FillHeight;
            pf_Yposition = (float)detailEnd + /*DetailHeight +*/ dt_Yposition;
            foreach (EbPageFooter p_footer in PageFooters)
            {
                foreach (EbReportField field in p_footer.GetFields())
                {
                    DrawFields(field, pf_Yposition, 0);
                }
                pf_Yposition += p_footer.HeightPt;
            }
        }

        public void DrawReportFooter()
        {
            RowHeight = 0;
            MultiRowTop = 0;
            detailprintingtop = 0;
            dt_Yposition = ph_Yposition + PageHeaderHeight;
            //pf_Yposition = dt_Yposition + DT_FillHeight;
            pf_Yposition = (float)detailEnd /*+ DetailHeight*/ + dt_Yposition;
            if (RenderReportFooterInBottom)
            {
                rf_Yposition = HeightPt - ReportFooterHeight;
            }
            else
            {
                rf_Yposition = pf_Yposition + PageFooterHeight;
            }
            foreach (EbReportFooter r_footer in ReportFooters)
            {
                float footer_diffrence = 0;
                EbReportField[] SortedReportFields = this.ReportFieldsSortedPerRFooter[r_footer];
                if (SortedReportFields.Length > 0)
                {
                    for (int iSortPos = 0; iSortPos < SortedReportFields.Length; iSortPos++)
                    {
                        EbReportField field = SortedReportFields[iSortPos];
                        // if (HeightPt - rf_Yposition + Margin.Top < field.TopPt)
                        if (HeightPt < field.TopPt + rf_Yposition + field.HeightPt + Margin.Bottom)
                        {
                            AddNewPage();
                            //footer_diffrence = HeightPt - rf_Yposition - Margin.Bottom;
                            footer_diffrence = field.TopPt;
                            FooterDrawn = true;
                            rf_Yposition = Margin.Top;
                        }
                        field.TopPt -= footer_diffrence;
                        DrawFields(field, rf_Yposition, 0);
                    }
                }
                rf_Yposition += r_footer.HeightPt;
            }
        }

        public void DrawFields(EbReportField field, float section_Yposition, int serialnumber)
        {
            if (!field.IsHidden)
            {
                List<Param> RowParams = null;
                if (field is EbDataField)
                {
                    EbDataField field_org = field as EbDataField;
                    if (GroupSummaryFields.ContainsKey(field.Name) || PageSummaryFields.ContainsKey(field.Name) || ReportSummaryFields.ContainsKey(field.Name))
                        CallSummerize(field_org, serialnumber);
                    if (AppearanceScriptCollection.ContainsKey(field.Name))
                        RunAppearanceExpression(field_org, serialnumber);
                    //if (!string.IsNullOrEmpty(field_org.LinkRefId))
                    //    RowParams = CreateRowParamForLink(field_org, serialnumber);
                }
                field.DrawMe(section_Yposition, this, RowParams, serialnumber);
            }
        }

        public void RunAppearanceExpression(EbDataField field, int slno)
        {
            if (field.Font is null || field.Font.Size == 0)
                field.Font = new EbFont { Color = "#000000", FontName = "Roboto", Caps = false, Size = 10, Strikethrough = false, Style = Enums.FontStyle.NORMAL, Underline = false };
            PdfGEbFont pg_font = new PdfGEbFont
            {
                Caps = field.Font.Caps,
                color = field.Font.Color,
                FontName = field.Font.FontName,
                Size = field.Font.Size,
                Strikethrough = field.Font.Strikethrough,
                Style = (PdfGFontStyle)(int)field.Font.Style,
                Underline = field.Font.Underline
            };
            EbPdfGlobals globals = new EbPdfGlobals
            {
                CurrentField = new PdfGReportField(field.LeftPt, field.WidthPt, field.TopPt, field.HeightPt, field.BackColor, field.ForeColor, field.IsHidden, pg_font)
            };

            AddParamsNCalcsInGlobal(globals);

            foreach (string calcfd in field.DataFieldsUsedAppearance)
            {
                string TName = calcfd.Split('.')[0];
                int TableIndex = Convert.ToInt32(TName.Substring(1));
                string fName = calcfd.Split('.')[1];
                globals[TName].Add(fName, new PdfNTV { Name = fName, Type = (PdfEbDbTypes)(int)DataSet.Tables[TableIndex].Columns[fName].Type, Value = DataSet.Tables[TableIndex].Rows[slno][fName] });
            }
            //AppearanceScriptCollection[field.Name].RunAsync(globals);
            field.SetValuesFromGlobals(globals.CurrentField);
        }

        //public List<Param> CreateRowParamForLink(EbDataField field, int slno)
        //{
        //    List<Param> RowParams = new List<Param>();
        //    foreach (EbControl control in LinkCollection[field.LinkRefId])
        //    {
        //        Param x = DataSet.Tables[field.TableIndex].Rows[slno].GetCellParam(control.Name);
        //        ArrayList IndexToRemove = new ArrayList();
        //        for (int i = 0; i < RowParams.Count; i++)
        //        {
        //            if (RowParams[i].Name == control.Name)
        //            {
        //                IndexToRemove.Add(i); //the parameter will be in the report alredy
        //            }
        //        }
        //        for (int i = 0; i < IndexToRemove.Count; i++)
        //        {
        //            RowParams.RemoveAt(Convert.ToInt32(IndexToRemove[i]));
        //        }
        //        RowParams.Add(x);
        //    }
        //    if (!Parameters.IsEmpty())//the parameters which are alredy present in the rendering of current report
        //    {
        //        foreach (Param p in Parameters)
        //        {
        //            RowParams.Add(p);
        //        }
        //    }
        //    return RowParams;
        //}

        //public void GetWatermarkImages()
        //{
        //    if (this.ReportObjects != null)
        //    {
        //        foreach (EbReportField field in ReportObjects)
        //        {
        //            if (field is EbWaterMark)
        //            {
        //                int id = (field as EbWaterMark).ImageRefId;
        //                if (id != 0)
        //                {
        //                    byte[] fileByte = GetImage(id);
        //                    if (!fileByte.IsEmpty())
        //                        WatermarkImages.Add(id, fileByte);
        //                }
        //            }
        //        }
        //    }
        //}

        public void SetPassword()
        {
            Ms1.Position = 0;
            PdfReader = new PdfReader(Ms1);
            Stamp = new PdfStamper(PdfReader, Ms1);
            byte[] USER = Encoding.ASCII.GetBytes(UserPassword);
            byte[] OWNER = Encoding.ASCII.GetBytes(OwnerPassword);
            Stamp.SetEncryption(USER, OWNER, 0, PdfWriter.ENCRYPTION_AES_128);
            Stamp.FormFlattening = true;
            Stamp.Close();
        }

        public void SetDetail()
        {
            string timestamp = String.Format("{0:" + CultureInfo.DateTimeFormat.FullDateTimePattern + "}", CurrentTimestamp);
            ColumnText ct = new ColumnText(Canvas);
            Phrase phrase = new Phrase("page:" + PageNumber.ToString() + ", " + App.Settings.CurrentUser?.FullName ?? "Machine User" + ", " + timestamp);
            phrase.Font.Size = 6;
            phrase.Font.Color = Color.GRAY;
            ct.SetSimpleColumn(phrase, 5, 2 + Margin.Bottom, (WidthPt - 20 - Margin.Left) - 20, 20 + Margin.Bottom, 15, Element.ALIGN_RIGHT);
            ct.Go();
        }

        public EbReport()
        {
            ReportHeaders = new List<EbReportHeader>();

            PageHeaders = new List<EbPageHeader>();

            Detail = new List<EbReportDetail>();

            PageFooters = new List<EbPageFooter>();

            ReportFooters = new List<EbReportFooter>();

            ReportGroups = new List<EbReportGroup>();
        }

        // public static EbOperations Operations = ReportOperations.Instance;

        //public byte[] GetImage(int refId)
        //{
        //    DownloadFileResponse dfs = null;

        //    byte[] fileByte = new byte[0];
        //    dfs = FileClient.Get
        //         (new DownloadImageByIdRequest
        //         {
        //             ImageInfo = new ImageMeta
        //             {
        //                 FileRefId = refId,
        //                 FileCategory = Enums.EbFileCategory.Images
        //             }
        //         });
        //    if (dfs.StreamWrapper != null)
        //    {
        //        dfs.StreamWrapper.Memorystream.Position = 0;
        //        fileByte = dfs.StreamWrapper.Memorystream.ToBytes();
        //    }

        //    return fileByte;
        //}

        public void AddParamsNCalcsInGlobal(EbPdfGlobals globals)
        {
            foreach (string key in CalcValInRow.Keys) //adding Calc to global
            {
                globals["Calc"].Add(key, CalcValInRow[key]);
            }

            if (Parameters != null)
                foreach (Param p in Parameters) //adding Params to global
                {
                    globals["Params"].Add(p.Name, new PdfNTV { Name = p.Name, Type = (PdfEbDbTypes)Convert.ToInt32(p.Type), Value = p.Value });
                }

            if (SummaryValInRow.Count > 0)
                foreach (string key in SummaryValInRow.Keys)
                {
                    globals["Summary"].Add(key.Replace(".", "_"), SummaryValInRow[key]);
                }
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

        public override string Left { get; set; }

        public override string Top { get; set; }

        public override float LeftPt { get; set; }

        public override float TopPt { get; set; }

        public override string Title { get; set; }

        public override string Height { get; set; }

        public override string Width { get; set; }

        private List<EbReportField> _list = null;
        public List<EbReportField> GetFields()
        {
            if (_list == null)
            {
                _list = new List<EbReportField>();
                foreach (EbReportField f in Fields)
                {
                    if (f is EbTable_Layout)
                    {
                        foreach (EbTableLayoutCell c in (f as EbTable_Layout).CellCollection)
                        {
                            foreach (EbReportField r in c.ControlCollection)
                                _list.Add(r);
                        }
                    }
                    else
                        _list.Add(f);
                }
            }
            return _list;
        }
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

        public override string Height { get; set; }

        public override string Width { get; set; }

        public override string Left { get; set; }

        public override string Top { get; set; }

        public override float LeftPt { get; set; }

        public override float TopPt { get; set; }

        public override string Title { get; set; }

        public override string BackColor { get; set; }

        public override float HeightPt { get; set; }

        public override float WidthPt { get; set; }
    }

    public class EbGroupHeader : EbReportSection
    {
        public int Order { set; get; }

        public bool GroupInNewPage { get; set; }
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
