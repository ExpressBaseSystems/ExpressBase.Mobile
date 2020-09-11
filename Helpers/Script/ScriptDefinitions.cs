using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressBase.Mobile.Helpers.Script
{
    public class ScriptDefinitions
    {
        public const string FormRenderEval =
        @"
            public class EbEvaluator 
            {
                public string Validate()
                {
                    return ""Hi"";
                }
            }
        ";
    }
}
