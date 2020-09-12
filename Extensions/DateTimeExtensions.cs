using ExpressBase.Mobile.Helpers;
using System;

namespace ExpressBase.Mobile.Extensions
{
    public static class DateTimeExtensions
    {
        public static string SubtractByNow(this DateTime date)
        {
            try
            {
                DateTime now = DateTime.Now;
                int days = DateTime.DaysInMonth(now.Year, now.Month);

                TimeSpan ts = now.Subtract(date);

                int intDays = ts.Days;
                int intHours = ts.Hours;
                int intMinutes = ts.Minutes;
                int intSeconds = ts.Seconds;

                if (intDays > days)
                    return $"{intDays / days} months ago";

                if (intDays > 0)
                    return $"{intDays - 1} days ago";

                if (intHours > 0)
                    return $"{intHours} hours ago";

                if (intMinutes > 0)
                    return $"{intMinutes} minutes ago";

                if (intSeconds > 0)
                    return $"just now";
            }
            catch (Exception ex)
            {
                EbLog.Error(ex.Message);
            }

            return $"... ago";
        }
    }
}
