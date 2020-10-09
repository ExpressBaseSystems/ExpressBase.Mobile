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

        private Dictionary<string, EbMobileControl> controls;

        private ControlDependencyMap dependencyMap;

        private readonly EbSciptEvaluator evaluator;

        private EbFormHelper()
        {
            evaluator = new EbSciptEvaluator();
        }

        private void Dispose()
        {
            controls = null;
            dependencyMap = null;
        }

        public static void Initialize(Dictionary<string, EbMobileControl> controls)
        {
            Instance.Dispose();

            Instance.controls = controls;

            Instance.dependencyMap = new ControlDependencyMap();
            Instance.dependencyMap.Init(controls);
        }

        public static void ControlValueChanged(string controlName)
        {
            if (!Instance.dependencyMap.HasDependency(controlName))
                return;

            ExprDependency exprDep = Instance.dependencyMap.GetDependency(controlName);

            if (exprDep.HasValueDependency)
            {
                foreach (var name in exprDep.ValueExpr)
                {
                    EbMobileControl ctrl = Instance.controls[name];
                    Instance.EvaluateValueExpr(ctrl);
                }
            }

            if (exprDep.HasHideDependency)
            {
                foreach (var name in exprDep.HideExpr)
                {
                    EbMobileControl ctrl = Instance.controls[name];
                    Instance.EvaluateHideExpr(ctrl);
                }
            }

            if (exprDep.HasDisableDependency)
            {
                foreach (var name in exprDep.DisableExpr)
                {
                    EbMobileControl ctrl = Instance.controls[name];
                    Instance.EvaluateDisableExpr(ctrl);
                }
            }
        }

        public static void SetDefaultValue(string controlName)
        {
            Instance.SetDefaultValueInternal(controlName);
        }

        private void SetDefaultValueInternal(string controlName)
        {
            if (this.dependencyMap.HasDependency(controlName))
            {
                ExprDependency exprDep = this.dependencyMap.GetDependency(controlName);

                if (exprDep.HasDefaultDependency)
                {
                    foreach (string name in exprDep.DefaultValueExpr)
                        this.SetDefaultValueInternal(name);

                    EbMobileControl ctrl = this.controls[controlName];
                    this.EvaluateDefaultValueExpr(ctrl);
                }
            }
            else
            {
                EbMobileControl ctrl = this.controls[controlName];
                this.EvaluateDefaultValueExpr(ctrl);
            }
        }

        public void EvaluateValueExpr(EbMobileControl ctrl)
        {
            string expr = ctrl.ValueExpr.GetCode();

            if (this.GetComputedExpr(expr, out string computed))
            {
                try
                {
                    object value = evaluator.ScriptEvaluate(computed);
                    ctrl.SetValue(value);
                    ctrl.ValueChanged();
                }
                catch (Exception ex)
                {
                    EbLog.Info($"Value script evaluation error in control '{ctrl.Name}'");
                    EbLog.Error(ex.Message);
                }
            }
        }

        public void EvaluateHideExpr(EbMobileControl ctrl)
        {
            string expr = ctrl.HiddenExpr.GetCode();

            if (this.GetComputedExpr(expr, out string computed))
            {
                try
                {
                    bool value = evaluator.ScriptEvaluate<bool>(computed);
                    if (value)
                        ctrl.SetVisibilty(false);
                    else
                        ctrl.SetVisibilty(true);
                }
                catch (Exception ex)
                {
                    EbLog.Info($"hide script evaluation error in control '{ctrl.Name}', considering as false");
                    EbLog.Error(ex.Message);

                    ctrl.SetVisibilty(true);
                }
            }
        }

        public void EvaluateDisableExpr(EbMobileControl ctrl)
        {
            string expr = ctrl.DisableExpr.GetCode();

            if (this.GetComputedExpr(expr, out string computed))
            {
                try
                {
                    bool value = evaluator.ScriptEvaluate<bool>(computed);
                    if (value)
                        ctrl.SetAsReadOnly(true);
                    else
                        ctrl.SetAsReadOnly(false);
                }
                catch (Exception ex)
                {
                    EbLog.Info($"Disable script evaluation error in control '{ctrl.Name}', considering as false");
                    EbLog.Error(ex.Message);

                    ctrl.SetAsReadOnly(false);
                }
            }
        }

        public void EvaluateDefaultValueExpr(EbMobileControl ctrl)
        {
            string expr = ctrl.DefaultValueExpression.GetCode();

            if (this.GetComputedExpr(expr, out string computed))
            {
                try
                {
                    object value = evaluator.ScriptEvaluate(computed);
                    ctrl.SetValue(value);
                    ctrl.DefaultExprEvaluated = true;
                }
                catch (Exception ex)
                {
                    EbLog.Info($"Default script evaluation error in control '{ctrl.Name}'");
                    EbLog.Error(ex.Message);
                }
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

        private bool GetComputedExpr(string expr, out string computedResult)
        {
            computedResult = string.Empty;

            try
            {
                MatchCollection collection = ControlDependencyMap.EbScriptRegex.Matches(expr);

                if (collection == null || collection.Count <= 0)
                {
                    computedResult = expr;
                    return true;
                }  

                foreach (Match match in collection)
                {
                    string matchUnit = match.Value;

                    EbScriptExpression ebExpr = this.GetExpression(matchUnit);

                    if (ebExpr == null)
                        throw new Exception("Eb script expression format error" + matchUnit);

                    object value = this.controls[ebExpr.Name].InvokeDynamically(ebExpr.Method);

                    expr = expr.Replace(matchUnit + "()", Convert(value));
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

        private string Convert(object value)
        {
            if (value == null)
                return "null";
            else if (value is bool)
                return value.ToString().ToLower();
            else
                return $"\"{value}\"";
        }
    }
}
