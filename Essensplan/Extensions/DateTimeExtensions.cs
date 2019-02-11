using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Essensplan.Extensions
{
    public static class DateTimeExtensions
    {
        public static string SayAsDate(this DateTime date)
        {
            return $"<say-as interpret-as=\"date\">????{date.ToString("MMdd")}</say-as>";
        }

        public static string SayAsDateYear(this DateTime date)
        {
            return $"<say-as interpret-as=\"date\">{date.ToShortDateString()}</say-as>";
        }

        public static int GetWeekOfYear(this DateTime date)
        {
            var cInfo = new CultureInfo("de-DE");
            var kalender = cInfo.Calendar;
            var weekRule = cInfo.DateTimeFormat.CalendarWeekRule;
            var dayOfWeek = cInfo.DateTimeFormat.FirstDayOfWeek;            

            return kalender.GetWeekOfYear(date, weekRule, dayOfWeek);
        }

        public static int GetNumberOfWeeks(this DateTime dateTime)
        {
            var cInfo = new CultureInfo("de-DE");
            var kalender = cInfo.Calendar;
            var weekRule = cInfo.DateTimeFormat.CalendarWeekRule;
            var dayOfWeek = cInfo.DateTimeFormat.FirstDayOfWeek;
            var date = new DateTime(dateTime.Year, 12, 31);

            return kalender.GetWeekOfYear(date, weekRule, dayOfWeek);
        }

        public static List<DateTime> GetDaysOfWeek(this DateTime date)
        {            
            var currentDayOfWeek = (int)date.DayOfWeek;
            var sonntag = date.AddDays(-currentDayOfWeek);
            var montag = sonntag.AddDays(1);
            
            if (currentDayOfWeek == 0)            
                montag = montag.AddDays(-7);
            
            return Enumerable.Range(0, 5).Select(d => montag.AddDays(d)).ToList();
        }
    }
}
