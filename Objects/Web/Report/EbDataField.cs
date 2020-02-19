using System;
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

        public string AppearanceExpression { get; set; }

        public EbScript AppearExpression { get; set; }
    }

    public class EbDataFieldText : EbDataField
    {

    }

    public class EbDataFieldDateTime : EbDataField
    {
        public DateFormatReport Format { get; set; } = DateFormatReport.from_culture;
    }

    public class EbDataFieldBoolean : EbDataField
    {

    }

    public class EbDataFieldNumeric : EbDataField
    {
        public bool AmountInWords { get; set; }

        public bool SuppressIfZero { get; set; }

        public int DecimalPlaces { get; set; } = 2;
    }

    public interface IEbDataFieldSummary
    {
        object SummarizedValue { get; }
        void Summarize(object value);
    }

    public class EbDataFieldNumericSummary : EbDataFieldNumeric, IEbDataFieldSummary
    {
        public SummaryFunctionsNumeric Function { get; set; }

        public bool ResetOnNewPage { get; set; }

        public object SummarizedValue => throw new NotImplementedException();

        public void Summarize(object value)
        {
            throw new NotImplementedException();
        }
    }

    public class EbDataFieldTextSummary : EbDataFieldText, IEbDataFieldSummary
    {
        public SummaryFunctionsText Function { get; set; }

        public bool ResetOnNewPage { get; set; }

        public object SummarizedValue => throw new NotImplementedException();

        public void Summarize(object value)
        {
            throw new NotImplementedException();
        }
    }

    public class EbDataFieldDateTimeSummary : EbDataFieldDateTime, IEbDataFieldSummary
    {
        public SummaryFunctionsDateTime Function { get; set; }

        public bool ResetOnNewPage { get; set; }

        public object SummarizedValue => throw new NotImplementedException();

        public void Summarize(object value)
        {
            throw new NotImplementedException();
        }
    }

    public class EbDataFieldBooleanSummary : EbDataFieldBoolean, IEbDataFieldSummary
    {
        public SummaryFunctionsBoolean Function { get; set; }

        public bool ResetOnNewPage { get; set; }

        public object SummarizedValue => throw new NotImplementedException();

        public void Summarize(object value)
        {
            throw new NotImplementedException();
        }
    }
    public class EbCalcField : EbDataField
    {
        public string ValueExpression { get; set; }

        public EbScript ValExpression { get; set; }

        public string CalcFieldType { get; set; }

        public int CalcFieldIntType { get; set; }

        public int DecimalPlaces { get; set; } = 2;

        public bool AmountInWords { get; set; }
    }

    public class EbCalcFieldNumericSummary : EbCalcField, IEbDataFieldSummary
    {
        public SummaryFunctionsNumeric Function { get; set; }

        public bool ResetOnNewPage { get; set; }

        public object SummarizedValue => throw new NotImplementedException();

        public void Summarize(object value)
        {
            throw new NotImplementedException();
        }
    }

    public class EbCalcFieldTextSummary : EbCalcField, IEbDataFieldSummary
    {
        public SummaryFunctionsText Function { get; set; }

        public bool ResetOnNewPage { get; set; }

        public object SummarizedValue => throw new NotImplementedException();

        public void Summarize(object value)
        {
            throw new NotImplementedException();
        }
    }

    public class EbCalcFieldDateTimeSummary : EbCalcField, IEbDataFieldSummary
    {
        public SummaryFunctionsDateTime Function { get; set; }

        public bool ResetOnNewPage { get; set; }

        public object SummarizedValue => throw new NotImplementedException();

        public void Summarize(object value)
        {
            throw new NotImplementedException();
        }
    }

    public class EbCalcFieldBooleanSummary : EbCalcField, IEbDataFieldSummary
    {
        public SummaryFunctionsBoolean Function { get; set; }

        public bool ResetOnNewPage { get; set; }

        public object SummarizedValue => throw new NotImplementedException();

        public void Summarize(object value)
        {
            throw new NotImplementedException();
        }
    }
}