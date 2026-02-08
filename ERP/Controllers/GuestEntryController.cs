using ERP.Data;
using ERP.Models;

using ERP.ViewModels.GuestEntry;
using ERP.ViewModels.ViewProcess;
using ERP.ViewModels.WorkReport;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace ERP.Controllers
{
    //1-تمام افرادی که نقش observ هستند تاجایی که کامل شده رو میتونن ببینن 
    //2-تمامی افرادی که WorkflowInstances به عنوان Assigned user هستند میتوند تا جایی که کامل شده رو ببینن 
    //3-افرادی که در WorkflowAccesses نقش editor دارند میتواندد فقط بخش خود را ادیت کنند و بقیه را تا جایی که تکمیل شده ببینند.
    //4-در WorkflowInstances افرادی که iseditor:true هستند فقط میتوانند بخش خود را ادیت کنند و بقیه را تا جایی که تکمیل شده ببیند.

    [Authorize]
    public class GuestEntryController : Controller
    {
        private readonly ERPContext _context;
        private readonly ILogger<GuestEntryController> _logger;
        public GuestEntryController(ERPContext context, ILogger<GuestEntryController> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<IActionResult> Index()
        {
            var username = User.Identity.Name;
            var userid = _context.Users.Where(p => p.UserName == username).Select(j => j.Id).FirstOrDefault();

            var workflowStages = new List<string>
    {
        "start0",
        "TayidModirVahed1",
        "Edary2",
        "NegahbaniVorud3",
        "NegahbaniKhoroj4",
        "Report5",
        "ModirAmel6",
        "End7"
    };

            // چک کردن نقش‌های SuperAdmin یا Management
            bool isSuperAdminOrManagement = User.IsInRole("SuperAdmin") || User.IsInRole("Management");
            IQueryable<int> accessibleGuestEntryIds;


            if (isSuperAdminOrManagement)
            {
                // اگر کاربر SuperAdmin یا Management باشد، تمام GuestEntries را بدون فیلتر دسترسی برگردان
                accessibleGuestEntryIds = _context.GuestEntries.Select(g => g.GuestEntryID).Distinct();
            }
            else
            {
                // گرفتن تمام مراحل که کاربر در آن‌ها نقش Observer دارد
                var observerStages = await _context.WorkflowAccesses
                    .Where(a => a.UserID == userid && a.ProcessName == "GuestEntryWorkflow" && a.Role == "Observer")
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
                accessibleGuestEntryIds = _context.GuestEntries
                    .Where(g =>
                        // شرط برای Observer: فقط GuestEntryهایی که در accessibleStages هستند
                        accessibleStages.Contains(g.State) ||
                        // شرط برای AssignedUser
                        _context.WorkflowInstances.Any(wi => wi.WorkFlowID == g.GuestEntryID && wi.ProcessName == "GuestEntryWorkflow" && wi.AssignedUserID == userid))
                    .Select(g => g.GuestEntryID)
                    .Distinct();
            }


            ViewBag.all = _context.GuestEntries
                .Where(g => accessibleGuestEntryIds.Contains(g.GuestEntryID))
                .Count();

            ViewBag.s1 = _context.GuestEntries
                .Where(g => g.State == "TayidModirVahed1" && accessibleGuestEntryIds.Contains(g.GuestEntryID))
                .Count();
            ViewBag.s2 = _context.GuestEntries
                .Where(g => g.State == "Edary2" && accessibleGuestEntryIds.Contains(g.GuestEntryID))
                .Count();
            ViewBag.s3 = _context.GuestEntries
                .Where(g => g.State == "NegahbaniVorud3" && accessibleGuestEntryIds.Contains(g.GuestEntryID))
                .Count();
            ViewBag.s4 = _context.GuestEntries
                .Where(g => g.State == "NegahbaniKhoroj4" && accessibleGuestEntryIds.Contains(g.GuestEntryID))
                .Count();
            ViewBag.s5 = _context.GuestEntries
                .Where(g => g.State == "Report5" && accessibleGuestEntryIds.Contains(g.GuestEntryID))
                .Count();
            ViewBag.s6 = _context.GuestEntries
                .Where(g => g.State == "ModirAmel6" && accessibleGuestEntryIds.Contains(g.GuestEntryID))
                .Count();
            ViewBag.s7 = _context.GuestEntries
                .Where(g => g.State == "End7" && accessibleGuestEntryIds.Contains(g.GuestEntryID))
                .Count();

            return View();
        }

        public async Task<DataSourceResult> Data_GuestEntries([DataSourceRequest] DataSourceRequest request, string SelectTab)
        {
            var username = User.Identity.Name;
            var userid = _context.Users.Where(p => p.UserName == username).Select(j => j.Id).FirstOrDefault();
            // چک کردن نقش‌های SuperAdmin یا Management
            bool isSuperAdminOrManagement = User.IsInRole("SuperAdmin") || User.IsInRole("Management");
            // تعریف ترتیب مراحل
            var workflowStages = new List<string>
                    {
                        "start0",
                        "TayidModirVahed1",
                        "Edary2",
                        "NegahbaniVorud3",
                        "NegahbaniKhoroj4",
                        "Report5",
                        "ModirAmel6",
                        "End7"
                    };

            IQueryable<IndexGuestEntryViewModel> model1;
            if (isSuperAdminOrManagement)
            {
                // اگر کاربر SuperAdmin یا Management باشد، تمام GuestEntries را بدون فیلتر دسترسی برگردان
                if (SelectTab == "همه لیست")
                {
                    model1 = _context.GuestEntries
                        .Select(u => new IndexGuestEntryViewModel
                        {
                            GuestEntryID = u.GuestEntryID,
                            Req_name = _context.Users.Where(p => p.Id == u.Req_UserID).Select(p => p.FirstName + " " + p.LastName).FirstOrDefault(),
                            State = _context.WorkflowSections.Where(p => p.SectionCode == u.State && p.ProcessName == "GuestEntryWorkflow").Select(p => p.SectionName).FirstOrDefault(),
                            Vorud_persiandate = u.Vorud_persiandate,
                            Guid = u.Guid,
                            SourceTable = 0 // همه لیست‌ها بدون فیلتر دسترسی
                        }).OrderByDescending(u => u.GuestEntryID);
                }
                else
                {
                    var selectedState = SelectTab;
                    model1 = _context.GuestEntries
                        .Where(g => g.State == selectedState)
                        .Select(u => new IndexGuestEntryViewModel
                        {
                            GuestEntryID = u.GuestEntryID,
                            Req_name = _context.Users.Where(p => p.Id == u.Req_UserID).Select(p => p.FirstName + " " + p.LastName).FirstOrDefault(),
                            State = _context.WorkflowSections.Where(p => p.SectionCode == u.State && p.ProcessName == "GuestEntryWorkflow").Select(p => p.SectionName).FirstOrDefault(),
                            Vorud_persiandate = u.Vorud_persiandate,
                            Guid = u.Guid,
                            SourceTable = 1 // فیلتر بر اساس State
                        }).OrderByDescending(u => u.GuestEntryID);
                }
            }
            else
            {
                // گرفتن تمام مراحل که کاربر در آن‌ها نقش Observer دارد
                var observerStages = await _context.WorkflowAccesses
                    .Where(a => a.UserID == userid && a.ProcessName == "GuestEntryWorkflow" && a.Role == "Observer")
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
                    model1 = _context.GuestEntries
                        .Where(g =>
                            // شرط برای Observer: فقط GuestEntryهایی که در accessibleStages هستند
                            accessibleStages.Contains(g.State) ||
                            // شرط برای AssignedUser
                            _context.WorkflowInstances.Any(wi => wi.WorkFlowID == g.GuestEntryID && wi.ProcessName == "GuestEntryWorkflow" && wi.AssignedUserID == userid))
                        .Select(u => new IndexGuestEntryViewModel
                        {
                            GuestEntryID = u.GuestEntryID,
                            Req_name = _context.Users.Where(p => p.Id == u.Req_UserID).Select(p => p.FirstName + " " + p.LastName).FirstOrDefault(),
                            State = _context.WorkflowSections.Where(p => p.SectionCode == u.State && p.ProcessName == "GuestEntryWorkflow").Select(p => p.SectionName).FirstOrDefault(),
                            Vorud_persiandate = u.Vorud_persiandate,
                            Guid = u.Guid,
                            SourceTable = 0 // همه لیست‌ها با فیلتر دسترسی
                        }).OrderByDescending(u => u.GuestEntryID);
                }
                else
                {
                    var selectedState = SelectTab;
                    model1 = _context.GuestEntries
                        .Where(g => g.State == selectedState && (
                            // شرط برای Observer: فقط اگر selectedState در accessibleStages باشد
                            accessibleStages.Contains(g.State) ||
                            // شرط برای AssignedUser
                            _context.WorkflowInstances.Any(wi => wi.WorkFlowID == g.GuestEntryID && wi.ProcessName == "GuestEntryWorkflow" && wi.AssignedUserID == userid)))
                        .Select(u => new IndexGuestEntryViewModel
                        {
                            GuestEntryID = u.GuestEntryID,
                            Req_name = _context.Users.Where(p => p.Id == u.Req_UserID).Select(p => p.FirstName + " " + p.LastName).FirstOrDefault(),
                            State = _context.WorkflowSections.Where(p => p.SectionCode == u.State && p.ProcessName == "GuestEntryWorkflow").Select(p => p.SectionName).FirstOrDefault(),
                            Vorud_persiandate = u.Vorud_persiandate,
                            Guid = u.Guid,
                            SourceTable = 1 // فیلتر بر اساس State
                        }).OrderByDescending(u => u.GuestEntryID);
                }
            }
            return await model1.ToDataSourceResultAsync(request);
        }

        public async Task<IActionResult> GuestEntryView(int? id)
        {
            GuestEntry model;
            var username = User.Identity.Name;
            var userid = await _context.Users.Where(p => p.UserName == username).Select(j => j.Id).SingleOrDefaultAsync();
            var reqName = await _context.Users.Where(p => p.Id == userid).Select(p => p.FirstName + " " + p.LastName).FirstOrDefaultAsync();

            var workflowStages = new List<string>
                         {
                    "start0",
                    "TayidModirVahed1",
                    "Edary2",
                    "NegahbaniVorud3",
                    "NegahbaniKhoroj4",
                    "Report5",
                    "ModirAmel6",
                    "End7"
                       };
            if (id.HasValue && id > 0)
            {
                model = await _context.GuestEntries.FirstOrDefaultAsync(g => g.GuestEntryID == id);
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
                    .Where(w => w.WorkFlowID == id.Value && w.IsCompleted && w.ProcessName == "GuestEntryWorkflow")
                    .OrderBy(w => w.AssignedDate)
                    .ToListAsync();
                var completedSections = workflowHistory.Select(w => w.Section).ToList();
                ViewBag.CompletedSections = completedSections; // برای دیباگ

                int currentStageIndex = workflowStages.IndexOf(currentState);
                var validStages = workflowStages.Take(currentStageIndex + 1).ToList();

                var isObserver = await _context.WorkflowAccesses
                            .Where(a => a.UserID == currentUserId &&
                                        a.ProcessName == "GuestEntryWorkflow" &&
                                        a.Role == "Observer" &&
                                        validStages.Contains(a.Section))
                            .AnyAsync();

                var isAssignedUser = await _context.WorkflowInstances.AnyAsync(w => w.WorkFlowID == id.Value && w.AssignedUserID == currentUserId && w.ProcessName == "GuestEntryWorkflow");

                ViewBag.IsObserver = isSuperAdminOrManagement || isObserver || isAssignedUser; // فقط برای مرحله فعلی یا اگر کاربر Assigned است

                // تنظیم دسترسی ویرایش (قوانین 3 و 4)
                var workflowInstance = await _context.WorkflowInstances.Where(w => w.WorkFlowID == id.Value && w.Section == currentState && w.ProcessName == "GuestEntryWorkflow")
             .OrderByDescending(w => w.AssignedDate).FirstOrDefaultAsync();

                var isEditorFromAccess = await _context.WorkflowAccesses.AnyAsync(a => a.UserID == currentUserId && a.ProcessName == "GuestEntryWorkflow" && a.Role == "Editor" && a.Section == currentState);
                var isEditorFromInstance = workflowInstance?.IsEditor == true && workflowInstance.AssignedUserID == currentUserId;

                // اگر کاربر Editor است یا AssignedUser با IsEditor=true است، اجازه ویرایش بده
                ViewBag.CanEdit = (isEditorFromAccess || isEditorFromInstance) && currentState == workflowInstance?.Section;
                ViewBag.WorkflowInstance = workflowInstance;
                ViewBag.WorkflowHistory = workflowHistory;
            }
            else
            {
                model = new GuestEntry
                {
                    GuestEntryID = 0,
                    Req_UserID = reqName,
                    State = "start0"
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
        public async Task<IActionResult> SaveForm1([FromBody] SaveForm1 dto)
        {
            var username = User.Identity.Name;
            var userid = await _context.Users.Where(p => p.UserName == username).Select(j => j.Id).SingleOrDefaultAsync();
            // بررسی تکراری بودن Guid
            if (await _context.GuestEntries.AnyAsync(x => x.Guid == dto.Guid))
            {
                return Json(new { success = false, message = "این درخواست قبلاً ثبت شده است." });
            }
            if (ModelState.IsValid)
            {
                var model = new GuestEntry
                {
                    Req_UserID = userid,
                    Req_Section = dto.Req_Section,
                    Time = dto.Time,
                    MahalPazirayi = dto.MahalPazirayi,
                    Guests = dto.Guests ?? new List<GuestInfo>(), // مدیریت null
                    HadafBazdid = dto.HadafBazdid,
                    Name_Sazman = dto.Name_Sazman,
                    Vorud_persiandate = dto.Vorud_persiandate,
                    Vorud_Saat = dto.Vorud_Saat,
                    Noe_Mosaferat = dto.Noe_Mosaferat,
                    Eghamat = dto.Eghamat,
                    PhoneNumber = dto.PhoneNumber,
                    Bazdid = dto.Bazdid,
                    Gift = dto.Gift,
                    Lunch = dto.Lunch,
                    Jalasat = dto.Jalasat,
                    MojavezKhodro = dto.MojavezKhodro,
                    Khodro = dto.Khodro,
                    Companions = dto.Companions ?? new List<string>(), // مدیریت null
                    Tozihat_Darkhast = dto.Tozihat_Darkhast,
                    TayidModirVahed = true,
                    TozihatModirVahed = null,
                    Tozihat_Edary = null, // مقادیر پیش‌فرض برای فیلدهایی که ارسال نمی‌شن
                    TarikhVorud_persiandate = null,
                    SaateVorud = null,
                    Tozihat_Negahbani = null,
                    SaatKhoroj = null,
                    Tozihat_DarkhastKonnande = null,
                    Tozihat_ModirAmel = null,
                    State = "TayidModirVahed1",
                    S_persiandate = DateTime.Now.ToString(),
                    Guid = dto.Guid,
                };

                _context.GuestEntries.Add(model);
                await _context.SaveChangesAsync();

                var workflowInstance = new WorkflowInstance
                {
                    WorkFlowID = model.GuestEntryID,
                    ProcessName = "GuestEntryWorkflow",
                    Section = "start0",
                    AssignedUserID = userid,
                    IsEditor = false,
                    AssignedDate = DateTime.Now,
                    IsCompleted = true
                };
                _context.WorkflowInstances.Add(workflowInstance);

                var workflowInstance1 = new WorkflowInstance
                {
                    WorkFlowID = model.GuestEntryID,
                    ProcessName = "GuestEntryWorkflow",
                    Section = model.State,
                    AssignedUserID = dto.ManagerUserID,
                    IsEditor = true,
                    AssignedDate = DateTime.Now,
                    IsCompleted = false
                };
                _context.WorkflowInstances.Add(workflowInstance1);

                NotifyUser(userid, dto.ManagerUserID, dto.Guid, "تایید مدیر واحد");

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "داده‌ها با موفقیت ذخیره شدند.", id = model.GuestEntryID });
            }
            return Json(new { success = false, message = "خطا در ذخیره داده‌ها." });
        }
        [HttpPost]
        public async Task<IActionResult> SaveForm2([FromBody] SaveForm2 dto)
        {
            var username = User.Identity.Name;
            var userid = await _context.Users.Where(p => p.UserName == username).Select(j => j.Id).SingleOrDefaultAsync();
            if (ModelState.IsValid)
            {
                var model = await _context.GuestEntries
                    .FirstOrDefaultAsync(g => g.GuestEntryID == dto.GuestEntryID);

                if (model == null)
                    return Json(new { success = false, message = "رکورد یافت نشد." });

                model.TayidModirVahed = dto.TayidModirVahed;
                model.TozihatModirVahed = dto.TozihatModirVahed;
                model.State = "Edary2";


                var workflowInstance = await _context.WorkflowInstances.FirstOrDefaultAsync(g => g.WorkFlowID == dto.GuestEntryID && g.Section == "TayidModirVahed1" && g.ProcessName == "GuestEntryWorkflow");
                workflowInstance.IsCompleted = true;


                var workflowInstance1 = new WorkflowInstance
                {
                    WorkFlowID = model.GuestEntryID,
                    ProcessName = "GuestEntryWorkflow",
                    Section = model.State,
                    AssignedUserID = GetNextAssignedUser(model.State),
                    IsEditor = false,
                    AssignedDate = DateTime.Now,
                    IsCompleted = false
                };
                _context.WorkflowInstances.Add(workflowInstance1);

                NotifyUser(userid, GetNextAssignedUser(model.State), dto.Guid, "انجام هماهنگی‌های لازم");
                NotifyObservers(model.State, userid, dto.Guid);

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "تاییدیه ذخیره شد." });
            }

            return Json(new { success = false, message = "خطا در داده‌های ارسالی." });
        }
        [HttpPost]
        public async Task<IActionResult> SaveForm3([FromBody] SaveForm3 dto)
        {
            var username = User.Identity.Name;
            var userid = await _context.Users.Where(p => p.UserName == username).Select(j => j.Id).SingleOrDefaultAsync();
            if (ModelState.IsValid)
            {
                var model = await _context.GuestEntries.FirstOrDefaultAsync(g => g.GuestEntryID == dto.GuestEntryID);
                if (model == null) return Json(new { success = false, message = "رکورد یافت نشد." });

                model.Tozihat_Edary = dto.Tozihat_Edary;
                model.State = "NegahbaniVorud3";


                var workflowInstance = await _context.WorkflowInstances.FirstOrDefaultAsync(g => g.WorkFlowID == dto.GuestEntryID && g.Section == "Edary2" && g.ProcessName == "GuestEntryWorkflow");
                workflowInstance.IsCompleted = true;

                var workflowInstance1 = new WorkflowInstance
                {
                    WorkFlowID = model.GuestEntryID,
                    ProcessName = "GuestEntryWorkflow",
                    Section = model.State,
                    AssignedUserID = GetNextAssignedUser(model.State),
                    IsEditor = false,
                    AssignedDate = DateTime.Now,
                    IsCompleted = false
                };
                _context.WorkflowInstances.Add(workflowInstance1);

                NotifyUser(userid, GetNextAssignedUser(model.State), dto.Guid, "ثبت تاریخ و ساعت ورود");
                NotifyObservers(model.State, userid, dto.Guid);


                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "داده‌ها با موفقیت ذخیره شدند.", id = model.GuestEntryID });
            }
            return Json(new { success = false, message = "خطا در ذخیره داده‌ها." });
        }
        [HttpPost]
        public async Task<IActionResult> SaveForm4([FromBody] SaveForm4 dto)
        {
            var username = User.Identity.Name;
            var userid = await _context.Users.Where(p => p.UserName == username).Select(j => j.Id).SingleOrDefaultAsync();
            if (ModelState.IsValid)
            {
                var model = await _context.GuestEntries.FirstOrDefaultAsync(g => g.GuestEntryID == dto.GuestEntryID);
                if (model == null) return Json(new { success = false, message = "رکورد یافت نشد." });

                model.TarikhVorud_persiandate = dto.TarikhVorud_persiandate;
                model.SaateVorud = dto.SaateVorud;
                model.Tozihat_Negahbani = dto.Tozihat_Negahbani;
                model.State = "NegahbaniKhoroj4";

                var workflowInstance = await _context.WorkflowInstances.FirstOrDefaultAsync(g => g.WorkFlowID == dto.GuestEntryID && g.Section == "NegahbaniVorud3" && g.ProcessName == "GuestEntryWorkflow");
                workflowInstance.IsCompleted = true;

                var workflowInstance1 = new WorkflowInstance
                {
                    WorkFlowID = model.GuestEntryID,
                    ProcessName = "GuestEntryWorkflow",
                    Section = model.State,
                    AssignedUserID = GetNextAssignedUser(model.State),
                    IsEditor = false,
                    AssignedDate = DateTime.Now,
                    IsCompleted = true
                };
                _context.WorkflowInstances.Add(workflowInstance1);

                NotifyUser(userid, GetNextAssignedUser(model.State), dto.Guid, "ثبت تاریخ و ساعت خروج");
                NotifyObservers(model.State, userid, dto.Guid);

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "داده‌ها با موفقیت ذخیره شدند.", id = model.GuestEntryID });
            }
            return Json(new { success = false, message = "خطا در ذخیره داده‌ها." });
        }
        [HttpPost]
        public async Task<IActionResult> SaveForm5([FromBody] SaveForm5 dto)
        {
            var username = User.Identity.Name;
            var userid = await _context.Users.Where(p => p.UserName == username).Select(j => j.Id).SingleOrDefaultAsync();
            if (ModelState.IsValid)
            {
                var model = await _context.GuestEntries.FirstOrDefaultAsync(g => g.GuestEntryID == dto.GuestEntryID);
                if (model == null) return Json(new { success = false, message = "رکورد یافت نشد." });

                model.SaatKhoroj = dto.SaatKhoroj;
                model.State = "Report5";

                var workflowInstance = await _context.WorkflowInstances.FirstOrDefaultAsync(g => g.WorkFlowID == dto.GuestEntryID && g.Section == "NegahbaniKhoroj4" && g.ProcessName == "GuestEntryWorkflow");
                workflowInstance.IsCompleted = true;

                var StartUserID = _context.WorkflowInstances.Where(g => g.WorkFlowID == dto.GuestEntryID && g.Section == "start0" && g.ProcessName == "GuestEntryWorkflow").Select(p => p.AssignedUserID).FirstOrDefault();

                var workflowInstance1 = new WorkflowInstance
                {
                    WorkFlowID = model.GuestEntryID,
                    ProcessName = "GuestEntryWorkflow",
                    Section = model.State,
                    AssignedUserID = StartUserID,
                    IsEditor = true,
                    AssignedDate = DateTime.Now,
                    IsCompleted = false
                };
                _context.WorkflowInstances.Add(workflowInstance1);


                NotifyUser(userid, StartUserID, dto.Guid, "تکمیل گزارش بازدید");
                NotifyObservers(model.State, userid, dto.Guid);

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "داده‌ها با موفقیت ذخیره شدند.", id = model.GuestEntryID });
            }
            return Json(new { success = false, message = "خطا در ذخیره داده‌ها." });
        }
        [HttpPost]
        public async Task<IActionResult> SaveForm6([FromBody] SaveForm6 dto)
        {
            var username = User.Identity.Name;
            var userid = await _context.Users.Where(p => p.UserName == username).Select(j => j.Id).SingleOrDefaultAsync();
            if (ModelState.IsValid)
            {
                var model = await _context.GuestEntries.FirstOrDefaultAsync(g => g.GuestEntryID == dto.GuestEntryID);
                if (model == null) return Json(new { success = false, message = "رکورد یافت نشد." });

                //model.Tozihat_DarkhastKonnande = dto.Tozihat_DarkhastKonnande;
                model.MasolAnjam = dto.MasolAnjam;
                model.MasolPeygiry = dto.MasolPeygiry;
                model.SharhMavared = dto.SharhMavared;
                model.MohlatAnjam = dto.MohlatAnjam;
                model.State = "ModirAmel6";

                var workflowInstance = await _context.WorkflowInstances.FirstOrDefaultAsync(g => g.WorkFlowID == dto.GuestEntryID && g.Section == "Report5" && g.ProcessName == "GuestEntryWorkflow");
                workflowInstance.IsCompleted = true;

                var workflowInstance1 = new WorkflowInstance
                {
                    WorkFlowID = model.GuestEntryID,
                    ProcessName = "GuestEntryWorkflow",
                    Section = model.State,
                    AssignedUserID = GetNextAssignedUser(model.State),
                    IsEditor = false,
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

                NotifyUser(userid, GetNextAssignedUser(model.State), dto.Guid, "اظهار نظر مدیر عامل");
                NotifyObservers(model.State, userid, dto.Guid);

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "داده‌ها با موفقیت ذخیره شدند.", id = model.GuestEntryID });
            }
            return Json(new { success = false, message = "خطا در ذخیره داده‌ها." });
        }
        [HttpPost]
        public async Task<IActionResult> SaveForm7([FromBody] SaveForm7 dto)
        {
            var username = User.Identity.Name;
            var userid = await _context.Users.Where(p => p.UserName == username).Select(j => j.Id).SingleOrDefaultAsync();
            if (ModelState.IsValid)
            {
                var model = await _context.GuestEntries.FirstOrDefaultAsync(g => g.GuestEntryID == dto.GuestEntryID);
                if (model == null) return Json(new { success = false, message = "رکورد یافت نشد." });

                model.Tozihat_ModirAmel = dto.Tozihat_ModirAmel;
                model.State = "End7";

                var workflowInstance = await _context.WorkflowInstances.FirstOrDefaultAsync(g => g.WorkFlowID == dto.GuestEntryID && g.Section == "ModirAmel6" && g.ProcessName == "GuestEntryWorkflow");
                workflowInstance.IsCompleted = true;

                var StartUserID = _context.WorkflowInstances.Where(g => g.WorkFlowID == dto.GuestEntryID && g.Section == "start0" && g.ProcessName == "GuestEntryWorkflow").Select(p => p.AssignedUserID).FirstOrDefault();

                var workflowInstance1 = new WorkflowInstance
                {
                    WorkFlowID = model.GuestEntryID,
                    ProcessName = "GuestEntryWorkflow",
                    Section = model.State,
                    AssignedUserID = StartUserID,
                    IsEditor = false,
                    AssignedDate = DateTime.Now,
                    IsCompleted = true
                };
                _context.WorkflowInstances.Add(workflowInstance1);

                NotifyUser(userid, StartUserID, dto.Guid, "پایان");
                NotifyObservers(model.State, userid, dto.Guid);

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "داده‌ها با موفقیت ذخیره شدند.", id = model.GuestEntryID });
            }
            return Json(new { success = false, message = "خطا در ذخیره داده‌ها." });
        }

        private string GetNextAssignedUser(string state)
        {
            // پیدا کردن کاربر با نقش Editor برای بخش مشخص‌شده در WorkflowAccess
            var nextAccess = _context.WorkflowAccesses
                .Where(a => a.ProcessName == "GuestEntryWorkflow" && a.Section == state && a.Role == "Editor")
                .Select(a => a.UserID)
                .FirstOrDefault();

            if (nextAccess == null)
            {
                // لاگ خطا یا مدیریت مورد خاص
                _logger.LogWarning($"هیچ کاربری برای بخش {state} با نقش Editor یافت نشد.");
                return null; // یا یه مقدار پیش‌فرض، مثلاً یه کاربر ادمین
            }

            return nextAccess;
        }

        public JsonResult DownloadFile(int id)
        {
            var guestentries = _context.GuestEntries.Find(id);
            var guid = guestentries.Guid;
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


        public JsonResult GetManagerUser()
        {
            var groupid = _context.Groups.Where(p => p.Name == "مدیران و سرپرستان").Select(p => p.GroupID).FirstOrDefault();
            var users = _context.UserGroups
                         .Where(p => p.GroupID == groupid)
                         .Select(u => new
                         {
                             id = u.UserID,
                             text = _context.Users.Where(p => p.Id == u.UserID).Select(p => p.FirstName + " " + p.LastName).FirstOrDefault()
                         }).ToList();

            return Json(users);

        }

        private void NotifyObservers(string section, string senderId, string guid)
        {
            var observers = _context.WorkflowAccesses
                .Where(wa => wa.Role == "Observer" &&
                             wa.ProcessName == "GuestEntryWorkflow" &&
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
                    Guid = guid,
                    IsView = false,
                    Type = "ورود مهمان",
                    Title = "جهت اطلاع",
                };
                _context.ViewProcesss.Add(observerNotification);
            }

            _context.SaveChanges();
        }
        private void NotifyUser(string senderId, string receiverId, string guid, string title, string type = "ورود مهمان")
        {
            var notification = new ViewProcess
            {
                SenderID = senderId,
                ReceiverID = receiverId,
                SendDateTime = DateTime.Now,
                Guid = guid,
                IsView = false,
                Type = type,
                Title = title
            };

            _context.ViewProcesss.Add(notification);
            _context.SaveChanges();
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

