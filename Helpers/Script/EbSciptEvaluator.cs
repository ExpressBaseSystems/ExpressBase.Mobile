using CodingSeb.ExpressionEvaluator;
using ExpressBase.Mobile.Enums;
using System.Collections.Generic;

namespace ExpressBase.Mobile.Helpers.Script
{
    public class EbSciptEvaluator : ExpressionEvaluator
    {
        private readonly List<string> persistanceKeys = new List<string>
        {
            "form"
        };

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
            if (Variables != null && Variables.Count > 0)
            {
                foreach (string key in Variables.Keys)
                {
                    if (!persistanceKeys.Contains(key))
                    {
                        Variables.Remove(key);
                    }
                }
            }
        }

        public void SetVariable(string key, object value)
        {
            Variables.Add(key, value);
        }

        public void RemoveVariable(string key)
        {
            if (Variables.ContainsKey(key))
                Variables.Remove(key);
        }
    }

    public class EbScriptExpression
    {
        public string Name { set; get; }

        public string Method { set; get; }
    }

    public class EbFormEvaluator
    {
        public string __mode { set; get; }

        public EbFormEvaluator() { }

        public EbFormEvaluator(FormMode mode)
        {
            SetMode(mode);
        }

        public void SetMode(FormMode mode)
        {
            if (mode == FormMode.NEW || mode == FormMode.PREFILL)
                __mode = "new";
            else if (mode == FormMode.EDIT)
                __mode = "view";
            else
                __mode = "ref";
        }
    }
}
