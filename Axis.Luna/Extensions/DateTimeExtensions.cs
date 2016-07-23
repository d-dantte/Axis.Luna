using System;
using System.Globalization;

namespace Axis.Luna.Extensions
{
    public static class DateTimeExtensions
    {
        public static string FriendlyDisplay(this DateTime date)
        {
            var now = DateTime.Now;
            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            Calendar cal = dfi.Calendar;

            if (now.Year != date.Year) return date.ToString("MMM, yyyy");
            else if (now.Month != date.Month) return $"{date.Day.AsOrdinal()}{date.ToString(" MMM")}";
            else if (cal.GetWeekOfYear(now, CalendarWeekRule.FirstDay, DayOfWeek.Sunday) !=
                    cal.GetWeekOfYear(date, CalendarWeekRule.FirstDay, DayOfWeek.Sunday))
            {
                return $"{date.Day.AsOrdinal()}{date.ToString(" MMM")}";
            }
            else if (now.Day != date.Day) return date.ToString("dddd");
            else return date.ToString("HH:mm");
        }
    }
}
