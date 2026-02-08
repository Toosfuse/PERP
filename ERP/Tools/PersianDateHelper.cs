using System.Globalization;
namespace ERP.Tools
{
    public static class PersianDateHelper
    {
        private static readonly PersianCalendar pc = new PersianCalendar();

        public static string GetPersianWeekDateRange(int persianYear, int persianWeek)
        {
            try
            {
                var firstDayOfYear = new DateTime(persianYear, 1, 1, pc);
                var firstDayOfWeek = firstDayOfYear.AddDays((persianWeek - 1) * 7);

                // تنظیم دقیق به شنبه
                while (firstDayOfWeek.DayOfWeek != DayOfWeek.Saturday)
                    firstDayOfWeek = firstDayOfWeek.AddDays(-1);

                var lastDayOfWeek = firstDayOfWeek.AddDays(6);

                string start = $"{pc.GetYear(firstDayOfWeek):0000}/{pc.GetMonth(firstDayOfWeek):00}/{pc.GetDayOfMonth(firstDayOfWeek):00}";
                string end = $"{pc.GetYear(lastDayOfWeek):0000}/{pc.GetMonth(lastDayOfWeek):00}/{pc.GetDayOfMonth(lastDayOfWeek):00}";

                return $"{start} تا {end}";
            }
            catch
            {
                return "تاریخ نامشخص";
            }
        }
    }
}
