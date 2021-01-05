using ExpressBase.Mobile.Data;
using ExpressBase.Mobile.Helpers.Script;
using System;
using Xamarin.Forms;

namespace ExpressBase.Mobile.Helpers
{
    public class EbListHelper
    {
        private static EbListHelper instance;

        public static EbListHelper Instance => instance ??= new EbListHelper();

        private readonly EbSciptEvaluator evaluator;

        private readonly EbListEvaluator context;

        private EbListHelper()
        {
            evaluator = new EbSciptEvaluator
            {
                OptionScriptNeedSemicolonAtTheEndOfLastExpression = false
            };

            evaluator.Context = context = new EbListEvaluator();
        }

        public static void SetDataRow(EbDataRow row)
        {
            Instance.context.SetCurrentRow(row);
        }

        public static bool EvaluateHideExpr(string script)
        {
            try
            {
                return Instance.evaluator.Execute<bool>(script);
            }
            catch (Exception ex)
            {
                EbLog.Error("list hide expr failure, " + ex.Message);
                return false;
            }
        }

        public static void EvaluateValueExpr(View view, string script)
        {
            try
            {
                Instance.context.SetCurrentView(view);
                Instance.evaluator.Execute(script);
            }
            catch (Exception ex)
            {
                EbLog.Error("list hide expr failure, " + ex.Message);
            }
        }

        public static bool EvaluateLinkExpr(EbDataRow row, string script)
        {
            try
            {
                SetDataRow(row);
                return Instance.evaluator.Execute<bool>(script);
            }
            catch (Exception ex)
            {
                EbLog.Error("list hide expr failure, " + ex.Message);
            }
            return true;
        }
    }
}
