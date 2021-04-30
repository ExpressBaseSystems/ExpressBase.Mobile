using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers.Script;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpressBase.Mobile.Helpers
{
    public class EbFormHelper
    {
        private static EbFormHelper instance;

        public static EbFormHelper Instance => instance ??= new EbFormHelper();

        private Dictionary<string, EbMobileControl> controls;

        private ControlDependencyMap dependencyMap;

        private readonly EbSciptEvaluator evaluator;

        public const string CTRL_PARENT_FORM = "form";

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
            Instance.evaluator.SetVariable("form", new EbFormEvaluator(mode, form.ControlDictionary));

            Instance.dependencyMap = new ControlDependencyMap();
            Instance.dependencyMap.Init(form.ControlDictionary);
        }

        public static void ControlValueChanged(string controlName, string parent = CTRL_PARENT_FORM)
        {
            if (Instance.dependencyMap.HasDependency(controlName, parent))
            {
                ExprDependency exprDep = Instance.dependencyMap.GetDependency(controlName, parent);

                Instance.InitValueExpr(exprDep, controlName, parent);
                Instance.InitHideExpr(exprDep, parent: parent);
                Instance.InitDisableExpr(exprDep, parent: parent);
            }
            Instance.InitValidators(controlName, parent);
        }

        public static void EvaluateExprOnLoad(EbMobileControl ctrl, FormMode mode)
        {
            string parent = ctrl.Parent;

            if (Instance.dependencyMap.HasDependency(ctrl.Name, parent))
            {
                ExprDependency exprDep = Instance.dependencyMap.GetDependency(ctrl.Name, parent);

                if (mode == FormMode.NEW || mode == FormMode.EDIT || mode == FormMode.PREFILL)
                {
                    if (mode == FormMode.NEW || mode == FormMode.PREFILL)
                    {
                        Instance.InitHideExpr(exprDep, ctrl, parent);
                        Instance.InitDisableExpr(exprDep, ctrl, parent);
                    }
                    else if (mode == FormMode.EDIT)
                    {
                        if (ctrl.DoNotPersist)
                            Instance.InitValueExpr(exprDep, ctrl.Name, parent);

                        Instance.InitHideExpr(exprDep, ctrl, parent);
                    }
                }
            }
            else
            {
                if (mode == FormMode.NEW || mode == FormMode.EDIT || mode == FormMode.PREFILL)
                {
                    if (ctrl.HasExpression(ExprType.HideExpr))
                    {
                        Instance.EvaluateHideExpr(ctrl, parent);
                    }
                    if ((mode == FormMode.NEW || mode == FormMode.PREFILL) && ctrl.HasExpression(ExprType.DisableExpr))
                    {
                        Instance.EvaluateDisableExpr(ctrl, parent);
                    }
                }
            }
        }

        public static void SetDefaultValue(string controlName, string parent = CTRL_PARENT_FORM)
        {
            Instance.SetDefaultValueInternal(controlName, parent);
        }

        public static bool ContainsInValExpr(string dependencySource, string dependency, string parent = CTRL_PARENT_FORM)
        {
            if (!Instance.dependencyMap.HasDependency(dependencySource, parent))
                return false;

            ExprDependency map = Instance.dependencyMap.GetDependency(dependencySource, parent);

            return map.ValueExpr.Contains(dependency);
        }

        public static void SwitchViewToEdit()
        {
            foreach (EbMobileControl ctrl in Instance.controls.Values)
            {
                if (!ctrl.ReadOnly) ctrl.SetAsReadOnly(false);

                if (Instance.dependencyMap.HasDependency(ctrl.Name, ctrl.Parent))
                {
                    ExprDependency exprDep = Instance.dependencyMap.GetDependency(ctrl.Name, ctrl.Parent);

                    if (ctrl.DoNotPersist)
                        Instance.InitValueExpr(exprDep, ctrl.Name, ctrl.Parent);

                    Instance.InitDisableExpr(exprDep, parent: ctrl.Parent);
                }
            }
        }

        public static bool Validate()
        {
            foreach (EbMobileControl ctrl in Instance.controls.Values)
            {
                bool valid = Instance.InitValidators(ctrl.Name, ctrl.Parent);

                if (!ctrl.Validate() || !valid)
                    return false;
            }
            return true;
        }

        private void InitValueExpr(ExprDependency exprDep, string trigger_control, string parent = CTRL_PARENT_FORM)
        {
            if (exprDep.HasValueDependency)
            {
                foreach (string name in exprDep.ValueExpr)
                {
                    EbMobileControl ctrl = GetControl(name, parent);
                    EvaluateValueExpr(ctrl, trigger_control, parent);
                }
            }
        }

        private void InitHideExpr(ExprDependency exprDep, EbMobileControl currentControl = null, string parent = CTRL_PARENT_FORM)
        {
            if (exprDep.HasHideDependency)
            {
                foreach (var name in exprDep.HideExpr)
                {
                    EbMobileControl ctrl = GetControl(name, parent);
                    this.EvaluateHideExpr(ctrl, parent);
                }
            }
            else if (currentControl != null)
            {
                this.EvaluateHideExpr(currentControl, parent);
            }
        }

        private void InitDisableExpr(ExprDependency exprDep, EbMobileControl currentControl = null, string parent = CTRL_PARENT_FORM)
        {
            if (exprDep.HasDisableDependency)
            {
                foreach (var name in exprDep.DisableExpr)
                {
                    EbMobileControl ctrl = GetControl(name, parent);
                    this.EvaluateDisableExpr(ctrl, parent);
                }
            }
            else if (currentControl != null)
            {
                this.EvaluateDisableExpr(currentControl, parent);
            }
        }

        private void SetDefaultValueInternal(string controlName, string parent = CTRL_PARENT_FORM)
        {
            if (this.dependencyMap.HasDependency(controlName, parent))
            {
                ExprDependency exprDep = this.dependencyMap.GetDependency(controlName, parent);

                if (exprDep.HasDefaultDependency)
                {
                    foreach (string name in exprDep.DefaultValueExpr)
                        this.SetDefaultValueInternal(name, parent);
                }
            }

            EbMobileControl ctrl = GetControl(controlName, parent);

            if (ctrl.DefaultValueExpression != null && !ctrl.DefaultValueExpression.IsEmpty())
            {
                this.EvaluateDefaultValueExpr(ctrl, parent);
            }
        }

        private bool InitValidators(string controlName, string parent = CTRL_PARENT_FORM)
        {
            EbMobileControl ctrl = GetControl(controlName, parent);

            if (ctrl.Validators == null || !ctrl.Validators.Any() || ctrl is INonPersistControl)
                return true;

            bool flag = true;

            foreach (EbMobileValidator validator in ctrl.Validators)
            {
                if (validator.IsDisabled || validator.IsEmpty())
                    continue;

                flag = EvaluateValidatorExpr(validator, controlName, parent);
                ctrl.SetValidation(flag, validator.FailureMSG);
                if (!flag) break;
            }
            return flag;
        }

        private void EvaluateValueExpr(EbMobileControl ctrl, string trigger_control, string parent)
        {
            string expr = ctrl.ValueExpr.GetCode();

            if (this.GetComputedExpr(ctrl.Name, expr, parent, out string computed))
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

        private void EvaluateHideExpr(EbMobileControl ctrl, string parent)
        {
            string expr = ctrl.HiddenExpr.GetCode();

            if (this.GetComputedExpr(ctrl.Name, expr, parent, out string computed))
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

        private void EvaluateDisableExpr(EbMobileControl ctrl, string parent)
        {
            string expr = ctrl.DisableExpr.GetCode();

            if (this.GetComputedExpr(ctrl.Name, expr, parent, out string computed))
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

        private void EvaluateDefaultValueExpr(EbMobileControl ctrl, string parent)
        {
            string expr = ctrl.DefaultValueExpression.GetCode();

            if (this.GetComputedExpr(ctrl.Name, expr, parent, out string computed))
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

        private bool EvaluateValidatorExpr(EbMobileValidator validator, string controlName, string parent)
        {
            string expr = validator.Script.GetCode();

            if (this.GetComputedExpr(controlName, expr, parent, out string computed))
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

        private bool GetComputedExpr(string controlName, string expr, string parent, out string computedResult)
        {
            computedResult = string.Empty;

            try
            {
                List<string> dependentNames = dependencyMap.GetNames(parent);

                if (expr.Contains("this"))
                {
                    expr = expr.Replace("this", $"{CTRL_PARENT_FORM}.{controlName}");
                }

                if (expr.Contains($"{CTRL_PARENT_FORM}.{controlName}"))
                {
                    expr = expr.Replace($"{CTRL_PARENT_FORM}.{controlName}", $"{CTRL_PARENT_FORM}.Controls[\"{controlName}\"]");
                }

                if (parent != CTRL_PARENT_FORM && expr.Contains($"{CTRL_PARENT_FORM}.{parent}"))
                {
                    expr = expr.Replace($"{CTRL_PARENT_FORM}.{parent}", $"{CTRL_PARENT_FORM}.Controls[\"{parent}\"]");
                }

                if (dependentNames != null)
                {
                    foreach (string name in dependentNames)
                    {
                        if (expr.Contains($"{CTRL_PARENT_FORM}.{name}"))
                        {
                            expr = expr.Replace($"{CTRL_PARENT_FORM}.{name}", $"{CTRL_PARENT_FORM}.Controls[\"{name}\"]");
                        }
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

        private EbMobileControl GetControl(string controlName, string parent)
        {
            if (parent == CTRL_PARENT_FORM)
            {
                return this.controls[controlName];
            }
            else
            {
                if (this.controls.TryGetValue(parent, out var ctrl) && ctrl is ILinesEnabled lines)
                {
                    return lines.ChildControls.Find(c => c.Name == controlName);
                }
            }
            return null;
        }
    }
}
