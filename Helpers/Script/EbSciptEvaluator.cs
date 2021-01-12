using CodingSeb.ExpressionEvaluator;
using ExpressBase.Mobile.CustomControls.XControls;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;

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
            Variables.ToList().FindAll(kvp => kvp.Value is StronglyTypedVariable).ForEach(kvp => Variables.Remove(kvp.Key));
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

    public class EbListEvaluator
    {
        private EbDataRow currentRow;

        private View currentView;

        public void SetCurrentRow(EbDataRow row)
        {
            currentRow = row;
        }

        public void SetCurrentView(View view)
        {
            currentView = view;
        }

        public object getValue(string columnName)
        {
            if (currentRow == null)
                return null;
            return currentRow[columnName];
        }

        public void setValue(object value)
        {
            if (value == null) return;

            if (currentView != null && currentView is EbXLabel xLabel)
            {
                xLabel.Text = value.ToString();
            }
        }

        public void setBackground(string color)
        {
            try
            {
                if (currentView == null || color == null)
                    return;

                Color xcolor = Color.FromHex(color);

                if (currentView is EbXLabel xLabel)
                    xLabel.XBackgroundColor = xcolor;
                else
                    currentView.BackgroundColor = xcolor;
            }
            catch (Exception ex)
            {
                EbLog.Info("error at [setBackground] in list evaluator, " + ex.Message);
            }
        }

        public void setColor(string color)
        {
            try
            {
                if (currentView == null || color == null)
                    return;

                PropertyInfo propInfo = currentView.GetType().GetProperty("TextColor", BindingFlags.Instance | BindingFlags.Public);

                if (propInfo != null)
                {
                    propInfo.SetValue(currentView, Color.FromHex(color));
                }
            }
            catch (Exception ex)
            {
                EbLog.Info("error at [setColor] in list evaluator, " + ex.Message);
            }
        }
    }
}
