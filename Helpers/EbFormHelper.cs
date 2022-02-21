using ExpressBase.Mobile.Constants;
using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Enums;
using ExpressBase.Mobile.Helpers.Script;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Helpers
{
    public class EbFormHelper
    {
        public static EbFormHelper Instance { set; get; }

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

        public static void Initialize(EbMobileForm form, FormMode mode)
        {
            Instance = new EbFormHelper
            {
                controls = form.ControlDictionary
            };
            Instance.evaluator.SetVariable("form", new EbFormEvaluator(mode, form.ControlDictionary));

            Instance.dependencyMap = new ControlDependencyMap();
            Instance.dependencyMap.Init(form.ControlDictionary);
        }

        public static void ControlValueChanged(string controlName, string parent = CTRL_PARENT_FORM)
        {
            if (Instance.dependencyMap.HasDependency(controlName, parent, out ExprDependency dependency))
            {
                Instance.InitValueExpr(dependency, controlName, parent);
                Instance.InitHideExpr(dependency, parent: parent);
                Instance.InitDisableExpr(dependency, parent: parent);
            }
            Instance.InitValidators(controlName, parent);
        }

        public static void EvaluateExprOnLoad(EbMobileControl ctrl, FormMode mode)
        {
            string parent = ctrl.Parent;

            if (Instance.dependencyMap.HasDependency($"{parent}.{ctrl.Name}"))
            {
                ExprDependency exprDep = Instance.dependencyMap.GetDependency($"{parent}.{ctrl.Name}");

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
                    if (ctrl.HasExpression(ExpressionType.HideExpression))
                    {
                        Instance.EvaluateHideExpr(ctrl, parent);
                    }
                    if ((mode == FormMode.NEW || mode == FormMode.PREFILL) && ctrl.HasExpression(ExpressionType.DisableExpression))
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
            if (!Instance.dependencyMap.HasDependency($"{parent}.{dependencySource}"))
                return false;

            ExprDependency map = Instance.dependencyMap.GetDependency($"{parent}.{dependencySource}");

            return map.ValueExpr.Contains(dependency);
        }

        public static void SwitchViewToEdit()
        {
            foreach (EbMobileControl ctrl in Instance.controls.Values)
            {
                if (!ctrl.ReadOnly) ctrl.SetAsReadOnly(false);

                if (Instance.dependencyMap.HasDependency($"{ctrl.Parent}.{ctrl.Name}"))
                {
                    ExprDependency exprDep = Instance.dependencyMap.GetDependency($"{ctrl.Parent}.{ctrl.Name}");

                    if (ctrl.DoNotPersist)
                        Instance.InitValueExpr(exprDep, ctrl.Name, ctrl.Parent);

                    Instance.InitDisableExpr(exprDep, parent: ctrl.Parent);
                }
            }
        }

        public static string Validate()
        {
            string msg = null;
            foreach (EbMobileControl ctrl in Instance.controls.Values)
            {
                msg = Validate_inner(ctrl);
                if (msg != null)
                    break;
            }
            return msg;
        }

        private static string Validate_inner(EbMobileControl ctrl)
        {
            string msg = null;
            if (!ctrl.Validate())
            {
                msg = string.IsNullOrEmpty(ctrl.Label) ? ctrl.Name : ctrl.Label;
                msg = string.IsNullOrEmpty(msg) ? "Fields required" : (msg + " is required");
                if (ctrl.Hidden)
                    msg += " (Hidden)";
            }

            bool valid = Instance.InitValidators(ctrl.Name, ctrl.Parent);

            if (!valid)
            {
                msg = ctrl.GetValidatorFailureMsg();
                msg = string.IsNullOrEmpty(msg) ? ("Validation failed: " + ctrl.Label ?? ctrl.Name) : msg;
            }
            return msg;
        }

        public static string ValidateDataGrid(EbMobileDataGrid dataGrid)
        {
            string msg = null;
            foreach (EbMobileControl ctrl in dataGrid.ChildControls)
            {
                msg = Validate_inner(ctrl);
                if (msg != null)
                    break;
            }
            return msg;
        }

        public List<Param> GetPsParams(List<Param> Params)
        {
            List<Param> parameters = new List<Param>();

            foreach (Param p in Params)
            {
                string value = this.GetControlByName(p.Name)?.getValue()?.ToString();

                parameters.Add(new Param
                {
                    Name = p.Name,
                    Type = p.Type,
                    Value = value
                });
            }
            return parameters;
        }

        public static void ExecDGOuterDependency(string dgname)
        {
            if (Instance.dependencyMap.DGDependencyMapColl.TryGetValue(dgname, out ExprDependency exprDep))
            {
                if (exprDep.HasValueDependency)
                {
                    foreach (string name in exprDep.ValueExpr)
                    {
                        EbMobileControl ctrl = Instance.GetControl(name);
                        Instance.EvaluateValueExpr(ctrl, name, CTRL_PARENT_FORM);
                    }
                }

                if (exprDep.HasHideDependency)
                {
                    foreach (string name in exprDep.HideExpr)
                    {
                        EbMobileControl ctrl = Instance.GetControl(name);
                        Instance.EvaluateHideExpr(ctrl, CTRL_PARENT_FORM);
                    }
                }

                if (exprDep.HasDisableDependency)
                {
                    foreach (string name in exprDep.DisableExpr)
                    {
                        EbMobileControl ctrl = Instance.GetControl(name);
                        Instance.EvaluateDisableExpr(ctrl, CTRL_PARENT_FORM);
                    }
                }
            }
        }

        public static void AddAllControlViews(StackLayout FormViewContainer, List<EbMobileControl> Controls,
            FormMode FormMode, NetworkMode NetWorkType, EbDataRow Context, string Parent, bool IsGrid)
        {
            if (Controls.Count == 0)
                return;

            List<View> views = new List<View>();
            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition());
            views.Add(grid);
            int cur_totwidth = 0;
            View CtrlView;
            int columIndex = 0;

            foreach (EbMobileControl ctrl in Controls)
            {
                ctrl.Parent = Parent;

                if (ctrl.Width <= 0 || ctrl.Width > 100)
                    ctrl.Width = 100;

                if (ctrl is EbMobileTableLayout table)
                    CtrlView = table.GetGridObject(Parent, FormMode, NetWorkType, Context);
                else if (IsGrid)
                    CtrlView = ctrl.XControl == null ? ctrl.Draw(FormMode, NetWorkType) : ctrl.XView;
                else
                    CtrlView = ctrl.Draw(FormMode, NetWorkType, Context);

                if (cur_totwidth + ctrl.Width > 100)
                {
                    grid = new Grid();
                    grid.RowDefinitions.Add(new RowDefinition());
                    views.Add(grid);
                    cur_totwidth = 0;
                    columIndex = 0;
                }

                if (columIndex > 0 && CtrlView is StackLayout layout)
                    layout.Padding = new Thickness(0, layout.Padding.Top, layout.Padding.Right, layout.Padding.Bottom);

                grid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(ctrl.Width, GridUnitType.Star)
                });
                grid.Children.Add(CtrlView, columIndex++, 0);
                cur_totwidth += ctrl.Width;
            }
            foreach (View v in views)
                FormViewContainer.Children.Add(v);
        }

        private void InitValueExpr(ExprDependency exprDep, string trigger_control, string parent = CTRL_PARENT_FORM)
        {
            if (exprDep.HasValueDependency)
            {
                foreach (string name in exprDep.ValueExpr)
                {
                    EbMobileControl ctrl = GetControl(name);
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
                    EbMobileControl ctrl = GetControl(name);
                    this.EvaluateHideExpr(ctrl, parent);
                }
            }
            else if (currentControl != null && currentControl.HiddenExpr?.IsEmpty() == false)
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
                    EbMobileControl ctrl = GetControl(name);
                    this.EvaluateDisableExpr(ctrl, parent);
                }
            }
            else if (currentControl != null && currentControl.DisableExpr?.IsEmpty() == false)
            {
                this.EvaluateDisableExpr(currentControl, parent);
            }
        }

        private void SetDefaultValueInternal(string controlName, string parent = CTRL_PARENT_FORM)
        {
            if (this.dependencyMap.HasDependency($"{parent}.{controlName}"))
            {
                ExprDependency exprDep = this.dependencyMap.GetDependency($"{parent}.{controlName}");

                if (exprDep.HasDefaultDependency)
                {
                    foreach (string name in exprDep.DefaultValueExpr)
                        this.SetDefaultValueInternal(name, parent);
                }
            }

            EbMobileControl ctrl = GetControl($"{parent}.{controlName}");

            if (ctrl.DefaultValueExpression != null && !ctrl.DefaultValueExpression.IsEmpty())
            {
                this.EvaluateDefaultValueExpr(ctrl, parent);
            }
        }

        private bool InitValidators(string controlName, string parent = CTRL_PARENT_FORM)
        {
            EbMobileControl ctrl = GetControl($"{parent}.{controlName}");

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

            string computed = GetComputedExpression(expr, ctrl.Name, parent);

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

        private void EvaluateHideExpr(EbMobileControl ctrl, string parent)
        {
            string expr = ctrl.HiddenExpr.GetCode();

            string computed = GetComputedExpression(expr, ctrl.Name, parent);

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

        private void EvaluateDisableExpr(EbMobileControl ctrl, string parent)
        {
            string expr = ctrl.DisableExpr.GetCode();

            string computed = GetComputedExpression(expr, ctrl.Name, parent);

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

        private void EvaluateDefaultValueExpr(EbMobileControl ctrl, string parent)
        {
            string expr = ctrl.DefaultValueExpression.GetCode();

            string computed = GetComputedExpression(expr, ctrl.Name, parent);

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

        private bool EvaluateValidatorExpr(EbMobileValidator validator, string controlName, string parent)
        {
            string expr = validator.Script.GetCode();

            string computed = GetComputedExpression(expr, controlName, parent);

            try
            {
                return evaluator.Execute<bool>(computed);
            }
            catch (Exception ex)
            {
                EbLog.Info($"validator script evaluation error in control '{controlName}'");
                EbLog.Error(ex.Message);
            }
            return false;
        }

        private string GetComputedExpression(string expression, string control, string parent)
        {
            string computed = expression;

            if (computed.Contains("this"))
            {
                computed = computed.Replace("this", CTRL_PARENT_FORM);
            }

            List<string> dependentNames = dependencyMap.GetControlNames();

            foreach (string name in dependentNames)
            {
                string[] parts = name.Split(CharConstants.DOT);

                string dependentParent = parts[0];
                string dependentControl = parts[1];

                if (computed.Contains($"{CTRL_PARENT_FORM}.{dependentControl}"))
                {
                    computed = computed.Replace($"{CTRL_PARENT_FORM}.{dependentControl}", $"{CTRL_PARENT_FORM}.Controls[\"{dependentControl}\"]");
                }

                if (parent != CTRL_PARENT_FORM && computed.Contains($"{CTRL_PARENT_FORM}.{dependentParent}"))
                {
                    computed = computed.Replace($"{CTRL_PARENT_FORM}.{dependentParent}", $"{CTRL_PARENT_FORM}.Controls[\"{dependentParent}\"]");
                }

                if (computed.Contains($"currentRow[\"{dependentControl}\"]"))
                {
                    computed = computed.Replace($"currentRow[\"{dependentControl}\"]", $"GetControl(\"{dependentControl}\")");
                }

                if (computed.Contains($"sum(\"{dependentControl}\")"))
                {
                    computed = computed.Replace($"sum(\"{dependentControl}\")", $"Sum(\"{dependentControl}\")");
                }
            }
            return computed;
        }

        private EbMobileControl GetControl(string nameExpression)
        {
            string[] nameParts = nameExpression.Split(CharConstants.DOT);

            string parent = nameParts[0];
            string control = nameParts[1];

            if (parent == CTRL_PARENT_FORM)
            {
                return this.controls[control];
            }
            else
            {
                if (this.controls.TryGetValue(parent, out var childctrl) && childctrl is ILinesEnabled lines)
                {
                    return lines.ChildControls.Find(c => c.Name == control);
                }
            }
            return null;
        }

        private EbMobileControl GetControlByName(string controlName)
        {
            foreach (EbMobileControl ctrl in this.controls.Values)
            {
                if (ctrl.Name == controlName) return ctrl;

                if (ctrl is ILinesEnabled lines)
                {
                    foreach (var linectrl in lines.ChildControls)
                    {
                        if (linectrl.Name == controlName) return linectrl;
                    }
                }
            }
            return null;
        }
    }
}
