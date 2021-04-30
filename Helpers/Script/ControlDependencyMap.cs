using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ExpressBase.Mobile.Helpers.Script
{
    public sealed class ControlDependencyMap
    {
        public static readonly Regex EbScriptRegex = new Regex(@"(?:form)\S+\w+");

        public Dictionary<string, Dictionary<string, ExprDependency>> DependencyMap { set; get; }

        public Dictionary<string, List<string>> Names { set; get; }

        public const string CTRL_PARENT_FORM = "form";

        public ControlDependencyMap()
        {
            DependencyMap = new Dictionary<string, Dictionary<string, ExprDependency>>();

            Names = new Dictionary<string, List<string>>
            {
                { CTRL_PARENT_FORM, new List<string>() }
            };
        }

        public void Init(Dictionary<string, EbMobileControl> controls)
        {
            Names[CTRL_PARENT_FORM].AddRange(controls.Keys.ToArray());

            foreach (EbMobileControl ctrl in controls.Values)
            {
                bool hasValueExpr = ctrl.HasExpression(ExprType.ValueExpr);
                bool hasHideExpr = ctrl.HasExpression(ExprType.HideExpr);
                bool hasDisableExpr = ctrl.HasExpression(ExprType.DisableExpr);
                bool hasDefaultExpr = ctrl.HasExpression(ExprType.DefaultExpr);

                ctrl.Parent = CTRL_PARENT_FORM;

                if (ctrl is ILinesEnabled)
                {
                    InitLines(ctrl);
                }

                if (!hasValueExpr && !hasHideExpr && !hasDisableExpr && !hasDefaultExpr) continue;

                try
                {
                    if (hasValueExpr)
                    {
                        InitializeDependency(ctrl, ctrl.ValueExpr.GetCode(), ExprType.ValueExpr, CTRL_PARENT_FORM);
                    }

                    if (hasHideExpr)
                    {
                        InitializeDependency(ctrl, ctrl.HiddenExpr.GetCode(), ExprType.HideExpr, CTRL_PARENT_FORM);
                    }

                    if (hasDisableExpr)
                    {
                        InitializeDependency(ctrl, ctrl.DisableExpr.GetCode(), ExprType.DisableExpr, CTRL_PARENT_FORM);
                    }

                    if (hasDefaultExpr)
                    {
                        foreach (string dependent in GetDependentNames(ctrl.DefaultValueExpression.GetCode()))
                        {
                            CreateKeyPair(ctrl.Name);
                            ThrowDefaltExprCircularRef(dependent, ctrl.Name);

                            DependencyMap[CTRL_PARENT_FORM][ctrl.Name].AddDefaultDependent(dependent);
                        }
                    }
                }
                catch (Exception ex)
                {
                    EbLog.Error(ex.Message);
                    Utils.Toast(ex.Message);
                    break;
                }
            }
        }

        private void InitLines(EbMobileControl control)
        {
            string parent = control.Name;

            Names[parent] = new List<string>();

            foreach (EbMobileControl ctrl in (control as ILinesEnabled).ChildControls)
            {
                ctrl.Parent = parent;

                Names[parent].Add(ctrl.Name);

                bool hasValueExpr = ctrl.HasExpression(ExprType.ValueExpr);
                bool hasHideExpr = ctrl.HasExpression(ExprType.HideExpr);
                bool hasDisableExpr = ctrl.HasExpression(ExprType.DisableExpr);
                bool hasDefaultExpr = ctrl.HasExpression(ExprType.DefaultExpr);

                if (hasValueExpr)
                {
                    InitializeDependency(ctrl, ctrl.ValueExpr.GetCode(), ExprType.ValueExpr, parent);
                }

                if (hasHideExpr)
                {
                    InitializeDependency(ctrl, ctrl.HiddenExpr.GetCode(), ExprType.HideExpr, parent);
                }

                if (hasDisableExpr)
                {
                    InitializeDependency(ctrl, ctrl.DisableExpr.GetCode(), ExprType.DisableExpr, parent);
                }

                if (hasDefaultExpr)
                {
                    foreach (string dependent in GetDependentNames(ctrl.DefaultValueExpression.GetCode(), parent))
                    {
                        CreateKeyPair(ctrl.Name, parent);
                        ThrowDefaltExprCircularRef(dependent, parent);

                        DependencyMap[parent][ctrl.Name].AddDefaultDependent(dependent);
                    }
                }
            }
        }

        private void InitializeDependency(EbMobileControl control, string script, ExprType type, string parent)
        {
            if (!DependencyMap.ContainsKey(parent))
            {
                DependencyMap.Add(parent, new Dictionary<string, ExprDependency>());
            }

            foreach (string dependent in GetDependentNames(script, parent))
            {
                CreateKeyPair(dependent, parent);

                DependencyMap[parent][dependent].Add(type, control.Name);
            }
        }

        private void ThrowDefaltExprCircularRef(string source, string dependentControl, string parent = CTRL_PARENT_FORM)
        {
            if (DependencyMap.ContainsKey(parent) && DependencyMap[parent].ContainsKey(source))
            {
                ExprDependency depExpr = this.GetDependency(source, parent);

                if (depExpr.DefaultValueExpr.Contains(dependentControl))
                {
                    throw new Exception($"Circular reference detected in default expression of '{dependentControl}'");
                }
            }
        }

        private void CreateKeyPair(string controlName, string parent = CTRL_PARENT_FORM)
        {
            if (DependencyMap.ContainsKey(parent) && !DependencyMap[parent].ContainsKey(controlName))
            {
                DependencyMap[parent].Add(controlName, new ExprDependency());
            }
        }

        private List<string> GetDependentNames(string code, string parent = CTRL_PARENT_FORM)
        {
            List<string> ls = new List<string>();

            foreach (string name in Names[parent])
            {
                if (code.Contains(name))
                {
                    ls.Add(name);
                }
            }
            return ls;
        }

        public bool HasDependency(string controlName, string parent = CTRL_PARENT_FORM)
        {
            return DependencyMap.ContainsKey(parent) && DependencyMap[parent].ContainsKey(controlName);
        }

        public ExprDependency GetDependency(string controlName, string parent = CTRL_PARENT_FORM)
        {
            return DependencyMap[parent][controlName];
        }

        public List<string> GetNames(string parent)
        {
            return Names[parent];
        }
    }
}
