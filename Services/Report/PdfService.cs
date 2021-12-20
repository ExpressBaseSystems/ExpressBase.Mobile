using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers;
using ExpressBase.Mobile.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

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
                    //List<string> Groupings = null;
                    //  EbObjectService myObjectservice = base.ResolveService<EbObjectService>();
                    //  myObjectservice.EbConnectionFactory = this.EbConnectionFactory;
                    //  DataSourceService myDataSourceservice = base.ResolveService<DataSourceService>();
                    // myDataSourceservice.EbConnectionFactory = this.EbConnectionFactory;

                    // EbObjectParticularVersionResponse resultlist = myObjectservice.Get(new EbObjectParticularVersionRequest { RefId = request.Refid }) as EbObjectParticularVersionResponse;
                    Report = (EbReport)EbPageHelper.GetWebObjects(refid);

                    //Report.ReportService = this;
                    //Report.SolutionId = request.SolnId;
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
                    //Report.FileClient = new EbStaticFileClient();
                    //if (string.IsNullOrEmpty(FileClient.BearerToken) && !string.IsNullOrEmpty(request.BToken))
                    //{
                    //    FileClient.BearerToken = request.BToken;
                    //    FileClient.RefreshToken = request.RToken;
                    //}
                    //Report.FileClient = FileClient;
                    //Report.Solution = GetSolutionObject(request.SolnId);
                    // Report.CultureInfo = CultureHelper.GetSerializedCultureInfo(App.Settings.CurrentUser?.Preference.Locale ?? "en-US").GetCultureInfo();
                    Report.Parameters = JsonConvert.DeserializeObject<List<Param>>(param);
                    Report.Ms1 = new MemoryStream();
                    if (Report.DataSourceRefId != string.Empty)
                    {
                        //Groupings = new List<string>();
                        // Report.DataSet = App.DataDB.DoQueries(Report.OfflineQuery, Report.Parameters.ToArray());                       
                    }
                    float _width = Report.WidthPt - Report.Margin.Left;// - Report.Margin.Right;
                    float _height = Report.HeightPt - Report.Margin.Top - Report.Margin.Bottom;
                    Report.HeightPt = _height;
                    //iTextSharp.text.Rectangle rec =new iTextSharp.text.Rectangle(Report.WidthPt, Report.HeightPt);
                    iTextSharp.text.Rectangle rec = new iTextSharp.text.Rectangle(_width, _height);
                    Report.Doc = new Document(rec);
                    Report.Doc.SetMargins(Report.Margin.Left, Report.Margin.Right, Report.Margin.Top, Report.Margin.Bottom);
                    Report.Writer = PdfWriter.GetInstance(Report.Doc, Report.Ms1);
                    Report.Writer.Open();
                    Report.Doc.Open();
                    Report.Doc.AddTitle(Report.DisplayName);
                    Report.Writer.PageEvent = new HeaderFooter(Report);
                    Report.Writer.CloseStream = true;//important
                    Report.Canvas = Report.Writer.DirectContent;
                    Report.PageNumber = Report.Writer.PageNumber;
                    //Report.GetWatermarkImages();
                    FillingCollections(Report);
                    Report.Doc.NewPage();
                    Report.DrawReportHeader();
                    if (Report?.DataSet?.Tables[Report.DetailTableIndex]?.Rows.Count > 0)
                    {
                        Report.DrawDetail();
                    }
                    else
                    {
                        Report.DrawPageHeader();
                        Report.DrawPageFooter();
                        throw new Exception("Dataset is null, refid " + Report.DataSourceRefId);
                    }
                    Report.DrawReportFooter();
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Exception-reportService " + e.Message + e.StackTrace);
                    Console.ForegroundColor = ConsoleColor.White;

                    ColumnText ct = new ColumnText(Report.Canvas);
                    Phrase phrase = new Phrase("No Data available. Please check the parameters or contact admin");
                    phrase.Font.Size = 10;
                    ct.SetSimpleColumn(phrase, Report.LeftPt + 30, Report.HeightPt - 80, Report.WidthPt - 30, Report.HeightPt - 40, 15, Element.ALIGN_CENTER);
                    ct.Go();
                }

                Report.Doc.Close();
                if (Report.UserPassword != string.Empty || Report.OwnerPassword != string.Empty)
                    Report.SetPassword();
                Report.Ms1.Position = 0;//important
                if (Report.DataSourceRefId != string.Empty)
                {
                    Report.DataSet.Tables.Clear();
                    Report.DataSet = null;
                }

                return new ReportRenderResponse
                {
                    ReportName = Report.DisplayName,
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
            //foreach (EbReportField field in fields)
            //{
            //    if (!String.IsNullOrEmpty(field.HideExpression?.Code))
            //    {
            //        ExecuteHideExpression(Report, field);
            //    }
            //    if (!field.IsHidden && !String.IsNullOrEmpty(field.LayoutExpression?.Code))
            //    {
            //        ExecuteLayoutExpression(Report, field);
            //    }
            //    if (field is EbDataField)
            //    {
            //        EbDataField field_org = field as EbDataField;
            //        if (!string.IsNullOrEmpty(field_org.LinkRefId) && !Report.LinkCollection.ContainsKey(field_org.LinkRefId))
            //            FindControls(Report, field_org);//Finding the link's parameter controls

            //        if (section_typ == EbReportSectionType.Detail)
            //            FindLargerDataTable(Report, field_org);// finding the table of highest rowcount from dataset

            //        if (field is IEbDataFieldSummary)
            //            FillSummaryCollection(Report, field_org, section_typ);

            //        if (field is EbCalcField && !Report.ValueScriptCollection.ContainsKey(field.Name) && !string.IsNullOrEmpty((field_org as EbCalcField).ValExpression?.Code))
            //        {
            //            Script valscript = CompileScript((field as EbCalcField).ValExpression.Code);
            //            Report.ValueScriptCollection.Add(field.Name, valscript);
            //        }

            //        if (!field.IsHidden && !Report.AppearanceScriptCollection.ContainsKey(field.Name) && !string.IsNullOrEmpty(field_org.AppearExpression?.Code))
            //        {
            //            Script appearscript = CompileScript(field_org.AppearExpression.Code);
            //            Report.AppearanceScriptCollection.Add(field.Name, appearscript);
            //        }
            //    }
            //}
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
