using ExpressBase.Mobile.Constants;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.RegularExpressions;

namespace ExpressBase.Mobile.Helpers.Script
{
    public class ControlDependencyMap : DynamicObject
    {
        public static readonly Regex EbScriptRegex = new Regex(@"(?:form)\S+\w+");

        protected Dictionary<string, ExprDependency> DependencyMap { set; get; }

        public ControlDependencyMap()
        {
            DependencyMap = new Dictionary<string, ExprDependency>();
        }

        public virtual void Init(Dictionary<string, EbMobileControl> controls)
        {
            foreach (EbMobileControl ctrl in controls.Values)
            {
                bool hasValueExpr = ctrl.ValueExpr != null && !ctrl.ValueExpr.IsEmpty();
                bool hasHideExpr = ctrl.HiddenExpr != null && !ctrl.HiddenExpr.IsEmpty();
                bool hasDisableExpr = ctrl.DisableExpr != null && !ctrl.DisableExpr.IsEmpty();

                if (!hasValueExpr && !hasHideExpr && !hasDisableExpr) continue;

                try
                {
                    if (hasValueExpr)
                    {
                        foreach (string name in GetDependentNames(ctrl.ValueExpr.GetCode()))
                        {
                            if (!controls.ContainsKey(name))
                                throw new Exception($"Unknown Control name '{name}' in value expression of '{ctrl.Name}'");
                            this.CreateKeyPair(name);
                            this.DependencyMap[name].AddValueDependent(ctrl.Name);
                        }
                    }

                    if (hasHideExpr)
                    {
                        foreach (string name in GetDependentNames(ctrl.HiddenExpr.GetCode()))
                        {
                            if (!controls.ContainsKey(name))
                                throw new Exception($"Unknown Control name '{name}' in hide expression of '{ctrl.Name}'");
                            this.CreateKeyPair(name);
                            this.DependencyMap[name].AddHideDependent(ctrl.Name);
                        }
                    }

                    if (hasDisableExpr)
                    {
                        foreach (string name in GetDependentNames(ctrl.DisableExpr.GetCode()))
                        {
                            if (!controls.ContainsKey(name))
                                throw new Exception($"Unknown Control name '{name}' in disable expression of '{ctrl.Name}'");
                            this.CreateKeyPair(name);
                            this.DependencyMap[name].AddDisableDependent(ctrl.Name);
                        }
                    }
                }
                catch (Exception ex)
                {
                    EbLog.Error(ex.Message);
                    break;
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

        public ExprDependency GetDependency(string controlName)
        {
            return DependencyMap[controlName];
        }
    }
}
