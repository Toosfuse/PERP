using System.Globalization;

namespace ERP.Services
{
    public class Services: IServices
    {
        public string iGregorianToPersian(DateTime? date)
        {
            if (!date.HasValue)
            {
                return "-"; // یا هر مقدار پیش‌فرض دیگری که برای تاریخ خالی مناسب است
            }
            var persianCalendar = new PersianCalendar();

            // تبدیل تاریخ میلادی به تاریخ شمسی
            int year = persianCalendar.GetYear(date.Value);
            int month = persianCalendar.GetMonth(date.Value);
            int day = persianCalendar.GetDayOfMonth(date.Value);

            // برگرداندن تاریخ به فرمت yyyy/mm/dd
            return $"{year}/{month:D2}/{day:D2}";
        }
        public string iGregorianToPersianDateTime(DateTime? date)
        {
            if (!date.HasValue)
                return "-";

            var pc = new PersianCalendar();
            var d = date.Value;

            int year = pc.GetYear(d);
            int month = pc.GetMonth(d);
            int day = pc.GetDayOfMonth(d);

            int hour = d.Hour;
            int minute = d.Minute;

            return $"{year}/{month:D2}/{day:D2} {hour:D2}:{minute:D2}";
        }
    }
}
