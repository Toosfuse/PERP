using ERP.Data;
using ERP.Models;

using ICSharpCode.SharpZipLib.GZip;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net;
namespace ERP.Controllers
{
    [Authorize(Roles = "SuperAdmin,Management,Edari,AfterSales,QA")]
    public class ReportsController : Controller
    {
        private readonly ERPContext _context;

        //private readonly EMPContext _contextemp;

        public ReportsController(ERPContext context)
        {
            _context = context;
        }
        public static string GregorianToPersian(DateTime gregorianDate)
        {
            var persianCalendar = new PersianCalendar();

            // تبدیل تاریخ میلادی به تاریخ شمسی
            int year = persianCalendar.GetYear(gregorianDate);
            int month = persianCalendar.GetMonth(gregorianDate);
            int day = persianCalendar.GetDayOfMonth(gregorianDate);
            int hour = persianCalendar.GetHour(gregorianDate);
            int minute = persianCalendar.GetMinute(gregorianDate);

            // برگرداندن تاریخ به فرمت yyyy/mm/dd
            return $"{year}/{month:D2}/{day:D2}  {hour:D2}:{minute:D2}";
        }
        public IActionResult EmploymentCount()
        {
            return View();
        }

        public IActionResult LoginReport()
        {
            return View();
        }
     

        [HttpGet]
        public IActionResult QuestionScores()
        {
            return View();
        }


        int ExtractScore(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            // پیدا کردن عدد داخل پرانتز
            var start = value.IndexOf('(');
            var end = value.IndexOf(" امتیاز");

            if (start != -1 && end != -1 && end > start)
            {
                var numberPart = value.Substring(start + 1, end - start - 1);
                if (int.TryParse(numberPart, out var score))
                    return score;
            }

            return 0;
        }

        [HttpGet]
        public IActionResult TopUserScores()
        {
            return View();
        }

    }
}
