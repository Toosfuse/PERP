using System.Globalization;

namespace ERP.Services
{
    public class Services: IServices
    {
        public DateTime ConvertPersianDate(string persianDateString, bool useCurrentTime = true, TimeSpan? specificTime = null)
        {
            var parts = persianDateString.Split('/');
            if (parts.Length != 3)
                throw new FormatException("فرمت تاریخ شمسی اشتباه است. باید yyyy/MM/dd باشد.");

            int year = int.Parse(parts[0]);
            int month = int.Parse(parts[1]);
            int day = int.Parse(parts[2]);

            var pc = new PersianCalendar();

            int hour = 0, minute = 0, second = 0;
            if (useCurrentTime)
            {
                var now = DateTime.Now;
                hour = now.Hour;
                minute = now.Minute;
                second = now.Second;
            }
            else if (specificTime.HasValue)
            {
                hour = specificTime.Value.Hours;
                minute = specificTime.Value.Minutes;
                second = specificTime.Value.Seconds;
            }

            return pc.ToDateTime(year, month, day, hour, minute, second, 0);
        }

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
