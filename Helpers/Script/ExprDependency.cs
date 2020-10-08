using System.Collections.Generic;
using System.Linq;

namespace ExpressBase.Mobile.Helpers.Script
{
    public class ExprDependency
    {
        public List<string> ValueExpr { set; get; }

        public List<string> HideExpr { set; get; }

        public List<string> DisableExpr { set; get; }

        public bool HasValueDependency => this.ValueExpr.Any();

        public bool HasHideDependency => this.HideExpr.Any();

        public bool HasDisableDependency => this.DisableExpr.Any();

        public ExprDependency()
        {
            ValueExpr = new List<string>();
            HideExpr = new List<string>();
            DisableExpr = new List<string>();
        }

        public void AddValueDependent(string dependent)
        {
            if (!ValueExpr.Contains(dependent))
                ValueExpr.Add(dependent);
        }

        public void AddHideDependent(string dependent)
        {
            if (!HideExpr.Contains(dependent))
                HideExpr.Add(dependent);
        }

        public void AddDisableDependent(string dependent)
        {
            if (!DisableExpr.Contains(dependent))
                DisableExpr.Add(dependent);
        }
    }
}
