using System.Collections.Generic;
using System.Linq;

namespace ExpressBase.Mobile.Helpers.Script
{
    public enum ExpressionType
    {
        ValueExpression,
        HideExpression,
        DisableExpression,
        DefaultExpression
    }

    public class ExprDependency
    {
        public List<string> ValueExpr { set; get; }

        public List<string> HideExpr { set; get; }

        public List<string> DisableExpr { set; get; }

        public List<string> DefaultValueExpr { set; get; }

        public bool HasValueDependency => this.ValueExpr.Any();

        public bool HasHideDependency => this.HideExpr.Any();

        public bool HasDisableDependency => this.DisableExpr.Any();

        public bool HasDefaultDependency => this.DefaultValueExpr.Any();

        public ExprDependency()
        {
            ValueExpr = new List<string>();
            HideExpr = new List<string>();
            DisableExpr = new List<string>();
            DefaultValueExpr = new List<string>();
        }

        public void Add(ExpressionType type, string dependent)
        {
            switch (type)
            {
                case ExpressionType.ValueExpression:
                    AddValueDependent(dependent);
                    break;
                case ExpressionType.HideExpression:
                    AddHideDependent(dependent);
                    break;
                case ExpressionType.DisableExpression:
                    AddDisableDependent(dependent);
                    break;
                case ExpressionType.DefaultExpression:
                    AddDefaultDependent(dependent);
                    break;
            }
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

        public void AddDefaultDependent(string dependent)
        {
            if (!DefaultValueExpr.Contains(dependent))
                DefaultValueExpr.Add(dependent);
        }
    }
}
