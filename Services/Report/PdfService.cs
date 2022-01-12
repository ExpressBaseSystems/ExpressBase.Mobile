using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Extensions;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Helpers.Script;
using ExpressBase.Mobile.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace ExpressBase.Mobile.Services
{
    public class PdfService : BaseService
    {
        public PdfService() : base(true)
        {
        }


        public async Task<ReportRenderResponse> GetPdfOnline(string refid, string param)
        {
            ReportRenderResponse resp = null;
            try
            {
                RestRequest request = new RestRequest(ApiConstants.GET_PDF, Method.GET);
                // auth Headers for api
                request.AddHeader(AppConst.BTOKEN, App.Settings.BToken);
                request.AddHeader(AppConst.RTOKEN, App.Settings.RToken);

                request.AddParameter("refid", refid);
                request.AddParameter("param", param);

                IRestResponse response = await HttpClient.ExecuteAsync(request);

                if (response.IsSuccessful)
                    resp = JsonConvert.DeserializeObject<ReportRenderResponse>(response.Content);
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }
            return resp;
        }

        public ReportRenderResponse GetPdfOffline(string refid, string param)
        {
            if (!string.IsNullOrEmpty(refid))
            {
                EbReport Report = null;
                try
                {
                    Report = (EbReport)EbPageHelper.GetWebObjects(refid);

                    Report.IsLastpage = false;
                    Report.WatermarkImages = new Dictionary<int, byte[]>();
                    Report.WaterMarkList = new List<object>();
                    Report.ValueScriptCollection = new Dictionary<string, string>();
                    Report.AppearanceScriptCollection = new Dictionary<string, string>();
                    // Report.LinkCollection = new Dictionary<string, List<Common.Objects.EbControl>>();
                    Report.GroupSummaryFields = new Dictionary<string, List<EbDataField>>();
                    Report.PageSummaryFields = new Dictionary<string, List<EbDataField>>();
                    Report.ReportSummaryFields = new Dictionary<string, List<EbDataField>>();
                    Report.GroupFooters = new Dictionary<string, ReportGroupItem>();
                    Report.Groupheaders = new Dictionary<string, ReportGroupItem>();

                    Report.CultureInfo = new CultureInfo("en-IN", false);
                    Report.Parameters = JsonConvert.DeserializeObject<List<Param>>(param);
                    Report.Ms1 = new MemoryStream();
                    if (Report?.OfflineQuery.Code != string.Empty)
                    {
                        string query = Report.OfflineQuery.GetCode();
                        Report.DataSet = App.DataDB.DoQueries(query, Report.Parameters.ToDbParams().ToArray());
                    }
                    float _width = Report.WidthPt - Report.Margin.Left;// - Report.Margin.Right;
                    float _height = Report.HeightPt - Report.Margin.Top - Report.Margin.Bottom;
                    Report.HeightPt = _height;

                    iTextSharp.text.Rectangle rec = new iTextSharp.text.Rectangle(_width, _height);
                    Report.Doc = new Document(rec);
                    Report.Doc.SetMargins(Report.Margin.Left, Report.Margin.Right, Report.Margin.Top, Report.Margin.Bottom);
                    Report.Writer = PdfWriter.GetInstance(Report.Doc, Report.Ms1);
                    Report.Writer.Open();
                    Report.Doc.Open();
                    Report.Doc.AddTitle(Report.DocumentName);
                    Report.Writer.PageEvent = new HeaderFooter(Report);
                    Report.Writer.CloseStream = true;//important
                    Report.Canvas = Report.Writer.DirectContent;
                    Report.PageNumber = Report.Writer.PageNumber;
                    FillingCollections(Report);
                    Report.Doc.NewPage();
                    Report.DrawReportHeader();

                    if (Report?.DataSet?.Tables.Count > 0 && Report?.DataSet?.Tables[Report.DetailTableIndex]?.Rows.Count > 0)
                    {
                        Report.DrawDetail();
                    }
                    else
                    {
                        Report.DrawPageHeader();
                        Report.detailEnd += 30;
                        Report.DrawPageFooter();
                        Report.DrawReportFooter();
                        throw new Exception("Dataset is null, refid " + Report.DataSourceRefId);
                    }

                    Report.DrawReportFooter();
                }
                catch (Exception e)
                {
                    ColumnText ct = new ColumnText(Report.Canvas);
                    Phrase phrase;
                    if (Report?.DataSet?.Tables[Report.DetailTableIndex]?.Rows.Count > 0)
                    {
                        phrase = new Phrase("Something went wrong. Please check the parameters or contact admin");
                    }
                    else
                    {
                        phrase = new Phrase("No Data available. Please check the parameters or contact admin");
                    }
                    phrase.Font.Size = 10;
                    float y = Report.HeightPt - (Report.ReportHeaderHeight + Report.Margin.Top + Report.PageHeaderHeight);
                    ct.SetSimpleColumn(phrase, Report.LeftPt + 30, y - 30, Report.WidthPt - 30, y, 15, Element.ALIGN_CENTER);
                    ct.Go();
                }

                Report.Doc.Close();
                if (Report.UserPassword != string.Empty || Report.OwnerPassword != string.Empty)
                    Report.SetPassword();

                string name = Report.DocumentName;

                if (Report.DataSourceRefId != string.Empty)
                {
                    Report.DataSet.Tables.Clear();
                    Report.DataSet = null;
                }

                return new ReportRenderResponse
                {
                    ReportName = name,
                    ReportBytea = Report.Ms1.ToArray(),
                    CurrentTimestamp = Report.CurrentTimestamp
                };
            }
            else
            {
                EbLog.Error(" PDF report Refid is null");
                return null;
            }
        }

        public void FillingCollections(EbReport Report)
        {
            foreach (EbReportHeader r_header in Report.ReportHeaders)
                Fill(Report, r_header.GetFields(), EbReportSectionType.ReportHeader);

            foreach (EbReportFooter r_footer in Report.ReportFooters)
                Fill(Report, r_footer.GetFields(), EbReportSectionType.ReportFooter);

            foreach (EbPageHeader p_header in Report.PageHeaders)
                Fill(Report, p_header.GetFields(), EbReportSectionType.PageHeader);

            foreach (EbReportDetail detail in Report.Detail)
                Fill(Report, detail.GetFields(), EbReportSectionType.Detail);

            foreach (EbPageFooter p_footer in Report.PageFooters)
                Fill(Report, p_footer.GetFields(), EbReportSectionType.PageFooter);

            foreach (EbReportGroup group in Report.ReportGroups)
            {
                Fill(Report, group.GroupHeader.GetFields(), EbReportSectionType.ReportGroups);
                Fill(Report, group.GroupFooter.GetFields(), EbReportSectionType.ReportGroups);
                foreach (EbReportField field in group.GroupHeader.GetFields())
                {
                    if (field is EbDataField)
                    {
                        Report.Groupheaders.Add((field as EbDataField).ColumnName, new ReportGroupItem
                        {
                            field = field as EbDataField,
                            PreviousValue = string.Empty,
                            order = group.GroupHeader.Order
                        });
                    }
                }
                foreach (EbReportField field in group.GroupFooter.GetFields())
                {
                    if (field is EbDataField)
                    {
                        Report.GroupFooters.Add((field as EbDataField).Name, new ReportGroupItem
                        {
                            field = field as EbDataField,
                            PreviousValue = string.Empty,
                            order = group.GroupHeader.Order
                        });
                    }
                }
            }

        }

        private void Fill(EbReport Report, List<EbReportField> fields, EbReportSectionType section_typ)
        {
            foreach (EbReportField field in fields)
            {
                if (!String.IsNullOrEmpty(field.HideExpression?.GetCode()))
                {
                    Report.ExecuteHideExpression(field);
                }
                if (!field.IsHidden && !String.IsNullOrEmpty(field.LayoutExpression?.GetCode()))
                {
                    Report.ExecuteLayoutExpression(field);
                }
                if (field is EbDataField)
                {
                    EbDataField field_org = field as EbDataField;

                    if (section_typ == EbReportSectionType.Detail)
                        FindLargerDataTable(Report, field_org);

                    if (field is IEbDataFieldSummary)
                        FillSummaryCollection(Report, field_org, section_typ);

                    if (field is EbCalcField && !Report.ValueScriptCollection.ContainsKey(field.Name) && !string.IsNullOrEmpty((field_org as EbCalcField).ValExpression?.Code))
                    {
                        string processedCode = Report.GetProcessedCodeForScriptCollection((field as EbCalcField).ValExpression.GetCode());
                        Report.ValueScriptCollection.Add(field.Name, processedCode);
                    }

                    if (!field.IsHidden && !Report.AppearanceScriptCollection.ContainsKey(field.Name) && !string.IsNullOrEmpty(field_org.AppearExpression?.Code))
                    {
                        string processedCode = Report.GetProcessedCodeForScriptCollection(field_org.AppearExpression.GetCode());
                        Report.AppearanceScriptCollection.Add(field.Name, processedCode);
                    }
                }
            }
        }



        public void FindLargerDataTable(EbReport Report, EbDataField field)
        {
            if (!Report.HasRows || field.TableIndex != Report.DetailTableIndex)
            {
                if (Report.DataSet?.Tables.Count > 0)
                {
                    if (Report.DataSet.Tables[field.TableIndex].Rows != null)
                    {
                        Report.HasRows = true;
                        int r_count = Report.DataSet.Tables[field.TableIndex].Rows.Count;
                        Report.DetailTableIndex = (r_count > Report.MaxRowCount) ? field.TableIndex : Report.DetailTableIndex;
                        Report.MaxRowCount = (r_count > Report.MaxRowCount) ? r_count : Report.MaxRowCount;
                    }
                    else
                    {
                        Console.WriteLine("Report.DataSet.Tables[field.TableIndex].Rows is null");
                    }
                }
                else
                {
                    Console.WriteLine("Report.DataSet.Tables.Count is 0");
                }
            }
        }

        public void FillSummaryCollection(EbReport report, EbDataField field, EbReportSectionType section_typ)
        {
            if (section_typ == EbReportSectionType.ReportGroups)
            {
                if (!report.GroupSummaryFields.ContainsKey(field.SummaryOf))
                {
                    report.GroupSummaryFields.Add(field.SummaryOf, new List<EbDataField> { field });
                }
                else
                {
                    report.GroupSummaryFields[field.SummaryOf].Add(field);
                }
            }
            if (section_typ == EbReportSectionType.PageFooter)
            {
                if (!report.PageSummaryFields.ContainsKey(field.SummaryOf))
                {
                    report.PageSummaryFields.Add(field.SummaryOf, new List<EbDataField> { field });
                }
                else
                {
                    report.PageSummaryFields[field.SummaryOf].Add(field);
                }
            }
            if (section_typ == EbReportSectionType.ReportFooter)
            {
                if (!report.ReportSummaryFields.ContainsKey(field.SummaryOf))
                {
                    report.ReportSummaryFields.Add(field.SummaryOf, new List<EbDataField> { field });
                }
                else
                {
                    report.ReportSummaryFields[field.SummaryOf].Add(field);
                }
            }
        }
    }

    public partial class HeaderFooter : PdfPageEventHelper
    {
        private EbReport Report { get; set; }

        public override void OnStartPage(PdfWriter writer, Document document)
        {
        }

        public override void OnEndPage(PdfWriter writer, Document d)
        {
            if (!Report.FooterDrawn && (Report?.DataSet?.Tables[Report.DetailTableIndex]?.Rows.Count > 0))
            {
                Report.DrawPageHeader();
                Report.DrawPageFooter();
            }
            Report.DrawWaterMark(d, writer);
            Report.SetDetail();
        }

        public HeaderFooter(EbReport _c) : base()
        {
            Report = _c;
        }
    }
}
