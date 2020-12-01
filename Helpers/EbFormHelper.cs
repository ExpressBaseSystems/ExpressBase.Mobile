using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers.Script;
using System;
using System.Collections.Generic;
using System.Linq;
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
            evaluator = new EbSciptEvaluator
            {
                OptionScriptNeedSemicolonAtTheEndOfLastExpression = false
            };
        }

        private void Dispose()
        {
            controls = null;
            dependencyMap = null;
            evaluator.RemoveVariable("form");
        }

        public static void Initialize(EbMobileForm form, FormMode mode)
        {
            Instance.Dispose();

            Instance.controls = form.ControlDictionary;
            Instance.evaluator.SetVariable("form", new EbFormEvaluator(mode));

            Instance.dependencyMap = new ControlDependencyMap();
            Instance.dependencyMap.Init(form.ControlDictionary);
        }

        public static void ControlValueChanged(string controlName)
        {
            if (Instance.dependencyMap.HasDependency(controlName))
            {
                ExprDependency exprDep = Instance.dependencyMap.GetDependency(controlName);

                Instance.InitValueExpr(exprDep, controlName);
                Instance.InitHideExpr(exprDep);
                Instance.InitDisableExpr(exprDep);
            }
            Instance.InitValidators(controlName);
        }

        public static void EvaluateExprOnLoad(EbMobileControl ctrl, FormMode mode)
        {
            if (Instance.dependencyMap.HasDependency(ctrl.Name))
            {
                ExprDependency exprDep = Instance.dependencyMap.GetDependency(ctrl.Name);

                if (mode == FormMode.NEW || mode == FormMode.EDIT || mode == FormMode.PREFILL)
                {
                    if (mode == FormMode.NEW || mode == FormMode.PREFILL)
                    {
                        Instance.InitHideExpr(exprDep, ctrl);
                        Instance.InitDisableExpr(exprDep, ctrl);
                    }
                    else if (mode == FormMode.EDIT)
                    {
                        if (ctrl.DoNotPersist)
                            Instance.InitValueExpr(exprDep, ctrl.Name);

                        Instance.InitHideExpr(exprDep, ctrl);
                    }
                }
            }
        }

        public static void SetDefaultValue(string controlName)
        {
            Instance.SetDefaultValueInternal(controlName);
        }

        public static bool ContainsInValExpr(string dependencySource, string dependency)
        {
            if (!Instance.dependencyMap.HasDependency(dependencySource))
                return false;

            ExprDependency map = Instance.dependencyMap.GetDependency(dependencySource);

            return map.ValueExpr.Contains(dependency);
        }

        public static void SwitchViewToEdit()
        {
            foreach (EbMobileControl ctrl in Instance.controls.Values)
            {
                if (!ctrl.ReadOnly) ctrl.SetAsReadOnly(false);

                if (Instance.dependencyMap.HasDependency(ctrl.Name))
                {
                    ExprDependency exprDep = Instance.dependencyMap.GetDependency(ctrl.Name);

                    if (ctrl.DoNotPersist)
                        Instance.InitValueExpr(exprDep, ctrl.Name);

                    Instance.InitDisableExpr(exprDep);
                }
            }
        }

        public static bool Validate()
        {
            foreach (EbMobileControl ctrl in Instance.controls.Values)
            {
                bool valid = Instance.InitValidators(ctrl.Name);

                if (!ctrl.Validate() || !valid)
                    return false;
            }
            return true;
        }

        private void InitValueExpr(ExprDependency exprDep, string trigger_control)
        {
            if (exprDep.HasValueDependency)
            {
                foreach (string name in exprDep.ValueExpr)
                {
                    EbMobileControl ctrl = this.controls[name];
                    this.EvaluateValueExpr(ctrl, trigger_control);
                }
            }
        }

        private void InitHideExpr(ExprDependency exprDep, EbMobileControl currentControl = null)
        {
            if (exprDep.HasHideDependency)
            {
                foreach (var name in exprDep.HideExpr)
                {
                    EbMobileControl ctrl = this.controls[name];
                    this.EvaluateHideExpr(ctrl);
                }
            }
            else if (currentControl != null)
            {
                this.EvaluateHideExpr(currentControl);
            }
        }

        private void InitDisableExpr(ExprDependency exprDep, EbMobileControl currentControl = null)
        {
            if (exprDep.HasDisableDependency)
            {
                foreach (var name in exprDep.DisableExpr)
                {
                    EbMobileControl ctrl = this.controls[name];
                    this.EvaluateDisableExpr(ctrl);
                }
            }
            else if (currentControl != null)
            {
                this.EvaluateDisableExpr(currentControl);
            }
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
                }
            }

            EbMobileControl ctrl = this.controls[controlName];

            if (ctrl.DefaultValueExpression != null && !ctrl.DefaultValueExpression.IsEmpty())
            {
                this.EvaluateDefaultValueExpr(ctrl);
            }
        }

        private bool InitValidators(string controlName)
        {
            EbMobileControl ctrl = controls[controlName];

            if (ctrl.Validators == null || !ctrl.Validators.Any() || ctrl is INonPersistControl)
                return true;

            bool flag = true;

            foreach (EbMobileValidator validator in ctrl.Validators)
            {
                if (validator.IsDisabled || validator.IsEmpty())
                    continue;

                flag = this.EvaluateValidatorExpr(validator, controlName);
                ctrl.SetValidation(flag, validator.FailureMSG);
                if (!flag) break;
            }
            return flag;
        }

        private void EvaluateValueExpr(EbMobileControl ctrl, string trigger_control)
        {
            string expr = ctrl.ValueExpr.GetCode();

            expr = this.FormatThisReference(expr, ctrl.Name);

            if (this.GetComputedExpr(expr, out string computed))
            {
                try
                {
                    object value = evaluator.Execute(computed);
                    ctrl.SetValue(value);
                    ctrl.ValueChanged(trigger_control);
                }
                catch (Exception ex)
                {
                    EbLog.Info($"Value script evaluation error in control '{ctrl.Name}'");
                    EbLog.Error(ex.Message);
                }
            }
        }

        private void EvaluateHideExpr(EbMobileControl ctrl)
        {
            string expr = ctrl.HiddenExpr.GetCode();

            expr = this.FormatThisReference(expr, ctrl.Name);

            if (this.GetComputedExpr(expr, out string computed))
            {
                try
                {
                    bool value = evaluator.Execute<bool>(computed);
                    ctrl.SetVisibilty(!value);
                }
                catch (Exception ex)
                {
                    EbLog.Info($"hide script evaluation error in control '{ctrl.Name}', considering as false");
                    EbLog.Error(ex.Message);

                    ctrl.SetVisibilty(true);
                }
            }
        }

        private void EvaluateDisableExpr(EbMobileControl ctrl)
        {
            string expr = ctrl.DisableExpr.GetCode();

            expr = this.FormatThisReference(expr, ctrl.Name);

            if (this.GetComputedExpr(expr, out string computed))
            {
                try
                {
                    bool value = evaluator.Execute<bool>(computed);
                    ctrl.SetAsReadOnly(value);
                }
                catch (Exception ex)
                {
                    EbLog.Info($"Disable script evaluation error in control '{ctrl.Name}', considering as false");
                    EbLog.Error(ex.Message);

                    ctrl.SetAsReadOnly(false);
                }
            }
        }

        private void EvaluateDefaultValueExpr(EbMobileControl ctrl)
        {
            string expr = ctrl.DefaultValueExpression.GetCode();

            if (this.GetComputedExpr(expr, out string computed))
            {
                try
                {
                    object value = evaluator.Execute(computed);
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

        private bool EvaluateValidatorExpr(EbMobileValidator validator, string controlName)
        {
            string expr = validator.Script.GetCode();

            expr = this.FormatThisReference(expr, controlName);

            if (this.GetComputedExpr(expr, out string computed))
            {
                try
                {
                    return evaluator.Execute<bool>(computed);
                }
                catch (Exception ex)
                {
                    EbLog.Info($"validator script evaluation error in control '{controlName}'");
                    EbLog.Error(ex.Message);
                }
            }
            return false;
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

                    if (ebExpr != null)
                    {
                        object value = this.controls[ebExpr.Name].InvokeDynamically(ebExpr.Method);

                        expr = expr.Replace(matchUnit + "()", Convert(value));
                    }
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
            else if (IsNumeric(value))
                return value.ToString();
            else
                return $"\"{value}\"";
        }

        private bool IsNumeric(object value)
        {
            if (value is int || value is double || value is float || value is decimal)
                return true;
            return false;
        }

        private string FormatThisReference(string script, string controlName)
        {
            if (script.Contains("this"))
                return script.Replace("this", $"form.{controlName}");

            return script;
        }
    }
}
