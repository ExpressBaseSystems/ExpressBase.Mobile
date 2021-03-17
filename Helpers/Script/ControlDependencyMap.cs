using ExpressBase.Mobile.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ExpressBase.Mobile.Helpers.Script
{
    public sealed class ControlDependencyMap
    {
        public static readonly Regex EbScriptRegex = new Regex(@"(?:form)\S+\w+");

        public Dictionary<string, ExprDependency> DependencyMap { set; get; }

        public Dictionary<string, Dictionary<string, ExprDependency>> GridDependencyMap { set; get; }

        public ControlDependencyMap()
        {
            DependencyMap = new Dictionary<string, ExprDependency>();

            GridDependencyMap = new Dictionary<string, Dictionary<string, ExprDependency>>();
        }

        public void Init(Dictionary<string, EbMobileControl> controls)
        {
            string[] allKeys = controls.Keys.ToArray();

            foreach (EbMobileControl ctrl in controls.Values)
            {
                bool hasValueExpr = ctrl.HasExpression(ExprType.ValueExpr);
                bool hasHideExpr = ctrl.HasExpression(ExprType.HideExpr);
                bool hasDisableExpr = ctrl.HasExpression(ExprType.DisableExpr);
                bool hasDefaultExpr = ctrl.HasExpression(ExprType.DefaultExpr);

                ctrl.Parent = EbFormHelper.CTRL_PARENT_FORM;

                if (ctrl is ILinesEnabled)
                {
                    InitLines(ctrl);
                }

                if (!hasValueExpr && !hasHideExpr && !hasDisableExpr && !hasDefaultExpr) continue;

                try
                {
                    if (hasValueExpr)
                    {
                        this.InitializeDependency(ctrl, ctrl.ValueExpr.GetCode(), ExprType.ValueExpr, allKeys);
                    }

                    if (hasHideExpr)
                    {
                        this.InitializeDependency(ctrl, ctrl.HiddenExpr.GetCode(), ExprType.HideExpr, allKeys);
                    }

                    if (hasDisableExpr)
                    {
                        this.InitializeDependency(ctrl, ctrl.DisableExpr.GetCode(), ExprType.DisableExpr, allKeys);
                    }

                    if (hasDefaultExpr)
                    {
                        foreach (string dependent in GetDependentNames(ctrl.DefaultValueExpression.GetCode()))
                        {
                            if (!controls.ContainsKey(dependent))
                                throw new Exception($"Unknown Control name '{dependent}' in default expression of '{ctrl.Name}'");

                            this.CreateKeyPair(ctrl.Name);
                            this.ThrowDefaltExprCircularRef(dependent, ctrl.Name);

                            this.DependencyMap[ctrl.Name].AddDefaultDependent(dependent);
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
            foreach (EbMobileControl ctrl in (control as ILinesEnabled).ChildControls)
            {
                ctrl.Parent = control.Name;

                bool hasValueExpr = ctrl.HasExpression(ExprType.ValueExpr);
                bool hasHideExpr = ctrl.HasExpression(ExprType.HideExpr);
                bool hasDisableExpr = ctrl.HasExpression(ExprType.DisableExpr);
                bool hasDefaultExpr = ctrl.HasExpression(ExprType.DefaultExpr);

                if (hasValueExpr)
                {
                    this.InitializeDependency(ctrl, ctrl.ValueExpr.GetCode(), ExprType.ValueExpr, control.Name);
                }

                if (hasHideExpr)
                {
                    this.InitializeDependency(ctrl, ctrl.HiddenExpr.GetCode(), ExprType.HideExpr, control.Name);
                }

                if (hasDisableExpr)
                {
                    this.InitializeDependency(ctrl, ctrl.DisableExpr.GetCode(), ExprType.DisableExpr, control.Name);
                }

                if (hasDefaultExpr)
                {

                }
            }
        }

        private void InitializeDependency(EbMobileControl control, string script, ExprType type, string[] allKeys)
        {
            foreach (string dependent in GetDependentNames(script))
            {
                if (!allKeys.Contains(dependent))
                    throw new Exception($"Unknown Control name '{dependent}' in expression type '{type}' of control '{control.Name}'");

                this.CreateKeyPair(dependent);

                this.DependencyMap[dependent].Add(type, control.Name);
            }
        }

        private void InitializeDependency(EbMobileControl control, string script, ExprType type, string parent)
        {
            if (!GridDependencyMap.ContainsKey(parent))
            {
                GridDependencyMap.Add(parent, new Dictionary<string, ExprDependency>());
            }

            foreach (string dependent in GetDependentNames(script))
            {
                CreateGridKeyPair(dependent, parent);

                GridDependencyMap[parent][dependent].Add(type, control.Name);
            }
        }

        private void ThrowDefaltExprCircularRef(string source, string dependentControl)
        {
            if (DependencyMap.ContainsKey(source))
            {
                ExprDependency depExpr = this.GetDependency(source);

                if (depExpr.DefaultValueExpr.Contains(dependentControl))
                {
                    throw new Exception($"Circular reference detected in default expression of '{dependentControl}'");
                }
            }
        }

        private void CreateKeyPair(string controlName)
        {
            if (!DependencyMap.ContainsKey(controlName))
            {
                DependencyMap.Add(controlName, new ExprDependency());
            }
        }

        private void CreateGridKeyPair(string controlName, string parent)
        {
            if (GridDependencyMap.ContainsKey(parent) && !GridDependencyMap[parent].ContainsKey(controlName))
            {
                GridDependencyMap[parent].Add(controlName, new ExprDependency());
            }
        }

        private List<string> GetDependentNames(string code)
        {
            var ls = new List<string>();

            foreach (Match match in EbScriptRegex.Matches(code))
            {
                string[] parts = match.Value.Split(CharConstants.DOT);

                if (parts.Length < 3)
                    continue;

                ls.Add(parts[1]);
            }
            return ls;
        }

        public bool HasDependency(string controlName)
        {
            return DependencyMap.ContainsKey(controlName);
        }

        public bool HasDependency(string controlName, string parent)
        {
            return GridDependencyMap.ContainsKey(parent) && GridDependencyMap[parent].ContainsKey(controlName);
        }

        public ExprDependency GetDependency(string controlName)
        {
            return DependencyMap[controlName];
        }

        public ExprDependency GetDependency(string controlName, string parent)
        {
            return GridDependencyMap[parent][controlName];
        }
    }
}
