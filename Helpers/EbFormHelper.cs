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

        private static readonly Regex scriptRegex = new Regex(@"(?:Form)\S+\w+");

        private Dictionary<string, EbMobileControl> Controls { set; get; }

        private Dictionary<string, List<string>> DependencyMap { set; get; }

        private readonly EbSciptEvaluator evaluator;

        private EbFormHelper()
        {
            evaluator = new EbSciptEvaluator();
        }

        public static void Initialize(Dictionary<string, EbMobileControl> controls)
        {
            Instance.Controls = controls;
            Instance.InitializeDependency();
        }

        public void InitializeDependency()
        {
            DependencyMap = new Dictionary<string, List<string>>();

            foreach (var ctrl in Controls.Values)
            {
                if (ctrl.ValueExpr == null || ctrl.ValueExpr.IsEmpty())
                    continue;

                try
                {
                    foreach (Match match in scriptRegex.Matches(ctrl.ValueExpr.GetCode()))
                    {
                        string[] parts = match.Value.Split(CharConstants.DOT);

                        string name = parts[1];

                        if (!DependencyMap.ContainsKey(name))
                            DependencyMap.Add(name, new List<string>());

                        if (!DependencyMap[name].Contains(ctrl.Name))
                            DependencyMap[name].Add(ctrl.Name);
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
            if (!Instance.DependencyMap.ContainsKey(controlName))
                return;

            List<string> dep = Instance.DependencyMap[controlName] ?? new List<string>();

            foreach (var name in dep)
            {
                EbMobileControl ctrl = Instance.Controls[name];

                Instance.EvaluateValueExpr(ctrl);
            }
        }

        public void EvaluateValueExpr(EbMobileControl ctrl)
        {
            try
            {
                string expr = ctrl.ValueExpr.GetCode();

                MatchCollection collection = scriptRegex.Matches(expr);

                if (collection == null) return;

                if (this.GetComputedExpr(collection, expr, out string computed))
                {
                    this.Evaluate(computed, ctrl);
                }
            }
            catch (Exception ex)
            {
                EbLog.Info("Failed to evaluate expression for :" + ctrl.Name);
                EbLog.Info(ex.Message);
            }
        }

        private bool GetComputedExpr(MatchCollection matches, string expr, out string computedResult)
        {
            computedResult = string.Empty;
            try
            {
                foreach (Match match in matches)
                {
                    string[] parts = match.Value.Split(CharConstants.DOT);

                    string controlName = parts[1];

                    string methodName = parts[2];

                    object value = this.Controls[controlName].InvokeDynamically(methodName);

                    if (value == null) throw new Exception("expression member value is null, operation terminated");

                    expr = expr.Replace(match.Value + "()", value.ToString());
                }

                expr = expr.Replace("return", string.Empty).Trim().Trim(CharConstants.SEMICOLON);

                computedResult = expr;

                return true;
            }
            catch (Exception ex)
            {
                EbLog.Info("explicit exception");
                EbLog.Info(ex.Message);
            }
            return false;
        }

        private void Evaluate(string expression, EbMobileControl ctrl)
        {
            try
            {
                object value = evaluator.Evaluate(expression);

                ctrl.SetValue(value);
                ctrl.ValueChanged();
            }
            catch (Exception ex)
            {
                EbLog.Info("Failed to calaculate expression");
                EbLog.Info(ex.Message);
            }
        }
    }
}
