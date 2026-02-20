using ERP.Data;
using ERP.Models;
using ERP.Services;
using ERP.ViewModels.SMT;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Globalization;

namespace ERP.Controllers
{
    [Authorize]
    public class SMTController : Controller
    {

        private readonly ERPContext _context;
        private readonly IServices _services;

        public SMTController(ERPContext context, IServices services)
        {
            _context = context;
           _services  = services;
        }

        public async Task<IActionResult> Index(string? searchData)
        {
            IQueryable<SMT> SMTQuery = _context.smt.AsQueryable();

            if (!string.IsNullOrEmpty(searchData))
            {
                searchData = searchData.Trim();
                SMTQuery = SMTQuery.Where(o =>
                    o.DataValue.Contains(searchData) ||

                    o.Id.ToString().Contains(searchData));
            }

            List<SMTViewModel> Smts = await SMTQuery
                .OrderByDescending(o => o.Id)
                .Select(o => new SMTViewModel
                {
                    Id = o.Id,
                    DataValue = o.DataValue,
                    DateCreate = o.DateCreate.ToPersianDateString(),
                })
                .ToListAsync();

            return View(Smts);
        }

        // در کنترلر اصلی (مثلاً SMT_ReadData)
        public async Task<ActionResult> SMT_Read([DataSourceRequest] DataSourceRequest request)
        {
            // 1. محاسبه تعداد برای هر روز
            Dictionary<DateTime, int> dailyCounts = await _context.smt
                .GroupBy(o => o.DateCreate.Date)
                .Select(g => new { DateKey = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.DateKey, x => x.Count);

            // 2. استخراج تاریخ‌های متمایز
            IQueryable<DateTime> distinctDatesQuery = _context.smt
                .Select(o => o.DateCreate)
                .Distinct()
                .AsQueryable();

            DataSourceResult pagedResult = await _context.smt
                .GroupBy(o => o.DateCreate.Date)
                .Select(g => new
                {
                    DateKey = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.DateKey) // مرتب‌سازی بر اساس کلید DateTime
                .Select(x => new SMTViewModel
                {
                    DateCreate = x.DateKey.ToString("yyyy/MM/dd", new CultureInfo("fa-IR")),
                    DailyScanCount = x.Count,
                })
                .ToDataSourceResultAsync(request);

            return Json(pagedResult);
        }

        public IActionResult Create(string? DateCreate)
        {
            SMTViewModel model = new();
            DateTime DateCreateMildadi = ConvertPersianDate(DateCreate);
            if (DateCreateMildadi.Date==DateTime.Now.Date)
                ViewBag.InputDisable = false;

            else 
            {
                DateTime dateMiladi = ConvertPersianDate(DateCreate);
                model.DateCreate = DateCreate;
                ViewBag.InputDisable = true;
            }
           

                return View(model); // Pass the initialized model
        }
        // بررسی تداخل سریال‌ها در بازه
       

        [HttpPost]
        public async Task<JsonResult> RegisterSMT(string DataValue)
        {
            try
            {

                bool Isdate = await IsAnyExitDataSMT(DataValue);
                using Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();

                if (Isdate == false)
                {
                    SMT SMT = new()
                    {
                        DataValue = DataValue,
                        IsDelete = false,
                        DateCreate = DateTime.Now // Ensure this is set if needed
                    };

                    _ = _context.smt.Add(SMT);
                    _ = await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return Json(new { success = true, message = "با موفقیت ذخیره شد" });
                }
                else
                {
                    // Return a response when data already exists
                    return Json(new { success = false, message = "داده قبلاً ثبت شده است" });
                }
            }
            catch (Exception ex)
            {
              
                return Json(new { success = false, message = "خطا در ذخیره سازی بانک اطلاعاتی", error = ex.Message });
            }
        }


        public async Task<IActionResult> SMT_ReadData([DataSourceRequest] DataSourceRequest request, string? DateCreate)
        {
            DataSourceResult Smts = null;
            bool inputDisabled; // متغیر جدید برای نگهداری وضعیت

            if (DateCreate == null)
            {
                // تاریخ امروز - فیلد باید غیرفعال باشد
                IQueryable<SMT> listsMTsQuery = _context.smt.Where(x => x.DateCreate.Date == DateTime.Now.Date).AsQueryable();
               

                Smts = await listsMTsQuery
                   .OrderByDescending(o => o.Id)
                   .Select(o => new SMTViewModel
                   {
                       Id = o.Id,
                       DataValue = o.DataValue,
                       DateCreate = o.DateCreate.ToString("yyyy/MM/dd", new CultureInfo("fa-IR")),
                   })
                   .ToDataSourceResultAsync(request);
          
            }
            else
            {
                // تاریخ خاصی انتخاب شده - فیلد باید فعال باشد
                DateTime DateCreateMildadi = ConvertPersianDate(DateCreate);
                IQueryable<SMT> listsMTsQuery = _context.smt.Where(x => x.DateCreate.Date == DateCreateMildadi.Date).AsQueryable();
                inputDisabled = false; // <-- تنظیم وضعیت

                Smts = await listsMTsQuery
                   .OrderByDescending(o => o.Id)
                   .Select(o => new SMTViewModel
                   {
                       Id = o.Id,
                       DataValue = o.DataValue,
                       DateCreate = o.DateCreate.ToString("yyyy/MM/dd", new CultureInfo("fa-IR")),
                   })
                   .ToDataSourceResultAsync(request);
            }

            // برگرداندن داده‌ها و وضعیت غیرفعال بودن در یک شیء واحد
            return Json(Smts);
        }
        private async Task<bool> IsAnyExitDataSMT(string DataValue)
        {
            await using var cmd = _context.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = "dbo.CheckIfDataExists";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@InputData", DataValue));


            var paramExists = new SqlParameter("@DataExists", SqlDbType.Bit) { Direction = ParameterDirection.Output };
            cmd.Parameters.Add(paramExists);


            if (cmd.Connection.State != ConnectionState.Open)
                await cmd.Connection.OpenAsync();

            await cmd.ExecuteNonQueryAsync();

            bool hasConflict = paramExists.Value != DBNull.Value && (bool)paramExists.Value;


            return hasConflict;
        }

        public IActionResult SecondaryList()
        {
            return View();
        }

        public IActionResult SecondaryListDetails(string date)
        {
            ViewBag.Date = date;
            return View();
        }
        public IActionResult Secondary()
        {
            return View();
        }
        public async Task<ActionResult> SMTSecondaryList_Read([DataSourceRequest] DataSourceRequest request)
        {
            var data = await _context.smtSecondary
                .Where(x => !x.IsDelete && !string.IsNullOrEmpty(x.SecondaryDatePersian))
                .GroupBy(x => x.SecondaryDatePersian)
                .OrderByDescending(g => g.Key)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            var result = data.Select(x => new SMTSecondaryListViewModel
            {
                SecondaryDatePersian = x.Date,
                count = x.Count
            }).ToList();

            var pagedResult = result.AsQueryable().ToDataSourceResult(request);
            return Json(pagedResult);
        }

        public async Task<ActionResult> SMTSecondaryByDate_Read([DataSourceRequest] DataSourceRequest request, string date)
        {
            if (string.IsNullOrEmpty(date))
                return Json(new DataSourceResult { Data = new List<SMTSecondaryViewModel>(), Total = 0 });

            var dateTime = ConvertPersianDate(date, useCurrentTime: false);

            var result = await _context.smtSecondary
                .Where(x => x.SecondaryDate.Date == dateTime.Date && !x.IsDelete)
                .Include(x => x.SMT)
                .OrderByDescending(x => x.Id)
                .Select(x => new SMTSecondaryViewModel
                {
                    Id = x.Id,
                    SMTId = x.SMTId,
                    DataValue = x.SMT.DataValue,
                    DateCreate = x.SMT.DateCreate.ToString("yyyy/MM/dd", new CultureInfo("fa-IR")),
                    SecondaryDate = x.SecondaryDate.ToString("yyyy/MM/dd", new CultureInfo("fa-IR")),
                    Username = x.Username,
                    CreatedAt = x.CreatedAt.ToString("yyyy/MM/dd HH:mm:ss", new CultureInfo("fa-IR"))
                })
                .ToDataSourceResultAsync(request);

            return Json(result);
        }

        [HttpPost]
        public async Task<JsonResult> SearchSMT(string barcode)
        {
            try
            {
                var smt = await _context.smt.FirstOrDefaultAsync(x => x.DataValue == barcode && !x.IsDelete);
                if (smt == null)
                    return Json(new { success = false, message = "بارکد یافت نشد" });

                return Json(new
                {
                    success = true,
                    id = smt.Id,
                    dataValue = smt.DataValue,
                    dateCreate = smt.DateCreate.ToString("yyyy/MM/dd HH:mm:ss", new CultureInfo("fa-IR"))
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطا در جستجو", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> RegisterSecondary(long smtId, string secondaryDate)
        {
            try
            {
                var username = User.Identity?.Name ?? "Unknown";
               
                var existing = await _context.smtSecondary.FirstOrDefaultAsync(x => x.SMTId == smtId && !x.IsDelete);
                
                if (existing != null)
                {
                    existing.SecondaryDate = DateTime.Now;
                    existing.SecondaryDatePersian = _services.iGregorianToPersian(DateTime.Now);
                    existing.Username = username;
                    existing.CreatedAt = DateTime.Now;
                    _context.smtSecondary.Update(existing);
                    await _context.SaveChangesAsync();

                    return Json(new
                    {
                        success = true,
                        id = existing.Id,
                        isUpdate = true,
                        secondaryDate = DateTime.Now,
                        username = existing.Username,
                        createdAt = existing.CreatedAt.ToString("yyyy/MM/dd HH:mm:ss", new CultureInfo("fa-IR"))
                    });
                }
                else
                {
                    var secondary = new SMTSecondary
                    {
                        SMTId = smtId,
                        SecondaryDate = DateTime.Now,
                        SecondaryDatePersian = _services.iGregorianToPersian(DateTime.Now),
                        Username = username,
                        CreatedAt = DateTime.Now
                    };

                    _context.smtSecondary.Add(secondary);
                    await _context.SaveChangesAsync();

                    return Json(new
                    {
                        success = true,
                        id = secondary.Id,
                        isUpdate = false,
                        secondaryDate = DateTime.Now,
                        username = secondary.Username,
                        createdAt = secondary.CreatedAt.ToString("yyyy/MM/dd HH:mm:ss", new CultureInfo("fa-IR"))
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطا در ثبت: " + ex.Message });
            }
        
      
              
            
          
        }

        public async Task<ActionResult> SMTSecondary_Read([DataSourceRequest] DataSourceRequest request, long? smtId)
        {
            if (!smtId.HasValue)
                return Json(new DataSourceResult { Data = new List<SMTSecondaryViewModel>(), Total = 0 });

            var result = await _context.smtSecondary
                .Where(x => x.SMTId == smtId && !x.IsDelete)
                .Include(x => x.SMT)
                .OrderByDescending(x => x.Id)
                .Select(x => new SMTSecondaryViewModel
                {
                    Id = x.Id,
                    SMTId = x.SMTId,
                    DataValue = x.SMT.DataValue,
                    DateCreate = x.SMT.DateCreate.ToString("yyyy/MM/dd HH:mm:ss", new CultureInfo("fa-IR")),
                    SecondaryDate = x.SecondaryDate.ToString("yyyy/MM/dd HH:mm:ss", new CultureInfo("fa-IR")),
                    Username = x.Username,
                    CreatedAt = x.CreatedAt.ToString("yyyy/MM/dd HH:mm:ss", new CultureInfo("fa-IR"))
                })
                .ToDataSourceResultAsync(request);

            return Json(result);
        }

        public DateTime ConvertPersianDate(string persianDateString, bool useCurrentTime = true, TimeSpan? specificTime = null)
        {
            if (string.IsNullOrWhiteSpace(persianDateString))
                return DateTime.Now; // اگر خالی باشد، تاریخ امروز برگردان

            try
            {

                var parts = persianDateString.Trim().Split('/');
                if (parts.Length != 3)
                    throw new FormatException("فرمت تاریخ شمسی اشتباه است. باید yyyy/MM/dd باشد.");

                int year = int.Parse(parts[0].Trim());
                int month = int.Parse(parts[1].Trim());
                int day = int.Parse(parts[2].Trim());

                PersianCalendar pc = new PersianCalendar();

                // ساعت را تنظیم کن
                int hour = 0, minute = 0, second = 0;
                if (useCurrentTime)
                {
                    DateTime now = DateTime.Now;
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
            catch (Exception ex)
            {
                return DateTime.Now;
            }
        }

    }
}