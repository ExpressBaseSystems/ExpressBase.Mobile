using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Helpers.Script
{
    public interface IScriptHelper
    {
        T Evaluate<T>(string statement, string expr);
    }

    public class EbSciptEvaluator
    {
        private IScriptHelper scriptHelper;

        private static EbSciptEvaluator instance;

        public static EbSciptEvaluator Instance => instance ??= new EbSciptEvaluator();

        private EbSciptEvaluator()
        {
            scriptHelper = DependencyService.Get<IScriptHelper>();
        }

        public T Execute<T>(string statement, string expr)
        {
            return (T)scriptHelper.Evaluate<T>(statement,expr);
        }
    }
}
