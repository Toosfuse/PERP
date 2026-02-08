using ERP.Data;
using ERP.Models;
using ERP.ModelsEMP;
using ERP.ViewModels.CoInqueryViewModel;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Stimulsoft.System.Windows.Forms;
using System.Globalization;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Stimulsoft.Report.StiOptions.Viewer.Windows.Exports;

namespace ERP.Controllers
{
    [Authorize(Roles = "SuperAdmin,Management,SalesMeter")]
    public class CompanyInqueryController : Controller
    {

        public CompanyInqueryController(ERPContext context)
        {
            _context = context;
        }
        private readonly ERPContext _context;
       

        public static string GregorianToPersian(DateTime gregorianDate)
        {
            var persianCalendar = new PersianCalendar();

            // تبدیل تاریخ میلادی به تاریخ شمسی
            int year = persianCalendar.GetYear(gregorianDate);
            int month = persianCalendar.GetMonth(gregorianDate);
            int day = persianCalendar.GetDayOfMonth(gregorianDate);

            // برگرداندن تاریخ به فرمت yyyy/mm/dd
            return $"{year}/{month:D2}/{day:D2}";
        }

        [HttpGet("tenders-excel")]
        public IActionResult ExportTendersToExcel()
        {
            // ایجاد Workbook
            var workbook = new XSSFWorkbook();

            // Sheet
            var sheet = workbook.CreateSheet("مناقصات");

            // ------------------ Header Style ------------------
            var headerStyle = workbook.CreateCellStyle();
            headerStyle.FillForegroundColor = IndexedColors.Aqua.Index;
            headerStyle.FillPattern = FillPattern.SolidForeground;

            var headerFont = workbook.CreateFont();
            headerFont.IsBold = true;
            headerStyle.SetFont(headerFont);
            headerStyle.Alignment=NPOI.SS.UserModel.HorizontalAlignment.Center;

            // ------------------ Headers ------------------
            string[] headers =
            {
      "شهر/استان",
            "شماره مناقصه",
            "قیمت کل",
            "تاریخ ارسال پاکت الف و نمونه",
            "تاریخ ارسال تمام پاکت‌ها",
            "نتیجه مناقصه",
            "برنده نهایی",
            "هزینه کارمزد",
            "وضعیت پیگیری",
            "شرح کالا/خدمت",
            "تعداد",
            "قیمت پایه توس فیوز",
            "امتیاز توس فیوز",
            "قیمت افزارآزما",
            "امتیاز افزارآزما",
            "قیمت بهینه سازان",
            "امتیاز بهینه سازان",
            "قیمت راد نیرو",
            "امتیاز راد نیرو",
            "قیمت سنجش نیرو",
            "امتیاز سنجش نیرو",
            "قیمت کنتورسازی",
            "امتیاز کنتورسازی",
            "قیمت سنجش افزار",
            "امتیاز سنجش افزار",
            "توضیحات"
    };

            var headerRow = sheet.CreateRow(0);
            for (int col = 0; col < headers.Length; col++)
            {
                var cell = headerRow.CreateCell(col);
                cell.SetCellValue(headers[col]);
                cell.CellStyle = headerStyle;
            }

            // ------------------ Data ------------------
            var data = (from tender in _context.CompanyTenders
                        join item in _context.CoTenderItems
                            on tender.Guid equals item.Guid into items
                        from item in items.DefaultIfEmpty()
                        select new { tender, item }).ToList();

            int rowNum = 1;
            foreach (var r in data)
            {
                var row = sheet.CreateRow(rowNum++);

               
                row.CreateCell(0).SetCellValue(r.tender.City ?? "");
                row.CreateCell(1).SetCellValue(r.tender.TenderNumber ?? "");
                row.CreateCell(2).SetCellValue(r.tender.TotalPrice);
                row.CreateCell(3).SetCellValue(r.tender.AlefDate ?? "");
                row.CreateCell(4).SetCellValue(r.tender.AllPocketDate ?? "");
                row.CreateCell(5).SetCellValue(r.tender.TenderResult ?? "");
                row.CreateCell(6).SetCellValue(r.tender.Winner ?? "");
                row.CreateCell(7).SetCellValue(r.tender.FeeCost ?? 0);
                row.CreateCell(8).SetCellValue(r.tender.Status ?? "");
                
                row.CreateCell(9).SetCellValue(_context.Products.SingleOrDefault(x => x.ProductID == r.item.ProductID).NameProduct) ;
                row.CreateCell(10).SetCellValue(r.item?.Quantity ?? 0);

                row.CreateCell(11).SetCellValue(r.item?.Price ?? 0);
                row.CreateCell(12).SetCellValue(r.item?.Score ?? 0);
                row.CreateCell(13).SetCellValue(r.item?.Price1 ?? 0);
                row.CreateCell(14).SetCellValue(r.item?.Score1 ?? 0);
                row.CreateCell(15).SetCellValue(r.item?.Price2 ?? 0);
                row.CreateCell(16).SetCellValue(r.item?.Score2 ?? 0);
                row.CreateCell(17).SetCellValue(r.item?.Price3 ?? 0);
                row.CreateCell(18).SetCellValue(r.item?.Score3 ?? 0);
                row.CreateCell(19).SetCellValue(r.item?.Price4 ?? 0);
                row.CreateCell(20).SetCellValue(r.item?.Score4 ?? 0);
                row.CreateCell(21).SetCellValue(r.item?.Price5 ?? 0);
                row.CreateCell(22).SetCellValue(r.item?.Score5 ?? 0);
                row.CreateCell(23).SetCellValue(r.item?.Price6 ?? 0);
                row.CreateCell(24).SetCellValue(r.item?.Score6 ?? 0);
                row.CreateCell(25).SetCellValue(r.item?.Description);
            }

            // ------------------ Auto Size ------------------
            for (int col = 0; col < headers.Length; col++)
                sheet.AutoSizeColumn(col);

            // ------------------ Safe Output ------------------
            byte[] fileBytes;
            using (var ms = new MemoryStream())
            {
                workbook.Write(ms);      // در NPOI قدیمی Stream بسته می‌شود
                fileBytes = ms.ToArray(); // ولی داده سالم اینجاست
            }

            var fileName = $"مناقصات_{DateTime.Now:yyyy-MM-dd_HH-mm}.xlsx";

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }


        [HttpGet("inqueries-excel")]
        public IActionResult ExportInqueriesToExcel()
        {
            var workbook = new XSSFWorkbook();
            
                var sheet = workbook.CreateSheet("استعلامات");

                // سبک هدر
                var headerStyle = workbook.CreateCellStyle();
                headerStyle.FillForegroundColor = IndexedColors.Aqua.Index;
                headerStyle.FillPattern = FillPattern.SolidForeground;
                headerStyle.Alignment =  NPOI.SS.UserModel.HorizontalAlignment.Center;
                headerStyle.WrapText = true;
       

               var headerFont = workbook.CreateFont();
                headerFont.IsBold = true;
                headerStyle.SetFont(headerFont);

                // هدرها
                string[] headers = new[]
                {
             "شرکت توزیع برق",
            "شماره نیاز",
            "قیمت کل",
            "هزینه کارمزد",
            "شماره پاسخ",
            "تاریخ نیاز",
            "نتیجه استعلام",
            "برنده نهایی",
            "وضعیت",
            "نام محصول",
            "تعداد",
            "قیمت توس فیوز",
            "قیمت افزارآزما",
            "قیمت بهینه سازان",
            "قیمت راد نیرو",
            "قیمت سنجش نیرو",
            "قیمت کنتورسازی",
            "قیمت سنجش افزار",
            "توضیحات"
            };

                var headerRow = sheet.CreateRow(0);
                for (int col = 0; col < headers.Length; col++)
                {
                    var cell = headerRow.CreateCell(col);
                    cell.SetCellValue(headers[col]);
                    cell.CellStyle = headerStyle;
                }

                // کوئری با Guid مشترک
                var data = (from inq in _context.CompanyInquerys
                            join item in _context.CoInqueryItems
                                on inq.Guid equals item.Guid into items
                            from item in items.DefaultIfEmpty()
                            select new { inq, item }).ToList();

                // پر کردن ردیف‌ها
                int rowNum = 1;
                foreach (var record in data)
                {
                    var row = sheet.CreateRow(rowNum++);


                row.CreateCell(0).SetCellValue(record.inq.City); 
                row.CreateCell(1).SetCellValue(record.inq.NeedNumber ?? "");
                row.CreateCell(2).SetCellValue(record.inq.TotalPrice);
                row.CreateCell(3).SetCellValue(record.inq.FeeCost ?? 0);
                row.CreateCell(4).SetCellValue(record.inq.ResponseNumber ?? "");
                row.CreateCell(5).SetCellValue(record.inq.NeedDate ?? "");
                row.CreateCell(6).SetCellValue(record.inq.InqueryResult ?? "");
                row.CreateCell(7).SetCellValue(record.inq.Winner ?? "");
                row.CreateCell(8).SetCellValue(record.inq.Status ?? "");
                row.CreateCell(9).SetCellValue(record.item?.Description ?? ""); // نام محصول / شرح کالا
                row.CreateCell(10).SetCellValue(record.item?.Quantity ?? 0);
                row.CreateCell(11).SetCellValue(record.item?.Price ?? 0);              // قیمت توس فیوز
                row.CreateCell(12).SetCellValue(record.item?.Price1 ?? 0);             // قیمت افزارآزما
                row.CreateCell(13).SetCellValue(record.item?.Price2 ?? 0);             // قیمت بهینه سازان
                row.CreateCell(14).SetCellValue(record.item?.Price3 ?? 0);             // قیمت راد نیرو
                row.CreateCell(15).SetCellValue(record.item?.Price4 ?? 0);             // قیمت سنجش نیرو
                row.CreateCell(16).SetCellValue(record.item?.Price5 ?? 0);             // قیمت کنتورسازی
                row.CreateCell(17).SetCellValue(record.item?.Price6 ?? 0);             // قیمت سنجش افزار
                row.CreateCell(18).SetCellValue(record.item?.Description);                                  
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

                var fileName = $"استعلامات_{DateTime.Now:yyyy-MM-dd_HH-mm}.xlsx";

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            
           
       }



        public IActionResult IndexTender(string? statusFilter = null,
                                         string? companyFilter = null,
                                         string? resultFilter = null)
        {
            var companys = new List<string>
    {
        "توس فیوز", "افزارآزما", "بهینه سازان",
        "راد نیرو", "سنجش نیرو", "سنجش افزار"
    };

            var statuses = new List<string>
    {
        "در حال پیگیری", "در حال ویرایش", "اتمام کار"
    };

            var results = new List<string>
    {
        "عدم حضور", "برنده", "بازنده", "تجدید شد"
    };

            var allTenders = _context.CompanyTenders.AsNoTracking();

            // ======== APPLY FILTERS (Server Side) ========
            var filtered = allTenders;

            if (!string.IsNullOrEmpty(statusFilter))
                filtered = filtered.Where(x => x.Status == statusFilter);

            if (!string.IsNullOrEmpty(companyFilter))
            {
                if (companyFilter == "نامشخص")
                    filtered = filtered.Where(x => x.Winner == null || x.Winner == "");
                else
                    filtered = filtered.Where(x => x.Winner == companyFilter);
            }

            if (!string.IsNullOrEmpty(resultFilter))
                filtered = filtered.Where(x => x.TenderResult == resultFilter);


            ViewBag.s1 = _context.CompanyTenders
           .Where(g => g.Status == "در حال پیگیری")
           .Count();
            ViewBag.s2 = _context.CompanyTenders
                .Where(g => g.Status == "در حال ویرایش")
                .Count();
            ViewBag.s3 = _context.CompanyTenders
                .Where(g => g.Status == "اتمام کار")
                .Count();

            ViewBag.s4 = _context.CompanyTenders
         .Where(g => g.TenderResult == "برنده")
         .Count();
            ViewBag.s5 = _context.CompanyTenders
                .Where(g => g.TenderResult == "بازنده")
                .Count();
            ViewBag.s6 = _context.CompanyTenders
            .Where(g => g.TenderResult == "تجدید شد")
            .Count();
            ViewBag.s7 = _context.CompanyTenders
                .Where(g => g.TenderResult == "عدم حضور")
                .Count();
          
            var model = new InqueryIndexViewModel
            {
                Companys = companys.Concat(new[] { "نامشخص" }).ToList(),
                Statuses = statuses,
                Results = results,

                AllCount = allTenders.Count(),

                CompanyCounts = new Dictionary<string, int>(),
                StatusCounts = new Dictionary<string, int>(),
                ResultCounts = new Dictionary<string, int>(),

                CurrentCompanyFilter = companyFilter,
                CurrentStatusFilter = statusFilter,
                CurrentResultFilter = resultFilter
            };

            // ======== Company Counts ========
            foreach (var company in companys)
            {
                model.CompanyCounts[company] = allTenders.Count(x =>
                    x.Winner != null && x.Winner == company);
            }

            // ⭐ فقط یک‌بار «نامشخص»
            model.CompanyCounts["نامشخص"] =
                allTenders.Count(x => x.Winner == null || x.Winner == ""  || x.Winner == "نامشخص");

            // ======== Status Counts ========
            foreach (var status in statuses)
            {
                model.StatusCounts[status] =
                    allTenders.Count(x => x.Status == status);
            }

            // ======== Result Counts ========
            foreach (var result in results)
            {
                model.ResultCounts[result] =
                    allTenders.Count(x => x.TenderResult == result);
            }

            return View(model);
        }
        public async Task<IActionResult> CoInquery(int? id)
        {
            bool isReadOnly = false;
            if (id != null && id < 0)
            {
                isReadOnly = true;
                id = Math.Abs(id.Value);
            }
            ViewBag.IsReadOnly = isReadOnly;
            CompanyInquery form = null;
            if (id != null)
            {
                form = await _context.CompanyInquerys.FindAsync(id);
                ViewBag.guid = form.Guid;
                if (form == null)
                {
                    return NotFound();
                }
            }
            if (form == null)
            {
                Guid guid = Guid.NewGuid();
                ViewBag.guid = guid;
                form = new CompanyInquery();
            }
            return View(form);
        }
        public async Task<IActionResult> CoTender(int? id)
        {
            bool isReadOnly = false;


            if (id != null && id < 0)
            {
                isReadOnly = true;
                id = Math.Abs(id.Value);
            }
            ViewBag.IsReadOnly = isReadOnly;


            CompanyTender form = null;
            if (id != null)
            {
                form = await _context.CompanyTenders.FindAsync(id);
                ViewBag.guid = form.Guid;
            }
           
    
            // جدید
            if (form == null)
            {
                Guid guid = Guid.NewGuid();
                ViewBag.guid = guid;
                form = new CompanyTender();
            }
            return View(form);
        }
        public IActionResult ListCoTender(
        [DataSourceRequest] DataSourceRequest request,
        string? SelectTab
      )
        {
            IQueryable<CompanyTender> query = _context.CompanyTenders;

         
            if (!string.IsNullOrEmpty(SelectTab))
            {
                if (SelectTab == "همه لیست")
                    query = query.AsTracking();
                else if (SelectTab == "همه شرکت ها")
                    query = query.Where(x => x.Winner == SelectTab);
                else if (SelectTab == "در حال پیگیری")
                    query = query.Where(x => x.Status == SelectTab);
                else if (SelectTab == "در حال ویرایش")
                    query = query.Where(x => x.Status == SelectTab);
                else if (SelectTab == "اتمام کار")
                    query = query.Where(x => x.Status == SelectTab);
                else if (SelectTab == "برنده")
                    query = query.Where(x => x.TenderResult == SelectTab);
                else if (SelectTab == "بازنده")
                    query = query.Where(x => x.TenderResult == SelectTab);
                else if (SelectTab == "عدم حضور")
                    query = query.Where(x => x.TenderResult == SelectTab);
                else if (SelectTab == "تجدید شد")
                    query = query.Where(x => x.TenderResult == SelectTab);
                else if (SelectTab == "نامشخص" || SelectTab==null)
                    query = query.Where(x => x.Winner == SelectTab || x.Winner=="" || x.Winner==null);
                else
                    query = query.Where(x => x.Winner == SelectTab);
            }

            

            return Json(query
                .OrderByDescending(x => x.CompanyTenderID)
                .ToDataSourceResult(request));
        }



        [HttpGet]
        public IActionResult GetTenderStatus(int CompanyTenderID)
        {
            var status = _context.CompanyTenders
                .Where(t => t.CompanyTenderID == CompanyTenderID)
                .Select(t => t.Status) // نام فیلد وضعیت پیگیری در جدول مناقصات
                .FirstOrDefault();

            return Json(new { status = status ?? "در حال پیگیری" });
        }

        [HttpGet]
        public async Task<IActionResult> GetInqueryStatus(int CompanyInqueryID)
        {
            var inquery = await _context.CompanyInquerys
                .Where(c => c.CompanyInqueryID == CompanyInqueryID)
                .Select(c => c.Status) // یا هر فیلدی که وضعیت پیگیری را نگه می‌دارد
                .FirstOrDefaultAsync();

            return Json(new { status = inquery ?? "در حال پیگیری" }); // اگر null بود، پیش‌فرض "در حال پیگیری"
        }
        public JsonResult GetProductMeter(string? text)
        {
            using (var mycontext = _context)
            {
                var products = mycontext.Products.Where(p => p.IsMeter == true).Select(c => new Models.Product
                {
                    ProductID = c.ProductID,
                    NameProductMarket = c.NameProductMarket,
                    CodeProduct = c.CodeProduct,
                    NoeProduct = c.NoeProduct,
                    NameProduct = c.NameProduct,
                    Price = c.Price,
                    QuantityINBox = c.QuantityINBox

                });
                if (!string.IsNullOrEmpty(text))
                {
                    products = products.Where(p => p.NameProduct.Contains(text));
                }
                return Json(products.ToList());

            }
        }
        [HttpPost]
        public async Task<JsonResult> UpdateTenderStatus(int CompanyTenderID, string TenderStatus)
        {
            try
            {
                var Tender = await _context.CompanyTenders
                    .FirstOrDefaultAsync(i => i.CompanyTenderID == CompanyTenderID);

                if (Tender == null)
                {
                    return Json(new { success = false, message = "مناقصه مورد نظر یافت نشد." });
                }

                // ذخیره وضعیت پیگیری در فیلد دلخواه (اگر فیلد اختصاصی دارید)
                // گزینه‌های ممکن:
                // 1. اگر فیلد PursuitStatus یا Status دارید:
                Tender.Status = TenderStatus; // یا inquery.PursuitStatus = PursuitStatus;

                // 2. اگر می‌خواهید در فیلد InqueryResult ذخیره شود (موقتاً):
                // inquery.InqueryResult = PursuitStatus;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "وضعیت پیگیری با موفقیت ثبت شد." });
            }
            catch (Exception ex)
            {
                // در حالت دیباگ می‌توانید پیام خطا را ببینید
                return Json(new { success = false, message = "خطا در ثبت وضعیت: " + ex.Message });
            }
        }
        public async Task<JsonResult> SubmitTender(int? CompanyTenderID, string? Winner, string City, string TenderNumber, double TotalPrice, string? AlefDate, string? AllPocketDate, string? TenderResult, double? FeeCost, string Guid)
        {
            if (ModelState.IsValid)
            {
                var username = User.Identity.Name;
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);

                if (CompanyTenderID == 0 || CompanyTenderID == null)
                {
                    // ثبت جدید
                    var tender = new CompanyTender
                    {
                        City = City,
                        TenderNumber = TenderNumber,
                        TotalPrice = TotalPrice,
                        AlefDate = AlefDate,
                        AllPocketDate = AllPocketDate,
                        TenderResult = TenderResult,
                        FeeCost = FeeCost,
                        Guid = Guid,
                        Winner = Winner,
                        CreateDate = GregorianToPersian(DateTime.Now),
                        Status = "درحال پیگیری"
                    };

                    _context.CompanyTenders.Add(tender);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // ویرایش
                    var tender = await _context.CompanyTenders.FirstOrDefaultAsync(t => t.CompanyTenderID == CompanyTenderID.Value);
                    if (tender != null)
                    {
                        tender.City = City;
                        tender.TenderNumber = TenderNumber;
                        tender.TotalPrice = TotalPrice;
                        tender.AlefDate = AlefDate;
                        tender.AllPocketDate = AllPocketDate;
                        tender.TenderResult = TenderResult;
                        tender.FeeCost = FeeCost;
                        tender.Guid = Guid;
                        tender.Winner = Winner;
                        tender.CreateDate= GregorianToPersian(DateTime.Now);
                        tender.Status = "درحال ویرایش";
                        await _context.SaveChangesAsync();
                    }
                }

                // تغییر وضعیت CoTenderItems
                var listcotenderitem = await _context.CoTenderItems.Where(p => p.Guid == Guid).ToListAsync();
                foreach (var p in listcotenderitem)
                {
                    p.IsTemp = false;
                }

                await _context.SaveChangesAsync();
            }

            return Json(new { status = "ok" }, new Newtonsoft.Json.JsonSerializerSettings());
        }
        public JsonResult InsertCotenderItem(int ProductID, double Quantity, string? Description, double Price, double Score, string Guid, double Price1, double Score1, double Price2, double Score2, double Price3, double Score3, double Price4, double Score4, double Price5, double Score5, double Price6, double Score6)
        {
            var product = _context.Products.Where(p => p.ProductID == ProductID).Single();
            ModelState.Remove("Description");
            if (ModelState.IsValid)
            {
                var Model = new CoTenderItem
                {
                    ProductID = ProductID,
                    Quantity = Quantity,
                    Description = Description,
                    Price = Price,
                    Score = Score,
                    Guid = Guid,
                    Price1 = Price1,
                    Score1 = Score1,
                    Price2 = Price2,
                    Score2 = Score2,
                    Price3 = Price3,
                    Score3 = Score3,
                    Price4 = Price4,
                    Score4 = Score4,
                    Price5 = Price5,
                    Score5 = Score5,
                    Price6 = Price6,
                    Score6 = Score6,
                    IsTemp = true,

                };
                _context.CoTenderItems.Add(Model);
                _context.SaveChangesAsync();
                string jsonString = JsonSerializer.Serialize(Model);
                return Json(new { data = Model, status = "ok" }, new Newtonsoft.Json.JsonSerializerSettings());
            }
            return Json(new { status = "Not OK" });
        }
        public JsonResult ListCotenderItem(string Guid)
        {
            var cotenderitemlist = _context.CoTenderItems.Where(p => p.Guid == Guid).ToList();
            var model = cotenderitemlist.Select(p => new TenderProductDto()
            {
                CoTenderItemID = p.CoTenderItemID,
                ProductID = p.ProductID,
                Quantity = p.Quantity,
                Description = p.Description,
                Price = p.Price,
                Score = p.Score,
                Guid = Guid,
                Price1 = p.Price1,
                Score1 = p.Score1,
                Price2 = p.Price2,
                Score2 = p.Score2,
                Price3 = p.Price3,
                Score3 = p.Score3,
                Price4 = p.Price4,
                Score4 = p.Score4,
                Price5 = p.Price5,
                Score5 = p.Score5,
                Price6 = p.Price6,
                Score6 = p.Score6,
                ProductName = _context.Products.Where(c => c.ProductID == p.ProductID).Select(p => p.NameProduct).FirstOrDefault(),

            }).ToList();
            double totalprice = 0;
            for (int i = 0; i < model.Count; i++)
            {
                totalprice = totalprice + (model[i].Price * model[i].Quantity);
            }
            return Json(new { data = model, totalprice, status = "ok" }, new Newtonsoft.Json.JsonSerializerSettings());
        }

        [HttpPost] // برای امنیت، اگر لازم باشه
        public async Task<JsonResult> DeleteCotenderItem(int id)
        {
            try
            {
                var cotenderitem = await _context.CoTenderItems.FindAsync(id);
                if (cotenderitem == null)
                {
                    return Json(new { status = "NotFound", message = "رکورد یافت نشد" });
                }

                _context.CoTenderItems.Remove(cotenderitem);
                await _context.SaveChangesAsync();

                return Json(new { status = "OK", message = "حذف با موفقیت انجام شد" });
            }
            catch (Exception ex)
            {
                return Json(new { status = "Error", message = ex.Message }); // به JS برگردون
            }
        }

        public JsonResult GetCoTenderItemById(int id)
        {
            var item = _context.CoTenderItems.FirstOrDefault(x => x.CoTenderItemID == id);
            if (item == null)
                return Json(new { status = "notfound" });

            var data = new TenderProductDto()
            {
                CoTenderItemID = item.CoTenderItemID,
                ProductID = item.ProductID,
                Quantity = item.Quantity,
                Description = item.Description,
                Price = item.Price,
                Score = item.Score,
                Guid = item.Guid,
                Price1 = item.Price1,
                Score1 = item.Score1,
                Price2 = item.Price2,
                Score2 = item.Score2,
                Price3 = item.Price3,
                Score3 = item.Score3,
                Price4 = item.Price4,
                Score4 = item.Score4,
                Price5 = item.Price5,
                Score5 = item.Score5,
                Price6 = item.Price6,
                Score6 = item.Score6,
                ProductName = _context.Products.Where(c => c.ProductID == item.ProductID).Select(p => p.NameProduct).FirstOrDefault()
            };
            return Json(data);
        }

        [HttpPost]
        public async Task<JsonResult> UpdateCoTenderItem(int CoTenderItemID, double Quantity, string? Description, double Price, double Score, double Price1, double Score1, double Price2, double Score2, double Price3, double Score3, double Price4, double Score4, double Price5, double Score5, double Price6, double Score6)
        {
            var item = await _context.CoTenderItems.FirstOrDefaultAsync(x => x.CoTenderItemID == CoTenderItemID);
            if (item == null)
                return Json(new { status = "notfound" });

            item.Quantity = Quantity;
            item.Price = Price;
            item.Score = Score;
            item.Description = Description;
            item.Price1 = Price1;
            item.Score1 = Score1;
            item.Price2 = Price2;
            item.Score2 = Score2;
            item.Price3 = Price3;
            item.Score3 = Score3;
            item.Price4 = Price4;
            item.Score4 = Score4;
            item.Price5 = Price5;
            item.Score5 = Score5;
            item.Price6 = Price6;
            item.Score6 = Score6;

            await _context.SaveChangesAsync();
            return Json(new { status = "ok" });
        }


        // استعلامات
        // ---------------------------------------------------------------------------------------------------
        private static string TrimOrNull(string value) => value?.Trim();

        public IActionResult IndexInquery(string statusFilter = null, string companyFilter = null, string resultFilter = null)
        {
            var companys = new List<string>
    {
        "نامشخص", "توس فیوز", "افزارآزما", "بهینه سازان",
        "راد نیرو", "سنجش نیرو", "سنجش افزار"
    };

            var statuses = new List<string>
    {
        "در حال پیگیری", "در حال ویرایش", "اتمام کار"
    };

            // اضافه کردن لیست نتایج استعلام
            var results = new List<string>
    {
        "برنده", "بازنده", "باطل شد"
    };

            var allInquerys = _context.CompanyInquerys.AsNoTracking();

            // ======== APPLY FILTERS (Server Side) ========
            var filtered = allInquerys;
            IQueryable<CompanyInquery> query = _context.CompanyInquerys;

            if (!string.IsNullOrEmpty(statusFilter))
            {
                query = query.Where(i => i.Status != null &&
                                        i.Status.Trim().Equals(statusFilter.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(companyFilter))
            {
                query = query.Where(i => (i.Winner ?? "").Trim()
                                        .Equals(companyFilter.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            // اضافه کردن فیلتر نتیجه استعلام
            if (!string.IsNullOrEmpty(resultFilter))
            {
                query = query.Where(i => i.InqueryResult != null &&
                                        i.InqueryResult.Trim().Equals(resultFilter.Trim(), StringComparison.OrdinalIgnoreCase));
            }

           

            ViewBag.s1 = _context.CompanyInquerys
       .Where(g => g.Status == "در حال پیگیری")
       .Count();
            ViewBag.s2 = _context.CompanyInquerys
                .Where(g => g.Status == "در حال ویرایش")
                .Count();
            ViewBag.s3 = _context.CompanyInquerys
                .Where(g => g.Status == "اتمام کار")
                .Count();

            ViewBag.s4 = _context.CompanyInquerys
         .Where(g => g.InqueryResult == "برنده")
         .Count();
            ViewBag.s5 = _context.CompanyInquerys
                .Where(g => g.InqueryResult == "بازنده")
                .Count();
            ViewBag.s6 = _context.CompanyInquerys
            .Where(g => g.InqueryResult == "باطل شد")
            .Count();
           


            var model = new InqueryIndexViewModel
            {
                Companys = companys.Concat(new[] { "نامشخص" }).ToList(),
                Statuses = statuses,
                Results = results,

                AllCount = allInquerys.Count(),

                CompanyCounts = new Dictionary<string, int>(),
                StatusCounts = new Dictionary<string, int>(),
                ResultCounts = new Dictionary<string, int>(),

                CurrentCompanyFilter = companyFilter,
                CurrentStatusFilter = statusFilter,
                CurrentResultFilter = resultFilter
            };


            // ======== Company Counts ========
            foreach (var company in companys)
            {
                model.CompanyCounts[company] = allInquerys.Count(x =>
                    x.Winner != null && x.Winner == company);
            }

            // ⭐ فقط یک‌بار «نامشخص»
            model.CompanyCounts["نامشخص"] =
                allInquerys.Count(x => x.Winner == null || x.Winner == "" || x.Winner == "نامشخص");

            // ======== Status Counts ========
            foreach (var status in statuses)
            {
                model.StatusCounts[status] =
                    allInquerys.Count(x => x.Status == status);
            }

          


            return View(model);
        }
       

        public IActionResult ListCoInquery(
     [DataSourceRequest] DataSourceRequest request,
     string? SelectTab)
        {
            IQueryable<CompanyInquery> query = _context.CompanyInquerys;


            if (!string.IsNullOrEmpty(SelectTab))
            {

                if (SelectTab == "همه لیست")
                    query = query.AsTracking();
                else if (SelectTab == "همه شرکت ها")
                    query = query.Where(x => x.Winner == SelectTab);
                else if (SelectTab == "در حال پیگیری")
                    query = query.Where(x => x.Status == SelectTab);
                else if (SelectTab == "در حال ویرایش")
                    query = query.Where(x => x.Status == SelectTab);
                else if (SelectTab == "اتمام کار")
                    query = query.Where(x => x.Status == SelectTab);
                else if (SelectTab == "برنده")
                    query = query.Where(x => x.InqueryResult == SelectTab);
                else if (SelectTab == "بازنده")
                    query = query.Where(x => x.InqueryResult == SelectTab);
                else if (SelectTab == "باطل شد")
                    query = query.Where(x => x.InqueryResult == SelectTab);
                else if (SelectTab == "نامشخص")
                    query = query.Where(x => x.Winner == SelectTab || x.Winner == "" || x.Winner == null);
                else
                    query = query.Where(x => x.Winner == SelectTab);

            }



            return Json(query
                .OrderByDescending(x => x.CompanyInqueryID)
                .ToDataSourceResult(request));
        }


        [HttpPost]
        public async Task<JsonResult> UpdateInqueryStatus(int CompanyInqueryID, string InqueryStatus)
        {
            try
            {
                var inquery = await _context.CompanyInquerys
                    .FirstOrDefaultAsync(i => i.CompanyInqueryID == CompanyInqueryID);

                if (inquery == null)
                {
                    return Json(new { success = false, message = "استعلام مورد نظر یافت نشد." });
                }

                // ذخیره وضعیت پیگیری در فیلد دلخواه (اگر فیلد اختصاصی دارید)
                // گزینه‌های ممکن:
                // 1. اگر فیلد PursuitStatus یا Status دارید:
                inquery.Status = InqueryStatus; // یا inquery.PursuitStatus = PursuitStatus;

                // 2. اگر می‌خواهید در فیلد InqueryResult ذخیره شود (موقتاً):
                // inquery.InqueryResult = PursuitStatus;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "وضعیت پیگیری با موفقیت ثبت شد." });
            }
            catch (Exception ex)
            {
                // در حالت دیباگ می‌توانید پیام خطا را ببینید
                return Json(new { success = false, message = "خطا در ثبت وضعیت: " + ex.Message });
            }
        }

        public async Task<JsonResult> SubmitInquery(int? CompanyInqueryID, string City, string? Winner, string NeedNumber, double TotalPrice, string? ResponseNumber, string? NeedDate, string? InqueryResult, double? FeeCost, string Guid)
        {
            if (ModelState.IsValid)
            {
                var username = User.Identity.Name;
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);

                if (CompanyInqueryID == 0 || CompanyInqueryID == null)
                {
                    // ثبت جدید
                    var inquery = new CompanyInquery
                    {
                        City = City,
                        NeedNumber = NeedNumber,
                        TotalPrice = TotalPrice,
                        ResponseNumber = ResponseNumber,
                        RegDate = GregorianToPersian(DateTime.Now),
                        NeedDate = NeedDate,
                        InqueryResult = InqueryResult,
                        FeeCost = FeeCost,
                        Guid = Guid,
                        Winner = Winner,
                        CreateDate= GregorianToPersian(DateTime.Now),
                        Status = "درحال پیگیری",
                    };

                    _context.CompanyInquerys.Add(inquery);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // ویرایش
                    var inquery = await _context.CompanyInquerys.FirstOrDefaultAsync(i => i.CompanyInqueryID == CompanyInqueryID.Value);
                    if (inquery != null)
                    {
                        inquery.City = City;
                        inquery.NeedNumber = NeedNumber;
                        inquery.TotalPrice = TotalPrice;
                        inquery.ResponseNumber = ResponseNumber;
                        inquery.NeedDate = NeedDate;
                        inquery.InqueryResult = InqueryResult;
                        inquery.FeeCost = FeeCost;
                        inquery.Guid = Guid;
                        inquery.Winner = Winner;
                        inquery.CreateDate = GregorianToPersian(DateTime.Now);
                        inquery.Status = "درحال ویرایش";
                        await _context.SaveChangesAsync();
                    }
                }

                // تغییر وضعیت CoInqueryItems
                var listcoinqueryitem = await _context.CoInqueryItems.Where(p => p.Guid == Guid).ToListAsync();
                foreach (var p in listcoinqueryitem)
                {
                    p.IsTemp = false;
                }

                await _context.SaveChangesAsync();
            }

            return Json(new { status = "ok" }, new Newtonsoft.Json.JsonSerializerSettings());
        }

        public JsonResult InsertCoinqueryItem(int ProductID, double Quantity, string? Description, double Price, double Score, string Guid, double Price1, double Score1, double Price2, double Score2, double Price3, double Score3, double Price4, double Score4, double Price5, double Score5, double Price6, double Score6)
        {
            var product = _context.Products.Where(p => p.ProductID == ProductID).Single();
            ModelState.Remove("Description");
            if (ModelState.IsValid)
            {
                var Model = new CoInqueryItem
                {
                    ProductID = ProductID,
                    Quantity = Quantity,
                    Description = Description,
                    Price = Price,
                    Guid = Guid,
                    Price1 = Price1,
                    Price2 = Price2,
                    Price3 = Price3,
                    Price4 = Price4,
                    Price5 = Price5,
                    Price6 = Price6,
                    IsTemp = true,

                };
                _context.CoInqueryItems.Add(Model);
                _context.SaveChangesAsync();
                string jsonString = JsonSerializer.Serialize(Model);
                return Json(new { data = Model, status = "ok" }, new Newtonsoft.Json.JsonSerializerSettings());
            }
            return Json(new { status = "Not OK" });
        }
        public JsonResult ListCoinqueryItem(string Guid)
        {
            var coinqueryitemlist = _context.CoInqueryItems.Where(p => p.Guid == Guid).ToList();
            var model = coinqueryitemlist.Select(p => new InqueryProductDto()
            {
                CoInqueryItemID = p.CoInqueryItemID,
                ProductID = p.ProductID,
                Quantity = p.Quantity,
                Description = p.Description,
                Price = p.Price,
                Guid = Guid,
                Price1 = p.Price1,
                Price2 = p.Price2,
                Price3 = p.Price3,
                Price4 = p.Price4,
                Price5 = p.Price5,
                Price6 = p.Price6,
                ProductName = _context.Products.Where(c => c.ProductID == p.ProductID).Select(p => p.NameProduct).FirstOrDefault(),

            }).ToList();
            double totalprice = 0;
            for (int i = 0; i < model.Count; i++)
            {
                totalprice = totalprice + (model[i].Price * model[i].Quantity);
            }
            return Json(new { data = model, totalprice, status = "ok" }, new Newtonsoft.Json.JsonSerializerSettings());
        }

        [HttpPost] // برای امنیت، اگر لازم باشه
        public async Task<JsonResult> DeleteCoinqueryItem(int id)
        {
            try
            {
                var coinqueryitem = await _context.CoInqueryItems.FindAsync(id);
                if (coinqueryitem == null)
                {
                    return Json(new { status = "NotFound", message = "رکورد یافت نشد" });
                }

                _context.CoInqueryItems.Remove(coinqueryitem);
                await _context.SaveChangesAsync();

                return Json(new { status = "OK", message = "حذف با موفقیت انجام شد" });
            }
            catch (Exception ex)
            {
                return Json(new { status = "Error", message = ex.Message }); // به JS برگردون
            }
        }

        public JsonResult GetCoinqueryItemById(int id)
        {
            var item = _context.CoInqueryItems.FirstOrDefault(x => x.CoInqueryItemID == id);
            if (item == null)
                return Json(new { status = "notfound" });

            var data = new InqueryProductDto()
            {
                CoInqueryItemID = item.CoInqueryItemID,
                ProductID = item.ProductID,
                Quantity = item.Quantity,
                Description = item.Description,
                Price = item.Price,
                Guid = item.Guid,
                Price1 = item.Price1,
                Price2 = item.Price2,
                Price3 = item.Price3,
                Price4 = item.Price4,
                Price5 = item.Price5,
                Price6 = item.Price6,
                ProductName = _context.Products
                                 .Where(c => c.ProductID == item.ProductID)
                                 .Select(p => p.NameProduct)
                                 .FirstOrDefault()
            };
            return Json(data);
        }

        [HttpPost]
        public async Task<JsonResult> UpdateCoinqueryItem(int CoInqueryItemID, double quantity, double price, string? description, double? price1, double? price2, double? price3, double? price4, double? price5, double? price6)
        {
            var item = await _context.CoInqueryItems.FirstOrDefaultAsync(x => x.CoInqueryItemID == CoInqueryItemID);
            if (item == null)
                return Json(new { status = "notfound" });

            item.Quantity = quantity;
            item.Price = price;
            item.Description = description;
            item.Price1 = price1;
            item.Price2 = price2;
            item.Price3 = price3;
            item.Price4 = price4;
            item.Price5 = price5;
            item.Price6 = price6;

            await _context.SaveChangesAsync();
            return Json(new { status = "ok" });
        }


    }
}
