using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Helpers.Script;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ExpressBase.Mobile.Helpers
{
    public class EbFormHelper
    {
        private static EbFormHelper instance;

        public static EbFormHelper Instance => instance ??= new EbFormHelper();

        private static readonly Regex scriptRegex = new Regex(@"(?:form)\S+\w+");

        private Dictionary<string, EbMobileControl> controls;

        private Dictionary<string, List<string>> dependencyMap;

        private readonly EbSciptEvaluator evaluator;

        private EbFormHelper()
        {
            evaluator = new EbSciptEvaluator();
        }

        public static void Initialize(Dictionary<string, EbMobileControl> controls)
        {
            Instance.controls = controls;

            Instance.InitializeDependency();
        }

        public void InitializeDependency()
        {
            dependencyMap = new Dictionary<string, List<string>>();

            foreach (var ctrl in controls.Values)
            {
                if (ctrl.ValueExpr == null || ctrl.ValueExpr.IsEmpty())
                    continue;

                try
                {
                    foreach (Match match in scriptRegex.Matches(ctrl.ValueExpr.GetCode()))
                    {
                        string matchUnit = match.Value;

                        EbScriptExpression ebExpr = this.GetExpression(matchUnit);

                        if (!dependencyMap.ContainsKey(ebExpr.Name))
                        {
                            dependencyMap.Add(ebExpr.Name, new List<string>());
                        }

                        if (!dependencyMap[ebExpr.Name].Contains(ctrl.Name))
                        {
                            dependencyMap[ebExpr.Name].Add(ctrl.Name);
                        }
                    }
                }
                catch (Exception ex)
                {
                    EbLog.Info("Failed to resolve control dependency :" + ctrl.Name);
                    EbLog.Info(ex.Message);
                }
            }
        }

        public static void ControlValueChanged(string controlName)
        {
            if (!Instance.dependencyMap.ContainsKey(controlName))
                return;

            List<string> dep = Instance.dependencyMap[controlName] ?? new List<string>();

            foreach (var name in dep)
            {
                EbMobileControl ctrl = Instance.controls[name];

                Instance.EvaluateValueExpr(ctrl);
            }
        }

        public void EvaluateValueExpr(EbMobileControl ctrl)
        {
            try
            {
                string expr = ctrl.ValueExpr.GetCode();

                MatchCollection collection = scriptRegex.Matches(expr);

                if (collection == null) 
                    return;

                if (this.GetComputedExpr(collection, expr, out string computed))
                {
                    this.ComputeAndSetValue(computed, ctrl);
                }
            }
            catch (Exception ex)
            {
                EbLog.Info("Failed to evaluate expression for :" + ctrl.Name);
                EbLog.Error(ex.Message);
            }
        }

        private EbScriptExpression GetExpression(string expr)
        {
            string[] parts = expr.Split(CharConstants.DOT);

            if (parts.Length < 3)
                return null;

            return new EbScriptExpression
            {
                Name = parts[1],
                Method = parts[2]
            };
        }

        private bool GetComputedExpr(MatchCollection matches, string expr, out string computedResult)
        {
            computedResult = string.Empty;

            try
            {
                foreach (Match match in matches)
                {
                    string matchUnit = match.Value;

                    EbScriptExpression ebExpr = this.GetExpression(matchUnit);

                    if (ebExpr == null)
                        throw new Exception("Eb script expression format error" + matchUnit);

                    object value = this.controls[ebExpr.Name].InvokeDynamically(ebExpr.Method);

                    if (value == null)
                        throw new Exception("expression member value is null, operation terminated");

                    expr = expr.Replace(matchUnit + "()", value.ToString());
                }

                computedResult = expr;

                return true;
            }
            catch (Exception ex)
            {
                EbLog.Info("Error in computation");
                EbLog.Error(ex.Message);
            }
            return false;
        }

        private void ComputeAndSetValue(string expression, EbMobileControl ctrl)
        {
            //int numLines = expression.Split(CharConstants.NEWLINE).Length;

            //if (numLines == 1 && expression.Contains(returnKeyWord))
            //{
            //    expression = expression.RemoveSubstring(returnKeyWord).TrimEnd(CharConstants.SEMICOLON);
            //}

            try
            {
                object value = evaluator.ScriptEvaluate(expression);
                ctrl.SetValue(value);
                ctrl.ValueChanged();
            }
            catch (Exception ex)
            {
                EbLog.Info("Failed to calaculate expression");
                EbLog.Error(ex.Message);
            }
        }
    }
}
