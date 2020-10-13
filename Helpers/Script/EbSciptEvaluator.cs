using CodingSeb.ExpressionEvaluator;

namespace ExpressBase.Mobile.Helpers.Script
{
    public class EbSciptEvaluator : ExpressionEvaluator
    {
        public object Execute(string script)
        {
            this.ClearVariables();

            return this.ScriptEvaluate(script);
        }

        public T Execute<T>(string script)
        {
            this.ClearVariables();

            return this.ScriptEvaluate<T>(script);
        }

        private void ClearVariables()
        {
            if (this.Variables.Count > 0) 
                this.Variables.Clear();
        }
    }

    public class EbScriptExpression
    {
        public string Name { set; get; }

        public string Method { set; get; }
    }
}
