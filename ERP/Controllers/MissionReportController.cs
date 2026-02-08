using ERP.Data;
using ERP.Models;
using ERP.ViewModels.GuestEntry;
using ERP.ViewModels.MissionReport;
using ERP.ViewModels.ViewProcess;
using ERP.ViewModels.WorkReport;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace ERP.Controllers
{
    [Authorize]
    public class MissionReportController : Controller
    {
        private readonly ERPContext _context;

        public MissionReportController(ERPContext context)
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

            // برگرداندن تاریخ به فرمت yyyy/mm/dd
            return $"{year}/{month:D2}/{day:D2}";
        }

        public async Task<IActionResult> Index()
        {
            var username = User.Identity.Name;
            var userid = await _context.Users.Where(p => p.UserName == username).Select(j => j.Id).SingleOrDefaultAsync();

            var workflowStages = new List<string>
                         {
                    "start0",
                    "TayidModirVahed1",
                    "TakmilGozaresh2",
                    "TayidModirVahed3",
                    "BarrasiEdari4",
                    "Location5",
                    "Kargozini6",
                    "End7"
                       };
            // چک کردن نقش‌های SuperAdmin یا Management
            bool isSuperAdminOrManagement = User.IsInRole("SuperAdmin") || User.IsInRole("Management");
            IQueryable<int> accessibleMissionReportIds;

            if (isSuperAdminOrManagement)
            {
                // اگر کاربر SuperAdmin یا Management باشد، تمام MissionReports را بدون فیلتر دسترسی برگردان
                accessibleMissionReportIds = _context.MissionReports.Select(g => g.MissionReportID).Distinct();
            }
            //else
            //{
            //    // فیلتر دسترسی برای کاربران غیر SuperAdmin و غیر Management
            //    accessibleMissionReportIds = _context.MissionReports
            //        .Where(g => _context.WorkflowAccesses.Any(wa => wa.UserID == userid && wa.ProcessName == "MissionReportWorkflow") ||
            //                    _context.WorkflowInstances.Any(wi => wi.WorkFlowID == g.MissionReportID && wi.ProcessName == "MissionReportWorkflow" && wi.AssignedUserID == userid))
            //        .Select(g => g.MissionReportID)
            //        .Distinct();
            //}
            else
            {
                // گرفتن تمام مراحل که کاربر در آن‌ها نقش Observer دارد
                var observerStages = await _context.WorkflowAccesses
                    .Where(a => a.UserID == userid && a.ProcessName == "MissionReportWorkflow" && a.Role == "Observer")
                    .Select(a => a.Section)
                    .ToListAsync();

                // محاسبه تمام مراحلی که کاربر به آن‌ها دسترسی دارد (مراحل Observer و مراحل بعدی)
                var accessibleStages = new List<string>();
                foreach (var observerStage in observerStages)
                {
                    int observerStageIndex = workflowStages.IndexOf(observerStage);
                    if (observerStageIndex >= 0)
                    {
                        // اضافه کردن تمام مراحل از observerStage به بعد
                        accessibleStages.AddRange(workflowStages.Skip(observerStageIndex));
                    }
                }
                accessibleStages = accessibleStages.Distinct().ToList();

                // فیلتر دسترسی برای کاربران غیر SuperAdmin و غیر Management
                accessibleMissionReportIds = _context.MissionReports
                    .Where(g =>
                        // شرط برای Observer: فقط GuestEntryهایی که در accessibleStages هستند
                        accessibleStages.Contains(g.State) ||
                        // شرط برای AssignedUser
                        _context.WorkflowInstances.Any(wi => wi.WorkFlowID == g.MissionReportID && wi.ProcessName == "MissionReportWorkflow" && wi.AssignedUserID == userid))
                    .Select(g => g.MissionReportID)
                    .Distinct();
            }
           

            ViewBag.all = _context.MissionReports
                .Where(g => accessibleMissionReportIds.Contains(g.MissionReportID))
                .Count();

            ViewBag.s1 = _context.MissionReports
                .Where(g => g.State == "TayidModirVahed1" && accessibleMissionReportIds.Contains(g.MissionReportID))
                .Count();
            ViewBag.s2 = _context.MissionReports
                .Where(g => g.State == "TakmilGozaresh2" && accessibleMissionReportIds.Contains(g.MissionReportID))
                .Count();
            ViewBag.s3 = _context.MissionReports
                .Where(g => g.State == "TayidModirVahed3" && accessibleMissionReportIds.Contains(g.MissionReportID))
                .Count();
            ViewBag.s4 = _context.MissionReports
                .Where(g => g.State == "BarrasiEdari4" && accessibleMissionReportIds.Contains(g.MissionReportID))
                .Count();
            ViewBag.s5 = _context.MissionReports
              .Where(g => g.State == "Location5" && accessibleMissionReportIds.Contains(g.MissionReportID))
              .Count();

            ViewBag.s6 = _context.MissionReports
              .Where(g => g.State == "Kargozini6" && accessibleMissionReportIds.Contains(g.MissionReportID))
              .Count();

            ViewBag.s7 = _context.MissionReports
              .Where(g => g.State == "End7" && accessibleMissionReportIds.Contains(g.MissionReportID))
              .Count();


            return View();
        }

        public async Task<DataSourceResult> Data_MissionReports([DataSourceRequest] DataSourceRequest request, string SelectTab)
        {
            var username = User.Identity.Name;
            var userid = _context.Users.Where(p => p.UserName == username).Select(j => j.Id).FirstOrDefault();
            // چک کردن نقش‌های SuperAdmin یا Management
            bool isSuperAdminOrManagement = User.IsInRole("SuperAdmin") || User.IsInRole("Management");

            var workflowStages = new List<string>
                         {
                    "start0",
                    "TayidModirVahed1",
                    "TakmilGozaresh2",
                    "TayidModirVahed3",
                    "BarrasiEdari4",
                    "Location5",
                    "Kargozini6",
                    "End7"
                       };
            IQueryable<IndexMissionReportViewModel> model1;
            if (isSuperAdminOrManagement)
            {
                // اگر کاربر SuperAdmin یا Management باشد، تمام MissionReports را بدون فیلتر دسترسی برگردان
                if (SelectTab == "همه لیست")
                {
                    model1 = _context.MissionReports
                        .Select(u => new IndexMissionReportViewModel
                        {
                            MissionReportID = u.MissionReportID,
                            Req_name = _context.Users.Where(p => p.Id == u.Req_UserID).Select(p => p.FirstName + " " + p.LastName).FirstOrDefault(),
                            State = _context.WorkflowSections.Where(p => p.SectionCode == u.State && p.ProcessName == "MissionReportWorkflow").Select(p => p.SectionName).FirstOrDefault(),
                            Vorud_persiandate = u.S_persiandate,
                            Guid = u.Guid,
                            SourceTable = 0 // همه لیست‌ها بدون فیلتر دسترسی
                        }).OrderByDescending(u => u.MissionReportID);
                }
                else
                {
                    var selectedState = SelectTab;
                    model1 = _context.MissionReports
                        .Where(g => g.State == selectedState)
                        .Select(u => new IndexMissionReportViewModel
                        {
                            MissionReportID = u.MissionReportID,
                            Req_name = _context.Users.Where(p => p.Id == u.Req_UserID).Select(p => p.FirstName + " " + p.LastName).FirstOrDefault(),
                            State = _context.WorkflowSections.Where(p => p.SectionCode == u.State && p.ProcessName == "MissionReportWorkflow").Select(p => p.SectionName).FirstOrDefault(),
                            Vorud_persiandate = u.S_persiandate,
                            Guid = u.Guid,
                            SourceTable = 1 // فیلتر بر اساس State
                        }).OrderByDescending(u => u.MissionReportID);
                }
            }
            else
            {
                // گرفتن تمام مراحل که کاربر در آن‌ها نقش Observer دارد
                var observerStages = await _context.WorkflowAccesses
                    .Where(a => a.UserID == userid && a.ProcessName == "MissionReportWorkflow" && a.Role == "Observer")
                    .Select(a => a.Section)
                    .ToListAsync();

                // محاسبه تمام مراحلی که کاربر به آن‌ها دسترسی دارد (مراحل Observer و مراحل بعدی)
                var accessibleStages = new List<string>();
                foreach (var observerStage in observerStages)
                {
                    int observerStageIndex = workflowStages.IndexOf(observerStage);
                    if (observerStageIndex >= 0)
                    {
                        // اضافه کردن تمام مراحل از observerStage به بعد
                        accessibleStages.AddRange(workflowStages.Skip(observerStageIndex));
                    }
                }
                accessibleStages = accessibleStages.Distinct().ToList();

                // فیلتر دسترسی برای کاربران غیر SuperAdmin و غیر Management
                if (SelectTab == "همه لیست")
                {
                    model1 = _context.MissionReports
                        .Where(g =>
                            // شرط برای Observer: فقط GuestEntryهایی که در accessibleStages هستند
                            accessibleStages.Contains(g.State) ||
                            // شرط برای AssignedUser
                            _context.WorkflowInstances.Any(wi => wi.WorkFlowID == g.MissionReportID && wi.ProcessName == "MissionReportWorkflow" && wi.AssignedUserID == userid))
                        .Select(u => new IndexMissionReportViewModel
                        {
                            MissionReportID = u.MissionReportID,
                            Req_name = _context.Users.Where(p => p.Id == u.Req_UserID).Select(p => p.FirstName + " " + p.LastName).FirstOrDefault(),
                            State = _context.WorkflowSections.Where(p => p.SectionCode == u.State && p.ProcessName == "MissionReportWorkflow").Select(p => p.SectionName).FirstOrDefault(),
                            Vorud_persiandate = u.S_persiandate,
                            Guid = u.Guid,
                            SourceTable = 0 // همه لیست‌ها با فیلتر دسترسی
                        }).OrderByDescending(u => u.MissionReportID);
                }
                else
                {
                    var selectedState = SelectTab;
                    model1 = _context.MissionReports
                        .Where(g => g.State == selectedState && (
                            // شرط برای Observer: فقط اگر selectedState در accessibleStages باشد
                            accessibleStages.Contains(g.State) ||
                            // شرط برای AssignedUser
                            _context.WorkflowInstances.Any(wi => wi.WorkFlowID == g.MissionReportID && wi.ProcessName == "MissionReportWorkflow" && wi.AssignedUserID == userid)))
                        .Select(u => new IndexMissionReportViewModel
                        {
                            MissionReportID = u.MissionReportID,
                            Req_name = _context.Users.Where(p => p.Id == u.Req_UserID).Select(p => p.FirstName + " " + p.LastName).FirstOrDefault(),
                            State = _context.WorkflowSections.Where(p => p.SectionCode == u.State && p.ProcessName == "MissionReportWorkflow").Select(p => p.SectionName).FirstOrDefault(),
                            Vorud_persiandate = u.S_persiandate,
                            Guid = u.Guid,
                            SourceTable = 1 // فیلتر بر اساس State
                        }).OrderByDescending(u => u.MissionReportID);
                }
            }

            return await model1.ToDataSourceResultAsync(request);
        }

        public async Task<IActionResult> MissionReportView(int? id)
        {
            MissionReport model;
            var username = User.Identity.Name;
            var userid = await _context.Users.Where(p => p.UserName == username).Select(j => j.Id).SingleOrDefaultAsync();
            var reqName = await _context.Users.Where(p => p.Id == userid).Select(p => p.FirstName + " " + p.LastName).FirstOrDefaultAsync();

            var workflowStages = new List<string>
                         {
                    "start0",
                    "TayidModirVahed1",
                    "TakmilGozaresh2",
                    "TayidModirVahed3",
                    "BarrasiEdari4",
                    "Location5",
                    "Kargozini6",
                    "End7"
                       };

            if (id.HasValue && id > 0)
            {
                model = await _context.MissionReports.FirstOrDefaultAsync(g => g.MissionReportID == id);
                if (model == null)
                {
                    return NotFound();
                }
                var reqName2 = await _context.Users.Where(p => p.Id == model.Req_UserID).Select(p => p.FirstName + " " + p.LastName).FirstOrDefaultAsync();
                model.Req_UserID = reqName2;
                ViewBag.Guid = model.Guid;

                // پیدا کردن دسترسی کاربر
                var currentUserId = userid;
                var currentState = model.State ?? "start0";

                // چک نقش سوپر ادمین یا مدیریت
                bool isSuperAdminOrManagement = User.IsInRole("SuperAdmin") || User.IsInRole("Management");

                // گرفتن تاریخچه برای بخش‌های تکمیل‌شده
                var workflowHistory = await _context.WorkflowInstances
                    .Where(w => w.WorkFlowID == id.Value && w.IsCompleted && w.ProcessName == "MissionReportWorkflow")
                    .OrderBy(w => w.AssignedDate)
                    .ToListAsync();

                var completedSections = workflowHistory.Select(w => w.Section).ToList();
                ViewBag.CompletedSections = completedSections; // برای دیباگ

                int currentStageIndex = workflowStages.IndexOf(currentState);
                var validStages = workflowStages.Take(currentStageIndex + 1).ToList();

                var isObserver = await _context.WorkflowAccesses
                                          .Where(a => a.UserID == currentUserId &&
                                                      a.ProcessName == "MissionReportWorkflow" &&
                                                      a.Role == "Observer" &&
                                                      validStages.Contains(a.Section))
                                          .AnyAsync();

                // تنظیم دسترسی Observer (قانون 1)
                //var isObserver = await _context.WorkflowAccesses
                //    .AnyAsync(a => a.UserID == currentUserId && a.ProcessName == "MissionReportWorkflow" && a.Role == "Observer");

                var isAssignedUser = await _context.WorkflowInstances.AnyAsync(w => w.WorkFlowID == id.Value && w.AssignedUserID == currentUserId && w.ProcessName == "MissionReportWorkflow");
                ViewBag.IsObserver = isSuperAdminOrManagement || isObserver || isAssignedUser; // قانون 1 و 2

                // تنظیم دسترسی ویرایش (قوانین 3 و 4)
                WorkflowInstance workflowInstance = await _context.WorkflowInstances.Where(w => w.WorkFlowID == id.Value && w.Section == currentState && w.ProcessName == "MissionReportWorkflow")
                    .OrderByDescending(w => w.AssignedDate).FirstOrDefaultAsync();

                var isEditorFromAccess = await _context.WorkflowAccesses.AnyAsync(a => a.UserID == currentUserId && a.ProcessName == "MissionReportWorkflow" && a.Role == "Editor" && a.Section == currentState);
                var isEditorFromInstance = workflowInstance?.IsEditor == true && workflowInstance.AssignedUserID == currentUserId;

                ViewBag.CanEdit = (isEditorFromAccess || isEditorFromInstance) && currentState == workflowInstance?.Section;
                ViewBag.WorkflowInstance = workflowInstance;
                ViewBag.WorkflowHistory = workflowHistory;
            }
            else
            {
                model = new MissionReport
                {
                    MissionReportID = 0,
                    S_persiandate = DateTime.Now.ToPersianDateTime().ToString("yyyy/MM/dd"),
                    State = "start0",
                    Req_name = reqName
                };
                ViewBag.Guid = Guid.NewGuid();
                ViewBag.CanEdit = User.Identity.IsAuthenticated; // همه کاربران احراز هویت‌شده می‌تونن شروع کنن
                ViewBag.IsObserver = false;
                ViewBag.WorkflowInstance = null;
                ViewBag.WorkflowHistory = null;
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveForm1([FromBody] SaveForm1Dto dto)
        {
            var username = User.Identity.Name;
            var userid = await _context.Users.Where(p => p.UserName == username).Select(j => j.Id).SingleOrDefaultAsync();
            // بررسی تکراری بودن Guid
            if (await _context.MissionReports.AnyAsync(x => x.Guid == dto.Guid))
            {
                return Json(new { success = false, message = "این درخواست قبلاً ثبت شده است." });
            }
            ModelState.Remove("ManagerUserID");
            if (ModelState.IsValid)
            {
                var model = new MissionReport
                {
                    MissionReportID = dto.MissionReportID,
                    S_persiandate = dto.S_persiandate,
                    Number_Hokm = dto.Number_Hokm,
                    AzTaraf = dto.AzTaraf,
                    DropdownVar002 = dto.DropdownVar002,
                    DropdownVar001 = dto.DropdownVar001,
                    Req_name = dto.Req_name,
                    Req_UserID = userid,
                    Number_Personeli = dto.Number_Personeli,
                    DropdownVar003 = dto.DropdownVar003,
                    Mamuriat_Sazman = dto.Mamuriat_Sazman,
                    Shahr_Mamuriat = dto.Shahr_Mamuriat,
                    Modat_mamuriat = dto.Modat_mamuriat,
                    Noe_Mamuriat = dto.Noe_Mamuriat,
                    Start_persiandate_var = dto.Start_persiandate_var,
                    Start_Hour = dto.Start_Hour,
                    End_persiandate_var = dto.End_persiandate_var,
                    End_Hour = dto.End_Hour,
                    Janeshin = dto.Janeshin,
                    Sharh_Mamuriat = dto.Sharh_Mamuriat,
                    State = "TayidModirVahed1",
                    Guid = dto.Guid,
                    //ManagerUserID = _context.ChooseUsers.Where(p => p.UserID == userid && p.WorkFlow == "ماموریت").Select(p => p.ManagerID).FirstOrDefault()
                };

                _context.MissionReports.Add(model);
                await _context.SaveChangesAsync();

                var workflowInstance = new WorkflowInstance
                {
                    WorkFlowID = model.MissionReportID,
                    ProcessName = "MissionReportWorkflow",
                    Section = "start0",
                    AssignedUserID = userid,
                    IsEditor = false,
                    AssignedDate = DateTime.Now,
                    IsCompleted = true
                };
                _context.WorkflowInstances.Add(workflowInstance);

                var workflowInstance1 = new WorkflowInstance
                {
                    WorkFlowID = model.MissionReportID,
                    ProcessName = "MissionReportWorkflow",
                    Section = model.State,
                    //AssignedUserID = _context.ChooseUsers.Where(p => p.UserID == userid && p.WorkFlow == "ماموریت").Select(p => p.ManagerID).FirstOrDefault(),
                    IsEditor = true,
                    AssignedDate = DateTime.Now,
                    IsCompleted = false
                };
                _context.WorkflowInstances.Add(workflowInstance1);

                //NotifyUser(userid, _context.ChooseUsers.Where(p => p.UserID == userid && p.WorkFlow == "ماموریت").Select(p => p.ManagerID).FirstOrDefault(), dto.Guid, "تایید مدیر واحد");
                NotifyObservers(model.State, userid, dto.Guid);

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "داده‌ها با موفقیت ذخیره شدند.", id = model.MissionReportID });
            }
            return Json(new { success = false, message = "خطا در ذخیره داده‌ها." });
        }
        [HttpPost]
        public async Task<IActionResult> SaveForm2([FromBody] SaveForm2Dto dto)
        {
            var username = User.Identity.Name;
            var userid = await _context.Users.Where(p => p.UserName == username).Select(j => j.Id).SingleOrDefaultAsync();
            if (ModelState.IsValid)
            {
                var model = await _context.MissionReports.FirstOrDefaultAsync(g => g.MissionReportID == dto.MissionReportID);
                if (model == null)
                    return Json(new { success = false, message = "رکورد یافت نشد." });

                model.Radio_Tayid1 = dto.Radio_Tayid1;
                model.Tozihat_Tayid1 = dto.Tozihat_Tayid1;
                model.State = "TakmilGozaresh2";
                var workflowInstance = await _context.WorkflowInstances.FirstOrDefaultAsync(g => g.WorkFlowID == dto.MissionReportID && g.Section == "TayidModirVahed1" && g.ProcessName == "MissionReportWorkflow");
                workflowInstance.IsCompleted = true;


                var workflowInstance1 = new WorkflowInstance
                {
                    WorkFlowID = model.MissionReportID,
                    ProcessName = "MissionReportWorkflow",
                    Section = model.State,
                    AssignedUserID = _context.WorkflowInstances.Where(p => p.WorkFlowID == model.MissionReportID && p.Section == "start0" && p.ProcessName == "MissionReportWorkflow").Select(p => p.AssignedUserID).FirstOrDefault(),
                    IsEditor = true,
                    AssignedDate = DateTime.Now,
                    IsCompleted = false
                };
                _context.WorkflowInstances.Add(workflowInstance1);

                NotifyUser(userid, _context.WorkflowInstances.Where(p => p.WorkFlowID == model.MissionReportID && p.Section == "start0" && p.ProcessName == "MissionReportWorkflow").Select(p => p.AssignedUserID).FirstOrDefault(), dto.Guid, "تکمیل گزارش");
                NotifyObservers(model.State, userid, dto.Guid);

                await _context.SaveChangesAsync();


                return Json(new { success = true, message = "داده‌ها با موفقیت ذخیره شدند.", id = model.MissionReportID });
            }
            return Json(new { success = false, message = "خطا در ذخیره داده‌ها." });
        }

        [HttpPost]
        public async Task<IActionResult> SaveForm3([FromBody] SaveForm3Dto dto)
        {
            var username = User.Identity.Name;
            var userid = await _context.Users.Where(p => p.UserName == username).Select(j => j.Id).SingleOrDefaultAsync();
            if (ModelState.IsValid)
            {
                var model = await _context.MissionReports.FirstOrDefaultAsync(g => g.MissionReportID == dto.MissionReportID);
                if (model == null)
                    return Json(new { success = false, message = "رکورد یافت نشد." });

                model.Rahgiri = dto.Rahgiri;
                model.TozihatGozaresh = dto.TozihatGozaresh;
                model.DispatchedPersonnel = dto.DispatchedPersonnel ?? new List<DispatchPersonnel>();
                model.MetPersonnel = dto.MetPersonnel ?? new List<MetPersonnel>();
                model.MissionExpenses = dto.MissionExpenses ?? new List<MissionExpense>();


                model.State = "TayidModirVahed3";

                var workflowInstance = await _context.WorkflowInstances.FirstOrDefaultAsync(g => g.WorkFlowID == dto.MissionReportID && g.Section == "TakmilGozaresh2" && g.ProcessName == "MissionReportWorkflow");
                workflowInstance.IsCompleted = true;

                var workflowInstance1 = new WorkflowInstance
                {
                    WorkFlowID = model.MissionReportID,
                    ProcessName = "MissionReportWorkflow",
                    Section = model.State,
                    //AssignedUserID = _context.ChooseUsers.Where(p => p.UserID == userid && p.WorkFlow == "ماموریت").Select(p => p.ManagerID).FirstOrDefault(),
                    IsEditor = true,
                    AssignedDate = DateTime.Now,
                    IsCompleted = false
                };
                _context.WorkflowInstances.Add(workflowInstance1);

                var filelist = _context.FileDBs.Where(x => x.Guid == dto.Guid).ToList();

                foreach (var item in filelist)
                {
                    if (item.IsTemp == true)
                    {
                        var file = _context.FileDBs.Where(x => x.FileID == item.FileID).First();
                        file.IsTemp = false;
                        _context.FileDBs.Update(file);
                        _context.SaveChanges();
                    }
                }
                string numberHokm = model.Number_Hokm ?? "نامشخص";
                string notificationTitle = $"تایید مدیر واحد - حکم: {numberHokm}";
                //NotifyUser(userid, _context.ChooseUsers.Where(p => p.UserID == userid && p.WorkFlow == "ماموریت").Select(p => p.ManagerID).FirstOrDefault(), model.Guid, notificationTitle);
                NotifyObservers(model.State, userid, model.Guid);

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "داده‌ها با موفقیت ذخیره شدند.", id = model.MissionReportID });
            }
            return Json(new { success = false, message = "خطا در ذخیره داده‌ها." });
        }
        [HttpPost]
        public async Task<IActionResult> SaveForm4([FromBody] SaveForm4Dto dto)
        {
            var username = User.Identity.Name;
            var userid = await _context.Users.Where(p => p.UserName == username).Select(j => j.Id).SingleOrDefaultAsync();
            if (ModelState.IsValid)
            {
                var model = await _context.MissionReports.FirstOrDefaultAsync(g => g.MissionReportID == dto.MissionReportID);
                if (model == null)
                    return Json(new { success = false, message = "رکورد یافت نشد." });

                model.Radio_Tayid2 = dto.Radio_Tayid2;
                model.Tozihat_Tayid2 = dto.Tozihat_Tayid2;
                model.State = "BarrasiEdari4";

                var workflowInstance = await _context.WorkflowInstances.FirstOrDefaultAsync(g => g.WorkFlowID == dto.MissionReportID && g.Section == "TayidModirVahed3" && g.ProcessName == "MissionReportWorkflow");
                workflowInstance.IsCompleted = true;

                var workflowInstance1 = new WorkflowInstance
                {
                    WorkFlowID = model.MissionReportID,
                    ProcessName = "MissionReportWorkflow",
                    Section = model.State,
                    AssignedUserID = GetNextAssignedUser(model.State),
                    IsEditor = false,
                    AssignedDate = DateTime.Now,
                    IsCompleted = true
                };
                _context.WorkflowInstances.Add(workflowInstance1);
                string numberHokm = model.Number_Hokm ?? "نامشخص";
                string notificationTitle = $"بررسی توسط سرپرست اداری - حکم: {numberHokm}";
                NotifyUser(userid, GetNextAssignedUser(model.State), dto.Guid, notificationTitle);
                NotifyObservers(model.State, userid, dto.Guid);
                await _context.SaveChangesAsync();


                return Json(new { success = true, message = "داده‌ها با موفقیت ذخیره شدند.", id = model.MissionReportID });
            }
            return Json(new { success = false, message = "خطا در ذخیره داده‌ها." });
        }
        [HttpPost]
        public async Task<IActionResult> SaveForm5([FromBody] SaveForm5Dto dto)
        {
            var username = User.Identity.Name;
            var userid = await _context.Users.Where(p => p.UserName == username).Select(j => j.Id).SingleOrDefaultAsync();
            if (ModelState.IsValid)
            {
                var model = await _context.MissionReports.FirstOrDefaultAsync(g => g.MissionReportID == dto.MissionReportID);
                if (model == null)
                    return Json(new { success = false, message = "رکورد یافت نشد." });

                model.TozihatSarparastedari = dto.TozihatSarparastedari;
                model.State = "Location5";

                var workflowInstance = await _context.WorkflowInstances.FirstOrDefaultAsync(g => g.WorkFlowID == dto.MissionReportID && g.Section == "BarrasiEdari4" && g.ProcessName == "MissionReportWorkflow");
                workflowInstance.IsCompleted = true;

                var workflowInstance1 = new WorkflowInstance
                {
                    WorkFlowID = model.MissionReportID,
                    ProcessName = "MissionReportWorkflow",
                    Section = model.State,
                    AssignedUserID = GetNextAssignedUser(model.State),
                    IsEditor = true,
                    AssignedDate = DateTime.Now,
                    IsCompleted = false
                };
                _context.WorkflowInstances.Add(workflowInstance1);

                NotifyUser(userid, GetNextAssignedUser(model.State), model.Guid, "تایید لوکیشن");
                NotifyObservers(model.State, userid, model.Guid);

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "داده‌ها با موفقیت ذخیره شدند.", id = model.MissionReportID });
            }
            return Json(new { success = false, message = "خطا در ذخیره داده‌ها." });
        }
        [HttpPost]
        public async Task<IActionResult> SaveForm6([FromBody] SaveForm6Dto dto)
        {
            var username = User.Identity.Name;
            var userid = await _context.Users.Where(p => p.UserName == username).Select(j => j.Id).SingleOrDefaultAsync();
            if (ModelState.IsValid)
            {
                var model = await _context.MissionReports.FirstOrDefaultAsync(g => g.MissionReportID == dto.MissionReportID);
                if (model == null)
                    return Json(new { success = false, message = "رکورد یافت نشد." });

                model.Location = dto.Location;
                model.Desclocation = dto.Desclocation;
                model.State = "Kargozini6"; // باقی ماندن در همان حالت برای بخش بعدی

                var workflowInstance = await _context.WorkflowInstances.FirstOrDefaultAsync(g => g.WorkFlowID == dto.MissionReportID && g.Section == "Location5" && g.ProcessName == "MissionReportWorkflow");
                workflowInstance.IsCompleted = true;

                var workflowInstance1 = new WorkflowInstance
                {
                    WorkFlowID = model.MissionReportID,
                    ProcessName = "MissionReportWorkflow",
                    Section = model.State,
                    AssignedUserID = GetNextAssignedUser(model.State),
                    IsEditor = true,
                    AssignedDate = DateTime.Now,
                    IsCompleted = false
                };
                _context.WorkflowInstances.Add(workflowInstance1);
                string numberHokm = model.Number_Hokm ?? "نامشخص";
                string notificationTitle = $"ثبت ساعت و تاریخ توسط کارگزینی - حکم: {numberHokm}";
                NotifyUser(userid, GetNextAssignedUser(model.State), dto.Guid, notificationTitle);
                NotifyObservers(model.State, userid, model.Guid);

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "داده‌ها با موفقیت ذخیره شدند.", id = model.MissionReportID });
            }
            return Json(new { success = false, message = "خطا در ذخیره داده‌ها." });
        }
        [HttpPost]
        public async Task<IActionResult> SaveForm7([FromBody] SaveForm7Dto dto)
        {
            var username = User.Identity.Name;
            var userid = await _context.Users.Where(p => p.UserName == username).Select(j => j.Id).SingleOrDefaultAsync();
            if (ModelState.IsValid)
            {
                var model = await _context.MissionReports.FirstOrDefaultAsync(g => g.MissionReportID == dto.MissionReportID);
                if (model == null)
                    return Json(new { success = false, message = "رکورد یافت نشد." });

                model.Start_persiandate_var2 = dto.Start_persiandate_var2;
                model.Start_Hour2 = dto.Start_Hour2;
                model.End_persiandate_var3 = dto.End_persiandate_var3;
                model.End_Hour2 = dto.End_Hour2;
                model.TozihatKargozini = dto.TozihatKargozini;
                model.State = "End7";

                var workflowInstance = await _context.WorkflowInstances.FirstOrDefaultAsync(g => g.WorkFlowID == dto.MissionReportID && g.Section == "Kargozini6" && g.ProcessName == "MissionReportWorkflow");
                workflowInstance.IsCompleted = true;

                var startUserID = await _context.WorkflowInstances
                    .Where(g => g.WorkFlowID == dto.MissionReportID && g.Section == "start0" && g.ProcessName == "MissionReportWorkflow")
                    .Select(p => p.AssignedUserID)
                    .SingleOrDefaultAsync();

                var workflowInstance1 = new WorkflowInstance
                {
                    WorkFlowID = model.MissionReportID,
                    ProcessName = "MissionReportWorkflow",
                    Section = model.State,
                    AssignedUserID = startUserID,
                    IsEditor = false,
                    AssignedDate = DateTime.Now,
                    IsCompleted = true
                };
                _context.WorkflowInstances.Add(workflowInstance1);

                NotifyUser(userid, startUserID, model.Guid, "پایان");
                NotifyObservers(model.State, userid, model.Guid);

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "داده‌ها با موفقیت ذخیره شدند.", id = model.MissionReportID });
            }
            return Json(new { success = false, message = "خطا در ذخیره داده‌ها." });
        }

        private string GetNextAssignedUser(string state)
        {
            var nextAccess = _context.WorkflowAccesses
                .Where(a => a.ProcessName == "MissionReportWorkflow" && a.Section == state && a.Role == "Editor")
                .Select(a => a.UserID)
                .FirstOrDefault();

            if (nextAccess == null)
            {
                //_logger.LogWarning($"هیچ کاربری برای بخش {state} با نقش Editor یافت نشد.");
                return null;
            }

            return nextAccess;
        }

        private void NotifyObservers(string section, string senderId, string guid)
        {
            var observers = _context.WorkflowAccesses
                .Where(wa => wa.Role == "Observer" &&
                             wa.ProcessName == "MissionReportWorkflow" &&
                             wa.Section == section)
                .Select(wa => wa.UserID)
                .Distinct()
                .ToList();

            foreach (var observerId in observers)
            {
                var observerNotification = new ViewProcess
                {
                    SenderID = senderId,
                    ReceiverID = observerId,
                    SendDateTime = DateTime.Now,
                    Guid = guid.ToString(),
                    IsView = false,
                    Type = "گزارش ماموریت",
                    Title = "جهت اطلاع"
                };
                _context.ViewProcesss.Add(observerNotification);
            }

            _context.SaveChanges();
        }

        private void NotifyUser(string senderId, string receiverId, string guid, string title, string type = "گزارش ماموریت")
        {
            var notification = new ViewProcess
            {
                SenderID = senderId,
                ReceiverID = receiverId,
                SendDateTime = DateTime.Now,
                Guid = guid.ToString(),
                IsView = false,
                Type = type,
                Title = title
            };

            _context.ViewProcesss.Add(notification);
            _context.SaveChanges();
        }

        public JsonResult GetManagerUser()
        {
            var users = _context.Users
                         .Where(p => p.InTFC == true)
                         .Select(u => new
                         {
                             id = u.Id,
                             text = u.FirstName + " " + u.LastName
                         }).ToList();

            return Json(users);

        }

        public JsonResult DownloadFile(int id)
        {
            var missionreports = _context.MissionReports.Find(id);
            var guid = missionreports.Guid;
            var filelist = _context.FileDBs.Where(x => x.Guid == guid).ToList();
            if (filelist != null)
            {
                var model = filelist.Select(u => new DownloadVM()
                {
                    FileID = u.FileID,
                    Name = u.Name,
                    Extension = u.Extension,
                }).ToList();
                return Json(new { data = model, status = "OK" }, new Newtonsoft.Json.JsonSerializerSettings());
            }
            //return File(file.Data, file.FileType, file.Name + file.Extension);
            return Json(new { status = "NOT OK" });
        }

        public JsonResult History(string Guid)
        {
            var viewprocesslist = _context.ViewProcesss.Where(x => x.Guid == Guid).ToList();
            if (viewprocesslist != null && viewprocesslist.Any())
            {
                var model = viewprocesslist.Select(u => new ViewProcessViewModel
                {
                    ViewProcessID = u.ViewProcessID,
                    SenderID = _context.Users.Where(p => p.Id == u.SenderID).Select(p => p.FirstName + " " + p.LastName).SingleOrDefault(),
                    ReceiverID = _context.Users.Where(p => p.Id == u.ReceiverID).Select(p => p.FirstName + " " + p.LastName).SingleOrDefault(),
                    SendDateTime = u.SendDateTime.ToPersianDateTime().ToString("yyyy/MM/dd HH:mm"),
                    ViewDateTime = u.ViewDateTime.HasValue ? u.ViewDateTime.Value.ToPersianDateTime().ToString("yyyy/MM/dd HH:mm") : "مشاهده نشده",
                    Title = u.Title,
                    Type = u.Type
                }).ToList();
                return Json(new { data = model, status = "OK" }, new Newtonsoft.Json.JsonSerializerSettings());
            }
            return Json(new { status = "NOT OK", data = new List<object>() });
        }
    }

}