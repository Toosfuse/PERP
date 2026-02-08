using ERP.Data;
using ERP.Models;
using ERP.ModelsEMP;
using ERP.Services;
using ERP.ViewModels;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Stimulsoft.Base;
using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ERP.Controllers
{
    [Authorize]
    public class PCBSerialNumberController : Controller
    {
        private readonly EMPContext _contextemp;
        private readonly ERPContext _context;
        private readonly IWebHostEnvironment _env;
        public readonly IServices _services;

        public PCBSerialNumberController(EMPContext contextemp, ERPContext context, IWebHostEnvironment env, IServices services)
        {
            _contextemp = contextemp;
            _context = context;
            _env = env;
           _services = services;
        }
        // GET: دریافت لیست سریال‌های اسکن‌شده برای یک بسته
        public async Task<JsonResult> GetScannedSerials(int packageId)
        {
            var scanned = await _contextemp.Meters
                .Where(m => m.PackagePk == packageId)
                .Select(m => m.Serial)
                .ToListAsync();

            return Json(scanned);
        }

        // بررسی فرمت سریال
        private bool CheckSerialFormat(string serial)
        {
            if (string.IsNullOrEmpty(serial) || serial.Length < 13)
                return false;

            if (!serial.StartsWith("18"))
                return false;

            string[] patterns =
            {
                @"^\d{2}-\d{1}-\d{2}-\d{8}$", // 18-6-10-45100000
                @"^\d{13}$",                 // 1861045100000
                @"^\d{14}$"                  // 18610451000000
            };

            return patterns.Any(p => System.Text.RegularExpressions.Regex.IsMatch(serial, p));
        }

        // GET: ScanBarcode/Index - نمایش لیست سفارشات
        public async Task<IActionResult> Index(int? cityId, string search)
        {
            ViewBag.Cities = await _contextemp.Cities
                .Select(c => new SelectListItem
                {
                    Value = c.CityId.ToString(),
                    Text = c.CityName ?? "نامشخص"
                })
                .OrderBy(c => c.Text)
                .ToListAsync();

            var ordersQuery = _contextemp.Orders
                .Include(o => o.City)
                .AsQueryable();

            if (cityId.HasValue && cityId > 0)
            {
                ordersQuery = ordersQuery.Where(o => o.CityId == cityId);
                ViewBag.SelectedCityId = cityId;
            }

            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim();
                ordersQuery = ordersQuery.Where(o =>
                    o.OrderNumber.Contains(search) ||
                    (o.OrderRegNumber != null && o.OrderRegNumber.Contains(search)) ||
                    (o.CustomerName != null && o.CustomerName.Contains(search)) ||
                    (o.City != null && o.City.CityName.Contains(search)));
            }

            var orders = await ordersQuery
                .OrderByDescending(o => o.OrderId)
                .Select(o => new OrderViewModel
                {
                    Id = o.OrderId,
                    OrderNumber = o.OrderNumber,
                    OrderRegNumber = o.OrderNumber,
                    RegDate = _services.iGregorianToPersian(o.RegDate),
                    CityId = o.CityId,
                    CityName = o.City != null ? o.City.CityName ?? "نامشخص" : "نامشخص",
                    StartSerial = o.StartSerial,
                    EndSerial = o.EndSerial,
                    CustomerName = o.CustomerName,
                    CodeProduct = o.CodeProduct
                })
                .ToListAsync();

          
           

            return View(orders);
        }
        private string ConvertToPersianDateFull(DateTime? date)
        {
            if (!date.HasValue)
                return string.Empty; // یا "-" یا "نامشخص"

            PersianCalendar pc = new PersianCalendar();
            int year = pc.GetYear(date.Value);
            int month = pc.GetMonth(date.Value);
            int day = pc.GetDayOfMonth(date.Value);

            return $"{year:0000}/{month:00}/{day:00}";
        }
        // GET: ScanBarcode/Create/5 - صفحه اسکن سریال برای یک سفارش
        public async Task<IActionResult> Create(int? id)
        {
            if (id == null) return NotFound();

            var order = await _contextemp.Orders
                .Include(o => o.City)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null) return NotFound();

            var package = await _contextemp.Packages
                .FirstOrDefaultAsync(p => p.OrderId == id);

            if (package == null) return NotFound();

            // تبدیل تاریخ ثبت به شمسی
            string regDatePersian = "";
            if (order.RegDate.HasValue)
            {
                var pc = new PersianCalendar();
                var gd = order.RegDate.Value;
                regDatePersian = $"{pc.GetYear(gd):0000}/{pc.GetMonth(gd):00}/{pc.GetDayOfMonth(gd):00}";
            }
            ViewData["RegDatePersian_Default"] = regDatePersian;

            // 1️⃣ گرفتن کنتورها
            var meters = await _contextemp.Meters
                .Where(m => m.PackagePk == package.PackagingId)
                .OrderByDescending(m => m.Id)
                .Select(m => new
                {
                    m.Serial,
                    m.DateInsert,
                    m.InsertUserPk,
                    m.PcbserialNumberDate,
                    m.PcbserialNumber   // ⭐⭐⭐ اضافه شود

                })
                .ToListAsync();

            // 2️⃣ گرفتن User ها
            var userIds = meters
                .Select(m => m.InsertUserPk)
                .Distinct()
                .ToList();

            var users = await _context.Users
                .Where(u => userIds.Contains(u.IntId))
                .Select(u => new
                {
                    u.IntId,
                    FullName = u.UserName + " " + u.LastName
                })
                .ToListAsync();

            // 3️⃣ Join در حافظه
            var displayMeters =
                (from m in meters
                 join u in users on m.InsertUserPk equals u.IntId into us
                 from u in us.DefaultIfEmpty()
                 select new PCBSerialNumber_MeterSerialInfo
                 {
                     Serial = m.Serial,
                     SeialBord = m.PcbserialNumber,              // ⭐⭐⭐ مهم

                     PCBSerialNumber_Date = m.PcbserialNumberDate,
                     FullName = u?.FullName ?? ""
                 }).ToList();

            var model = new PCBSerialNumberViewModel
            {
                PackageId = package.PackagingId,
                OrderID = order.OrderId,
                CodeProduct = order.CodeProduct,
                OrderNumber = order.OrderNumber,
                StartSerial = order.StartSerial ?? "",
                EndSerial = order.EndSerial ?? "",
                CityName = order.City?.CityName ?? "نامشخص",
                ScannedBordCount= displayMeters.Count(x =>
    !string.IsNullOrEmpty(x.SeialBord)),
                ScannedCount = displayMeters.Count,
                PCBSerial_MeterSerialInfo = displayMeters   // ✅ اصلاح شد
            };

            return View(model);
        }



        // خواندن داده برای Kendo Grid در صفحه Index
        public async Task<IActionResult> Orders_Read([DataSourceRequest] DataSourceRequest request, int? cityId, string search)
        {
            var ordersQuery = _contextemp.Orders.Include(o => o.City).AsQueryable();

            if (cityId.HasValue && cityId > 0)
                ordersQuery = ordersQuery.Where(o => o.CityId == cityId);

            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim();
                ordersQuery = ordersQuery.Where(o =>
                    o.OrderNumber.Contains(search) ||
                    (o.CustomerName != null && o.CustomerName.Contains(search)) ||
                    (o.City != null && o.City.CityName.Contains(search)));
            }

            var orders = await ordersQuery
                .OrderByDescending(o => o.OrderId)
                .Select(o => new OrderViewModel
                {
                    Id = o.OrderId,
                    OrderNumber = o.OrderNumber,
                    CodeProduct = o.CodeProduct,
                    CityName = o.City != null ? o.City.CityName ?? "نامشخص" : "نامشخص",
                    StartSerial = o.StartSerial ?? "-",
                    EndSerial = o.EndSerial ?? "-",
                    SerialCount = CalculateSerialCount(o.StartSerial, o.EndSerial),
                    RegDate = _services.iGregorianToPersian(o.RegDate),
                })
                .ToListAsync();

            return Json(orders.ToDataSourceResult(request));
        }


        public IActionResult ListScanbarcode_Read(
     [DataSourceRequest] DataSourceRequest request,
     int packageId)
        {
            // دیتابیس اول
            var meters = _contextemp.Meters
                .Where(m => m.PackagePk == packageId)
                .Select(m => new
                {
                    m.Serial,
                    m.PcbserialNumber,  
                    m.PcbserialNumberDate,
                    m.PcbserialNumberUserPk
                })
                .ToList();

            // کلیدهای یوزر موردنیاز
            var userIds = meters
                .Select(m => m.PcbserialNumberUserPk)
                .Distinct()
                .ToList();

            // دیتابیس دوم
            var users = _context.Users
                .Where(u => userIds.Contains(u.IntId))
                .Select(u => new
                {
                    u.IntId,
                    FullName = u.FirstName + " " + u.LastName
                })
                .ToList();

            // Join در حافظه
            var data =
                from m in meters
                join u in users on m.PcbserialNumberUserPk equals u.IntId into us
                from u in us.DefaultIfEmpty()
                select new PCBSerialNumber_MeterSerialInfo
                {
                    Serial = m.Serial,
                    PCBSerialNumber_Date =
        DateTime.TryParse(m.PcbserialNumberDate, out var dt)
        ? dt.ToPersianDigitalDateTimeString()
        : "",
                    SeialBord=m.PcbserialNumber,
                    FullName = u?.FullName ?? ""
                };

            return Json(data.ToDataSourceResult(request));
        }

        // POST: ثبت سریال اسکن‌شده
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> RegisterSerial(int OrderID, string serial,string serialBord)
        {
            try
            {
              

                serial = serial.Trim().Replace("\r", "").Replace("\n", "");

                // ۱. چک طول
                if (serial.Length != 14)
                    return Json(new { success = false, message = "شماره سریال باید دقیقاً 14 کاراکتر باشد." });

                // ۲. چک فرمت
                if (!CheckSerialFormat(serial))
                    return Json(new { success = false, message = "فرمت سریال صحیح نیست." });

                // ۳. دریافت سفارش/بسته
                var order = await _contextemp.Orders
                    .FirstOrDefaultAsync(p => p.OrderId == OrderID);
                var package = await _contextemp.Packages
                    .FirstOrDefaultAsync(p => p.OrderId == OrderID);

                if (order == null)
                    return Json(new { success = false, message = "سفارش یا بسته یافت نشد." });

                // ۴. چک بازه سریال
                if (!CheckSerialInPeriod(serial, package.StartSerial, package.EndSerial))
                    return Json(new { success = false, message = "سریال خارج از بازه مجاز بسته است." });


               
                long resultSerial = await IsAnySerialBordInRangeExistsMeter(serialBord);

                if (resultSerial > 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"شماره سریال برد با شماره سریال {resultSerial} وجود دارد"
                    });
                }
                

                var meter = _contextemp.Meters.SingleOrDefaultAsync(m => m.Serial == serial).Result;
                if (meter == null)
                    return Json(new { success = false, message = "کاربر محترم هنوز سریال اسکن تمپلیت نشده" });

                meter.PcbserialNumber = serialBord;
                meter.PcbserialNumberUserPk= GetCurrentUserId();
                meter.PcbserialNumberDate = DateTime.Now.ToString();
                _contextemp.Meters.Update(meter);
                await _contextemp.SaveChangesAsync();

                int newCount = await _contextemp.Meters.CountAsync(m => m.PackagePk == package.PackagingId);
                int scannedBordCount = await _contextemp.Meters.CountAsync(m =>
      m.PackagePk == package.PackagingId &&
      !string.IsNullOrEmpty(m.PcbserialNumber));

                var Meters = _contextemp.Meters.Where(x => x.Serial == serial).SingleOrDefault();
                var Users = _context.Users.SingleOrDefault(x => x.IntId == Meters.InsertUserPk);
                return Json(new
                {
                    success = true,
                    serial = serial,
                    dateInsert = Meters.PcbserialNumberDate,
                    userName = Users.FirstName + " " + Users.LastName,
                    newCount = newCount,
                    newBordCount = scannedBordCount
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطای سرور: " + ex.Message });
            }
        }

        // چک کردن بازه سریال
        private bool CheckSerialInPeriod(string serial, string startSerial, string endSerial)
        {
            if (string.IsNullOrEmpty(startSerial) || string.IsNullOrEmpty(endSerial))
                return false;

            if (!long.TryParse(serial.Replace("-", ""), out long serialNum))
                return false;

            if (!long.TryParse(startSerial.Replace("-", ""), out long start))
                return false;

            if (!long.TryParse(endSerial.Replace("-", ""), out long end))
                return false;

            return serialNum >= start && serialNum <= end;
        }

       
        // محاسبه تعداد سریال‌ها بر اساس بازه
        private static long CalculateSerialCount(string start, string end)
        {
            if (string.IsNullOrEmpty(start) || string.IsNullOrEmpty(end))
                return 0;

            if (long.TryParse(start.Replace("-", ""), out long s) &&
                long.TryParse(end.Replace("-", ""), out long e) && s <= e)
            {
                return e - s + 1;
            }

            return 0;
        }

        // تبدیل تاریخ شمسی به میلادی (در صورت نیاز در جاهای دیگر)
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


        private async Task<long> IsAnySerialBordInRangeExistsMeter(string serialBord)
        {
            if (!long.TryParse(serialBord, out long PCBSerialNumber))
            {
                throw new ArgumentException("شماره سریال برد باید عددی معتبر باشد.");
            }

            await using var cmd = _contextemp.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = "dbo.CheckSerialBordRangeExistsMeter";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@PCBSerialNumber", PCBSerialNumber));

            var paramResultSerial = new SqlParameter("@ResultSerial", SqlDbType.BigInt)
            {
                Direction = ParameterDirection.Output
            };
            cmd.Parameters.Add(paramResultSerial);

            if (cmd.Connection.State != ConnectionState.Open)
                await cmd.Connection.OpenAsync();

            await cmd.ExecuteNonQueryAsync();

            return paramResultSerial.Value == DBNull.Value
                ? 0
                : Convert.ToInt64(paramResultSerial.Value);
        }


        // تبدیل تاریخ ذخیره‌شده به نمایش شمسی
        private string ConvertToDisplayPersianDate(string dateInsert)
        {
            if (string.IsNullOrEmpty(dateInsert))
                return "-";

            // اگر تاریخ به صورت شمسی ذخیره شده باشد، بلافاصله برگردان
            if (dateInsert.Contains("/") && !dateInsert.StartsWith("1900"))
                return dateInsert;

            // اگر تاریخ به صورت میلادی باشد، تبدیل کنید
            if (DateTime.TryParse(dateInsert, out DateTime dt))
            {
                var pc = new PersianCalendar();
                return $"{pc.GetYear(dt):0000}/{pc.GetMonth(dt):00}/{pc.GetDayOfMonth(dt):00} {dt:HH:mm}";
            }

            return dateInsert;
        }

        // دریافت آی‌دی کاربر جاری
        private int GetCurrentUserId()
        {
            var username = User.Identity.Name;
            var intId = _context.Users.Where(p => p.UserName == username).Select(c => c.IntId).FirstOrDefault();
            return intId; // مقدار پیش‌فرض (در پروژه واقعی بهتر است خطا بدهد)
        }

        // تبدیل تاریخ شمسی‌ای که در DateInsert ذخیره‌شده است برای نمایش
        private string GetPersianDateDisplay(string dateInsertValue)
        {
            if (string.IsNullOrEmpty(dateInsertValue))
                return "-";

            // اگر به‌صورت شمسی ذخیره‌شده باشد، همان‌طور برگردان
            if (dateInsertValue.Contains("/"))
                return dateInsertValue;

            // اگر میلادی باشد، تبدیل کنید
            if (DateTime.TryParse(dateInsertValue, out DateTime dt))
            {
                var pc = new PersianCalendar();
                return $"{pc.GetYear(dt):0000}/{pc.GetMonth(dt):00}/{pc.GetDayOfMonth(dt):00} {dt:HH:mm:ss}";
            }

            return dateInsertValue;
        }
    }
}