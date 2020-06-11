using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressBase.Mobile.Extensions
{
    public static class DateTimeExtensions
    {
        public static string SubtractByNow(this DateTime date)
        {
            string subtracted;
            try
            {
                DateTime now = DateTime.Now;
                var days = DateTime.DaysInMonth(now.Year, now.Month);

                var diff = now.Subtract(date);

                if (date.Date == now.Date)
                    subtracted = $"{diff.Hours} hours ago";
                else if (diff.Days == 1)
                    subtracted = "Yesterday";
                else if (diff.Days <= days)
                    subtracted = $"{diff.Days - 1} days ago";
                else
                    subtracted = $"{(int)diff.Days / days} months ago";
            }
            catch (Exception)
            {
                subtracted = "... ago";
            }
            return subtracted;
        }
    }
}
