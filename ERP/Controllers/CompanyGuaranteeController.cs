using ERP.Data;
using ERP.Models;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Globalization;
using System.Text.Json;

namespace ERP.Controllers
{
    [Authorize(Roles = "SuperAdmin,Management,SalesMeter")]
    public class CompanyGuaranteeController : Controller
    {
        public static string GregorianToPersian(DateTime gregorianDate)
        {
            var persianCalendar = new PersianCalendar();
            int year = persianCalendar.GetYear(gregorianDate);
            int month = persianCalendar.GetMonth(gregorianDate);
            int day = persianCalendar.GetDayOfMonth(gregorianDate);
            return $"{year}/{month:D2}/{day:D2}";
        }

        public static DateTime PersianToGregorian(string persianDate)
        {
            var persianCalendar = new PersianCalendar();

            // فرض می‌کنیم که ورودی فرمت yyyy/mm/dd است
            var dateParts = persianDate.Split('/');
            int year = int.Parse(dateParts[0]);
            int month = int.Parse(dateParts[1]);
            int day = int.Parse(dateParts[2]);

            // تبدیل تاریخ شمسی به میلادی
            DateTime gregorianDate = persianCalendar.ToDateTime(year, month, day, 0, 0, 0, 0);

            return gregorianDate;
        }
        private readonly ERPContext _context;
        public CompanyGuaranteeController(ERPContext context)
        {
            _context = context;
        }
        public IActionResult IndexGuarantee()
        {
            ViewBag.all = _context.GuaranteeLetters.Count();
            ViewBag.s1 = _context.GuaranteeLetters.Count(p => p.GuaranteeStatus == "ابطال شد");
            ViewBag.s2 = _context.GuaranteeLetters.Count(p => p.GuaranteeStatus == "تا پایان دوره تضمین باید تمدید شود");
            ViewBag.s3 = _context.GuaranteeLetters.Count(p => p.GuaranteeStatus == "در انتظاراعلام برنده");
            ViewBag.s4 = _context.GuaranteeLetters.Count(p => p.GuaranteeStatus == "در حال اجرای قرارداد");
            ViewBag.s5 = _context.GuaranteeLetters.Count(p => p.GuaranteeStatus == "در حال پیگیری جهت ابطال");
            return View();
        }
        public IActionResult ListCoGuarantee([DataSourceRequest] DataSourceRequest request, string SelectTab)
        {
            if (SelectTab == "همه لیست")
            {
                var model = _context.GuaranteeLetters
                    .OrderByDescending(x => x.ExpirationDateGregorian)
                    .ToDataSourceResult(request);
                return Json(model);
            }
            else
            {
                var model = _context.GuaranteeLetters.Where(p=>p.GuaranteeStatus== SelectTab)
                    .OrderByDescending(x => x.ExpirationDateGregorian)
                    .ToDataSourceResult(request);
                return Json(model);
            }
            return null;
        }
        public async Task<IActionResult> CoGuarantee(int? id)
        {
            bool isReadOnly = false;
            if (id != null && id < 0)
            {
                isReadOnly = true;
                id = Math.Abs(id.Value);
            }
            ViewBag.IsReadOnly = isReadOnly;
            GuaranteeLetter form = null;
            if (id != null)
            {
                form = await _context.GuaranteeLetters.FindAsync(id);
                if (form == null)
                {
                    return NotFound();
                }
            }
            if (form == null)
            {
                form = new GuaranteeLetter();
            }
            return View(form);
        }

        [HttpGet("/Guarantee-excel")]
        public IActionResult ExportInqueriesToExcel()
        {
            var workbook = new XSSFWorkbook();

            var sheet = workbook.CreateSheet("ضمانت نامه");

            // سبک هدر
            var headerStyle = workbook.CreateCellStyle();
            headerStyle.FillForegroundColor = IndexedColors.Aqua.Index;
            headerStyle.FillPattern = FillPattern.SolidForeground;
            headerStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
            headerStyle.WrapText = true;


            var headerFont = workbook.CreateFont();
            headerFont.IsBold = true;
            headerStyle.SetFont(headerFont);

            // هدرها
            string[] headers = new[]
            {
             "نام شرکت توزیع",
            " شماره ضمانت نامه: ",
            " نوع ضمانتنامه ",
            " (مبلغ ضمانت نامه (ریال ",

            "تاریخ صدور",
            "تاریخ سررسید/ تمدید",
            "نام بانک عامل",
            "شماره قرارداد / مناقصه",

            "وضعیت قرارداد / مناقصه",
            " وضعیت ضمانت نامه ",
            "درصد پیشرفت قراردادهای معوق",
            "نتیجه پیگیری ضمانت نامه‌ها",
            
            };

            var headerRow = sheet.CreateRow(0);
            for (int col = 0; col < headers.Length; col++)
            {
                var cell = headerRow.CreateCell(col);
                cell.SetCellValue(headers[col]);
                cell.CellStyle = headerStyle;
            }

            // کوئری با Guid مشترک
            var data = _context.GuaranteeLetters.ToList();

            // پر کردن ردیف‌ها
            int rowNum = 1;
            foreach (var record in data)
            {
                var row = sheet.CreateRow(rowNum++);


                row.CreateCell(0).SetCellValue(record.DistributionCompanyName);
                row.CreateCell(1).SetCellValue(record.GuaranteeNumber);
                row.CreateCell(2).SetCellValue(record.GuaranteeType);
                row.CreateCell(3).SetCellValue(record.Amount);

                row.CreateCell(4).SetCellValue(record.IssueDate);
                row.CreateCell(5).SetCellValue(record.ExpirationDate);
                row.CreateCell(6).SetCellValue(record.BankName);
                row.CreateCell(7).SetCellValue(record.ContractNumber);

                row.CreateCell(8).SetCellValue(record.ContractStatus);
                row.CreateCell(9).SetCellValue(record.GuaranteeStatus); // نام محصول / شرح کالا
                row.CreateCell(10).SetCellValue(record.OverdueProgressPercentage);
                row.CreateCell(11).SetCellValue(record.FollowUpResult);              // قیمت توس فیوز
        
            }

            // تنظیم عرض ستون‌ها
            for (int col = 0; col < headers.Length; col++)
            {
                sheet.AutoSizeColumn(col);
            }

            // ------------------ Safe Output ------------------
            byte[] fileBytes;
            using (var ms = new MemoryStream())
            {
                workbook.Write(ms);
                fileBytes = ms.ToArray();
            }

            var fileName = $"ضمانتنامه_{DateTime.Now:yyyy-MM-dd_HH-mm}.xlsx";

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);


        }

        public async Task<JsonResult> SubmitGuarantee(int? GuaranteeLetterId, string DistributionCompanyName, string GuaranteeNumber, string GuaranteeType, string Amount, string IssueDate, string ExpirationDate, string BankName, string ContractNumber, string ContractStatus, string GuaranteeStatus, string? OverdueProgressPercentage, string? FollowUpResult)
        {
            ModelState.Remove("FollowUpResult");
            if (ModelState.IsValid)
            {
                var username = User.Identity.Name;
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);

                if (GuaranteeLetterId == 0 || GuaranteeLetterId == null)
                {
                    var guarantee = new GuaranteeLetter
                    {
                        DistributionCompanyName = DistributionCompanyName,
                        GuaranteeNumber = GuaranteeNumber,
                        GuaranteeType = GuaranteeType,
                        Amount = Amount,
                        IssueDate = IssueDate,
                        ExpirationDate = ExpirationDate,
                        IssueDateGregorian = PersianToGregorian(IssueDate),
                        ExpirationDateGregorian = PersianToGregorian(ExpirationDate),
                        BankName = BankName,
                        ContractNumber = ContractNumber,
                        ContractStatus = ContractStatus,
                        GuaranteeStatus = GuaranteeStatus,
                        OverdueProgressPercentage = OverdueProgressPercentage,
                        FollowUpResult = FollowUpResult,
                        RegDate = GregorianToPersian(DateTime.Now),
                        RegDateGregorian = DateTime.Now
                    };

                    _context.GuaranteeLetters.Add(guarantee);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    var guarantee = await _context.GuaranteeLetters.FirstOrDefaultAsync(i => i.GuaranteeLetterId == GuaranteeLetterId.Value);
                    if (guarantee != null)
                    {
                        guarantee.DistributionCompanyName = DistributionCompanyName;
                        guarantee.GuaranteeNumber = GuaranteeNumber;
                        guarantee.GuaranteeType = GuaranteeType;
                        guarantee.Amount = Amount;
                        guarantee.IssueDate = IssueDate;
                        guarantee.ExpirationDate = ExpirationDate;
                        guarantee.IssueDateGregorian = PersianToGregorian(IssueDate);
                        guarantee.ExpirationDateGregorian = PersianToGregorian(ExpirationDate);
                        guarantee.BankName = BankName;
                        guarantee.ContractNumber = ContractNumber;
                        guarantee.ContractStatus = ContractStatus;
                        guarantee.GuaranteeStatus = GuaranteeStatus;
                        guarantee.OverdueProgressPercentage = OverdueProgressPercentage;
                        guarantee.FollowUpResult = FollowUpResult;

                        await _context.SaveChangesAsync();
                    }
                }
            }

            return Json(new { status = "ok" }, new Newtonsoft.Json.JsonSerializerSettings());
        }
    }
}