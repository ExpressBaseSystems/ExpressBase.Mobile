using ExpressBase.Mobile.Constants;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpressBase.Mobile.Helpers.Script
{
    public sealed class ControlDependencyMap
    {
        public List<string> NameCollection { set; get; }

        public Dictionary<string, ExprDependency> DependencyMapCollection { set; get; }

        public const string CTRL_PARENT_FORM = "form";

        public ControlDependencyMap()
        {
            DependencyMapCollection = new Dictionary<string, ExprDependency>();

            NameCollection = new List<string>();
        }

        private void InitNameCollection(Dictionary<string, EbMobileControl> controls)
        {
            foreach (EbMobileControl control in controls.Values)
            {
                if (control is ILinesEnabled lines)
                {
                    foreach (EbMobileControl linesCtrl in lines.ChildControls)
                    {
                        string key = $"{control.Name}.{linesCtrl.Name}";
                        NameCollection.Add(key);
                    }
                }
                string keyControl = $"{CTRL_PARENT_FORM}.{control.Name}";
                NameCollection.Add(keyControl);
            }
        }

        public void Init(Dictionary<string, EbMobileControl> controls)
        {
            InitNameCollection(controls);

            foreach (EbMobileControl control in controls.Values)
            {
                if (control is ILinesEnabled lines)
                {
                    foreach (EbMobileControl linesCtrl in lines.ChildControls)
                    {
                        InitializeDependency(linesCtrl, control.Name);
                    }
                }
                else
                {
                    InitializeDependency(control, CTRL_PARENT_FORM);
                }
            }
        }

        private void InitializeDependency(EbMobileControl control, string parent)
        {
            foreach (int value in Enum.GetValues(typeof(ExpressionType)))
            {
                if (control.HasExpression((ExpressionType)value, out string script))
                {
                    foreach (string dependent in GetDependentNames(script))
                    {
                        if (!DependencyMapCollection.ContainsKey(dependent))
                        {
                            DependencyMapCollection[dependent] = new ExprDependency();
                        }
                        DependencyMapCollection[dependent].Add((ExpressionType)value, $"{parent}.{control.Name}");
                    }
                }
            }
        }

        private List<string> GetDependentNames(string code)
        {
            List<string> ls = new List<string>();

            foreach (string name in NameCollection)
            {
                List<string> keywords = GetKeyWords(name);

                if (keywords.Any(item => code.Contains(item)))
                {
                    ls.Add(name);
                }
            }
            return ls;
        }

        private List<string> GetKeyWords(string name)
        {
            string[] parts = name.Split(CharConstants.DOT);
            string parent = parts[0];
            string control = parts[1];

            if (parent.Equals(CTRL_PARENT_FORM))
            {
                return new List<string> 
                {
                    $"{parent}.{control}"
                };
            }
            else
            {
                return new List<string>
                {
                    $"{CTRL_PARENT_FORM}.{parent}.currentRow[\"{control}\"]",
                    $"{CTRL_PARENT_FORM}.{parent}.sum(\"{control}\")"
                };
            }
        }

        public bool HasDependency(string name)
        {
            return DependencyMapCollection.ContainsKey(name);
        }

        public bool HasDependency(string control, string parent, out ExprDependency dependency)
        {
            return DependencyMapCollection.TryGetValue($"{parent}.{control}", out dependency);
        }

        public ExprDependency GetDependency(string name)
        {
            if (DependencyMapCollection.TryGetValue(name, out ExprDependency dependency))
            {
                return dependency;
            }
            return null;
        }

        public List<string> GetControlNames()
        {
            return NameCollection;
        }
    }
}
