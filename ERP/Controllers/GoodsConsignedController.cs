using ERP.Data;
using ERP.Models;
using ERP.ViewModels.GoodsConsigned;
using ERP.ViewModels.ViewProcess;
using ERP.ViewModels.WorkReport;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using static ERP.ViewModels.GoodsConsigned.GoodsConsignedVM;

namespace ERP.Controllers
{
    [Authorize]
    public class GoodsConsignedController : Controller
    {
        private readonly ERPContext _context;
        private readonly ILogger<GoodsConsignedController> _logger;

        public GoodsConsignedController(ERPContext context, ILogger<GoodsConsignedController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var username = User.Identity.Name;
            var userid = await _context.Users.Where(p => p.UserName == username).Select(j => j.Id).FirstOrDefaultAsync();

            bool isSuperAdminOrManagement = User.IsInRole("SuperAdmin") || User.IsInRole("Management");
            IQueryable<int> accessibleGoodsConsignedIds;

            // گرفتن مراحل از WorkflowStep
            var workflowSteps = await _context.WorkflowSteps
                .Where(ws => ws.ProcessName == "GoodsConsignedWorkflow")
                .OrderBy(ws => ws.OrderIndex)
                .ToListAsync();

            if (isSuperAdminOrManagement)
            {
                accessibleGoodsConsignedIds = _context.GoodsConsigneds.Select(g => g.GoodsConsignedID).Distinct();
            }
            else
            {
                var observerStages = await _context.WorkflowAccesses
                    .Where(a => a.UserID == userid && a.ProcessName == "GoodsConsignedWorkflow" && a.Role == "Observer")
                    .Select(a => a.Section)
                    .ToListAsync();

                var accessibleStages = new List<string>();
                foreach (var observerStage in observerStages)
                {
                    var observerStep = workflowSteps.FirstOrDefault(ws => ws.SectionCode == observerStage);
                    if (observerStep != null)
                    {
                        accessibleStages.AddRange(workflowSteps
                            .Where(ws => ws.OrderIndex >= observerStep.OrderIndex)
                            .Select(ws => ws.SectionCode));
                    }
                }
                accessibleStages = accessibleStages.Distinct().ToList();

                accessibleGoodsConsignedIds = _context.GoodsConsigneds
                    .Where(g => accessibleStages.Contains(g.State) ||
                                _context.WorkflowInstances.Any(wi => wi.WorkFlowID == g.GoodsConsignedID && wi.ProcessName == "GoodsConsignedWorkflow" && wi.AssignedUserID == userid))
                    .Select(g => g.GoodsConsignedID)
                    .Distinct();
            }

            ViewBag.all = await _context.GoodsConsigneds
                .Where(g => accessibleGoodsConsignedIds.Contains(g.GoodsConsignedID))
                .CountAsync();

            ViewBag.s1 = await _context.GoodsConsigneds
                .Where(g => g.State == "TayidMasool1" && accessibleGoodsConsignedIds.Contains(g.GoodsConsignedID))
                .CountAsync();
            ViewBag.s2 = await _context.GoodsConsigneds
                .Where(g => g.State == "TayidSanaye2" && accessibleGoodsConsignedIds.Contains(g.GoodsConsignedID))
                .CountAsync();
            ViewBag.s3 = await _context.GoodsConsigneds
                .Where(g => g.State == "Net3" && accessibleGoodsConsignedIds.Contains(g.GoodsConsignedID))
                .CountAsync();
            ViewBag.s4 = await _context.GoodsConsigneds
                .Where(g => g.State == "TayidTadarokat4" && accessibleGoodsConsignedIds.Contains(g.GoodsConsignedID))
                .CountAsync();
            ViewBag.s5 = await _context.GoodsConsigneds
                .Where(g => g.State == "Mali5" && accessibleGoodsConsignedIds.Contains(g.GoodsConsignedID))
                .CountAsync();
            ViewBag.s6 = await _context.GoodsConsigneds
                .Where(g => g.State == "Anbar6" && accessibleGoodsConsignedIds.Contains(g.GoodsConsignedID))
                .CountAsync();
            ViewBag.s7 = await _context.GoodsConsigneds
                .Where(g => g.State == "Negahbani7" && accessibleGoodsConsignedIds.Contains(g.GoodsConsignedID))
                .CountAsync();
            ViewBag.s8 = await _context.GoodsConsigneds
               .Where(g => g.State == "Negahbani8" && accessibleGoodsConsignedIds.Contains(g.GoodsConsignedID))
               .CountAsync();
            ViewBag.s9 = await _context.GoodsConsigneds
                .Where(g => g.State == "End9" && accessibleGoodsConsignedIds.Contains(g.GoodsConsignedID))
                .CountAsync();
 

            return View();
        }

        public async Task<DataSourceResult> Data_GoodsConsigned([DataSourceRequest] DataSourceRequest request, string SelectTab)
        {
            var username = User.Identity.Name;
            var userid = await _context.Users.Where(p => p.UserName == username).Select(j => j.Id).FirstOrDefaultAsync();
            bool isSuperAdminOrManagement = User.IsInRole("SuperAdmin") || User.IsInRole("Management");

            var workflowSteps = await _context.WorkflowSteps
                .Where(ws => ws.ProcessName == "GoodsConsignedWorkflow")
                .OrderBy(ws => ws.OrderIndex)
                .ToListAsync();

            IQueryable<IndexGoodsConsignedVM> model1;
            if (isSuperAdminOrManagement)
            {
                if (SelectTab == "همه لیست")
                {
                    model1 = _context.GoodsConsigneds
                        .Select(u => new IndexGoodsConsignedVM
                        {
                            GoodsConsignedID = u.GoodsConsignedID,
                            State = _context.WorkflowSteps
                                .Where(p => p.SectionCode == u.State && p.ProcessName == "GoodsConsignedWorkflow")
                                .Select(p => p.SectionName)
                                .FirstOrDefault() ?? u.State,
                            S_persiandate = u.S_persiandate,
                            TypeSending = u.TypeSending,
                            Guid = u.Guid,
                            SourceTable = 0
                        }).OrderByDescending(u => u.GoodsConsignedID);
                }
                else
                {
                    var selectedState = SelectTab;
                    model1 = _context.GoodsConsigneds
                        .Where(g => g.State == selectedState)
                        .Select(u => new IndexGoodsConsignedVM
                        {
                            GoodsConsignedID = u.GoodsConsignedID,
                            State = _context.WorkflowSteps
                                .Where(p => p.SectionCode == u.State && p.ProcessName == "GoodsConsignedWorkflow")
                                .Select(p => p.SectionName)
                                .FirstOrDefault() ?? u.State,
                            S_persiandate = u.S_persiandate,
                            TypeSending = u.TypeSending,
                            Guid = u.Guid,
                            SourceTable = 1
                        }).OrderByDescending(u => u.GoodsConsignedID);
                }
            }
            else
            {
                var observerStages = await _context.WorkflowAccesses
                    .Where(a => a.UserID == userid && a.ProcessName == "GoodsConsignedWorkflow" && a.Role == "Observer")
                    .Select(a => a.Section)
                    .ToListAsync();

                var accessibleStages = new List<string>();
                foreach (var observerStage in observerStages)
                {
                    var observerStep = workflowSteps.FirstOrDefault(ws => ws.SectionCode == observerStage);
                    if (observerStep != null)
                    {
                        accessibleStages.AddRange(workflowSteps
                            .Where(ws => ws.OrderIndex >= observerStep.OrderIndex)
                            .Select(ws => ws.SectionCode));
                    }
                }
                accessibleStages = accessibleStages.Distinct().ToList();

                if (SelectTab == "همه لیست")
                {
                    model1 = _context.GoodsConsigneds
                        .Where(g => accessibleStages.Contains(g.State) ||
                                    _context.WorkflowInstances.Any(wi => wi.WorkFlowID == g.GoodsConsignedID && wi.ProcessName == "GoodsConsignedWorkflow" && wi.AssignedUserID == userid))
                        .Select(u => new IndexGoodsConsignedVM
                        {
                            GoodsConsignedID = u.GoodsConsignedID,
                            State = _context.WorkflowSteps
                                .Where(p => p.SectionCode == u.State && p.ProcessName == "GoodsConsignedWorkflow")
                                .Select(p => p.SectionName)
                                .FirstOrDefault() ?? u.State,
                            S_persiandate = u.S_persiandate,
                            TypeSending = u.TypeSending,
                            Guid = u.Guid,
                            SourceTable = 0
                        }).OrderByDescending(u => u.GoodsConsignedID);
                }
                else
                {
                    var selectedState = SelectTab;
                    model1 = _context.GoodsConsigneds
                        .Where(g => g.State == selectedState && (
                            accessibleStages.Contains(g.State) ||
                            _context.WorkflowInstances.Any(wi => wi.WorkFlowID == g.GoodsConsignedID && wi.ProcessName == "GoodsConsignedWorkflow" && wi.AssignedUserID == userid)))
                        .Select(u => new IndexGoodsConsignedVM
                        {
                            GoodsConsignedID = u.GoodsConsignedID,
                            State = _context.WorkflowSteps
                                .Where(p => p.SectionCode == u.State && p.ProcessName == "GoodsConsignedWorkflow")
                                .Select(p => p.SectionName)
                                .FirstOrDefault() ?? u.State,
                            S_persiandate = u.S_persiandate,
                            TypeSending=u.TypeSending,
                            Guid = u.Guid,
                            SourceTable = 1
                        }).OrderByDescending(u => u.GoodsConsignedID);
                }
            }
            return await model1.ToDataSourceResultAsync(request);
        }

        public async Task<IActionResult> GoodsConsignedView(int? id)
        {
            GoodsConsigned model;
            var username = User.Identity.Name;
            var userid = await _context.Users
                .Where(p => p.UserName == username)
                .Select(j => j.Id)
                .FirstOrDefaultAsync();

            var workflowSteps = await _context.WorkflowSteps
                .Where(ws => ws.ProcessName == "GoodsConsignedWorkflow")
                .OrderBy(ws => ws.OrderIndex)
                .ToListAsync();

            if (id.HasValue && id > 0)
            {
                // درست: فقط از Navigation Propertyهایی استفاده کن که virtual و ICollection هستن
                model = await _context.GoodsConsigneds
                    .Include(g => g.ReturnedItems)      // درست
                    .FirstOrDefaultAsync(g => g.GoodsConsignedID == id);

                if (model == null)
                    return NotFound();

                ViewBag.Guid = model.Guid;

                var currentUserId = userid;
                var currentState = model.State ?? "start0";
                bool isSuperAdminOrManagement = User.IsInRole("SuperAdmin") || User.IsInRole("Management");

                var workflowHistory = await _context.WorkflowInstances
                    .Where(w => w.WorkFlowID == id.Value && w.ProcessName == "GoodsConsignedWorkflow")
                    .OrderBy(w => w.AssignedDate)
                    .ToListAsync();

                var completedSections = workflowHistory
                    .Where(w => w.IsCompleted)
                    .Select(w => w.Section)
                    .Distinct()
                    .ToList();

                if (!completedSections.Contains(currentState) && currentState != "End9")
                    completedSections.Add(currentState);

                var validStages = completedSections.ToList();

                if (currentState == "End9")
                {
                    validStages = completedSections
                        .Where(s => workflowSteps.Any(ws => ws.SectionCode == s))
                        .ToList();
                }

                var isSuccessfulCompletion = currentState == "End9" &&
                                             workflowHistory.Any(w => w.Section == "Negahbani8");

                ViewBag.IsSuccessfulCompletion = isSuccessfulCompletion;
                ViewBag.CompletedSections = completedSections;
                ViewBag.ValidStages = validStages;

                var isObserver = await _context.WorkflowAccesses
                    .Where(a => a.UserID == currentUserId &&
                                a.ProcessName == "GoodsConsignedWorkflow" &&
                                a.Role == "Observer" &&
                                validStages.Contains(a.Section))
                    .AnyAsync();

                var isAssignedUser = await _context.WorkflowInstances
                    .AnyAsync(w => w.WorkFlowID == id.Value &&
                                  w.AssignedUserID == currentUserId &&
                                  w.ProcessName == "GoodsConsignedWorkflow");

                var isStartUser = await _context.WorkflowInstances
                    .AnyAsync(w => w.WorkFlowID == id.Value &&
                                  w.Section == "start0" &&
                                  w.AssignedUserID == currentUserId);

                if (isStartUser && currentState == "End9")
                    isObserver = true;

                ViewBag.IsObserver = isSuperAdminOrManagement || isObserver || isAssignedUser;

                var workflowInstance = await _context.WorkflowInstances
                    .Where(w => w.WorkFlowID == id.Value &&
                               w.Section == currentState &&
                               w.ProcessName == "GoodsConsignedWorkflow")
                    .OrderByDescending(w => w.AssignedDate)
                    .FirstOrDefaultAsync();

                var isEditorFromAccess = await _context.WorkflowAccesses
                    .AnyAsync(a => a.UserID == currentUserId &&
                                  a.ProcessName == "GoodsConsignedWorkflow" &&
                                  a.Role == "Editor" &&
                                  a.Section == currentState);

                var isEditorFromInstance = workflowInstance?.IsEditor == true &&
                                         workflowInstance.AssignedUserID == currentUserId;

                ViewBag.CanEdit = (isEditorFromAccess || isEditorFromInstance) &&
                                 currentState == workflowInstance?.Section &&
                                 currentState != "End9";

                ViewBag.WorkflowInstance = workflowInstance;
                ViewBag.WorkflowHistory = workflowHistory;

                var endReason = "";
                if (currentState == "End9" && !isSuccessfulCompletion)
                {
                    endReason = model.TayidMali == "1" ? "عدم تأیید مالی" : "";
                }
                ViewBag.EndReason = endReason;
            }
            else
            {
                model = new GoodsConsigned
                {
                    GoodsConsignedID = 0,
                    State = "start0",
                    S_persiandate = DateTime.Now.ToPersianDateTime().ToString("yyyy/MM/dd"),
                    Guid = Guid.NewGuid().ToString()
                };

                ViewBag.Guid = model.Guid;
                ViewBag.CanEdit = true;
                ViewBag.IsObserver = false;
                ViewBag.WorkflowInstance = null;
                ViewBag.WorkflowHistory = null;
                ViewBag.IsSuccessfulCompletion = false;
                ViewBag.EndReason = "";
            }

            ViewBag.WorkflowSteps = workflowSteps;
            return View(model);
        }

        // درخواست کننده
        [HttpPost]
        public async Task<IActionResult> SaveForm1([FromBody] SaveForm1 dto)
        {
            var username = User.Identity.Name;
            var userid = await _context.Users.Where(p => p.UserName == username).Select(j => j.Id).SingleOrDefaultAsync();
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "خطا در اعتبارسنجی داده‌ها." });
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var model = new GoodsConsigned()
                    {
                        S_persiandate = dto.S_persiandate,
                        Shomare_Serial = dto.Shomare_Serial,
                        Darkhast_persiandate = dto.Darkhast_persiandate,
                        Name_Darkhastkonnande = dto.Name_Darkhastkonnande,
                        MahalDaryaft = dto.MahalDaryaft,
                        ConsignedItem = dto.Goods,
                        TypeSending = dto.TypeSending,    // همین یک خط کافیه!
                                                          //
                                                          // State = "TayidMasool1",
                        Guid = dto.Guid
                    };
                    


                        _context.GoodsConsigneds.Add(model);
                    await _context.SaveChangesAsync();

                    var workflowInstance = new WorkflowInstance
                    {
                        WorkFlowID = model.GoodsConsignedID,
                        ProcessName = "GoodsConsignedWorkflow",
                        Section = "start0",
                        AssignedUserID = userid,
                        IsEditor = false,
                        AssignedDate = DateTime.Now,
                        IsCompleted = true
                    };
                    _context.WorkflowInstances.Add(workflowInstance);

                    var currentStep = await _context.WorkflowSteps
                        .Where(ws => ws.ProcessName == "GoodsConsignedWorkflow" && ws.SectionCode == "start0")
                        .FirstOrDefaultAsync();
                    var nextSteps = JsonSerializer.Deserialize<Dictionary<string, string>>(currentStep.NextSteps);
                    //سوال
                    model.State = nextSteps.TryGetValue("next", out var nextStep) ? nextStep : "TayidMasool1";

                    //var workflowInstance1 = new WorkflowInstance
                    //{
                    //    WorkFlowID = model.GoodsConsignedID,
                    //    ProcessName = "GoodsConsignedWorkflow",
                    //    Section = model.State,
                    //    AssignedUserID = GetNextAssignedUser(model.State),
                    //    IsEditor = true,
                    //    AssignedDate = DateTime.Now,
                    //    IsCompleted = false
                    //};
                    //_context.WorkflowInstances.Add(workflowInstance1);

                    var workflowInstance1 = new WorkflowInstance
                    {
                        WorkFlowID = model.GoodsConsignedID,
                        ProcessName = "GoodsConsignedWorkflow",
                        Section = model.State,
                        AssignedUserID = dto.ManagerUserID,
                        IsEditor = true,
                        AssignedDate = DateTime.Now,
                        IsCompleted = false
                    };
                    _context.WorkflowInstances.Add(workflowInstance1);

                    NotifyUser(userid, dto.ManagerUserID, dto.Guid, "تایید مدیر واحد");

                    var filelist = await _context.FileDBs.Where(x => x.Guid == dto.Guid).ToListAsync();
                    foreach (var item in filelist)
                    {
                        if (item.IsTemp == true)
                        {
                            item.IsTemp = false;
                            _context.FileDBs.Update(item);
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return Json(new { success = true, message = "داده‌ها با موفقیت ذخیره شدند.", id = model.GoodsConsignedID });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error saving form1");
                    return Json(new { success = false, message = "خطا در ذخیره داده‌ها: " + ex.Message });
                }
            }
        }

        [HttpPost] // مسئول واحد
        public async Task<IActionResult> SaveForm2([FromBody] SaveForm2 dto)
        {
            var username = User.Identity.Name;
            var userid = await _context.Users.Where(p => p.UserName == username).Select(j => j.Id).SingleOrDefaultAsync();

            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage).ToList();
                return Json(new { success = false, message = "خطا در اعتبارسنجی داده‌ها: " + string.Join(", ", errors) });
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // پیدا کردن رکورد GoodsConsigned
                    var model = await _context.GoodsConsigneds
                        .FirstOrDefaultAsync(g => g.GoodsConsignedID == dto.GoodsConsignedID && g.Guid == dto.Guid);

                    if (model == null)
                    {
                        return Json(new { success = false, message = "رکورد موردنظر یافت نشد." });
                    }

                    // به‌روزرسانی فیلدهای مرحله دوم
                    model.TayidMasool = dto.TayidMasool;
                    model.Tozihat_Masool = dto.Tozihat_Masool;

                    // به‌روزرسانی وضعیت فعلی
                    var currentStep = await _context.WorkflowSteps
                        .Where(ws => ws.ProcessName == "GoodsConsignedWorkflow" && ws.SectionCode == "TayidMasool1")
                        .FirstOrDefaultAsync();
                    if (currentStep == null)
                    {
                        return Json(new { success = false, message = "مرحله جریان کاری یافت نشد." });
                    }

                    // علامت‌گذاری مرحله فعلی به‌عنوان کامل‌شده
                    var currentWorkflowInstance = await _context.WorkflowInstances
                        .FirstOrDefaultAsync(wi => wi.WorkFlowID == model.GoodsConsignedID && wi.Section == "TayidMasool1" && !wi.IsCompleted);
                    if (currentWorkflowInstance != null)
                    {
                        currentWorkflowInstance.IsCompleted = true;
                        _context.WorkflowInstances.Update(currentWorkflowInstance);
                    }

                    // تعیین مرحله بعدی
                    var nextSteps = JsonSerializer.Deserialize<Dictionary<string, string>>(currentStep.NextSteps);
                    model.State = nextSteps.TryGetValue("next", out var nextStep) ? nextStep : "TayidSanaye2"; // مرحله بعدی (مثلاً واحد صنایع)

                    // تخصیص کاربر برای مرحله بعدی
                    var nextUserId = GetNextAssignedUser(model.State);
                    if (nextUserId == null)
                    {
                        _logger.LogWarning($"No user found for section {model.State} with role Editor.");
                        return Json(new { success = false, message = "کاربری برای تخصیص در مرحله بعدی یافت نشد." });
                    }

                    // ایجاد WorkflowInstance جدید برای مرحله بعدی
                    var workflowInstance = new WorkflowInstance
                    {
                        WorkFlowID = model.GoodsConsignedID,
                        ProcessName = "GoodsConsignedWorkflow",
                        Section = model.State,
                        AssignedUserID = nextUserId,
                        IsEditor = false,
                        AssignedDate = DateTime.Now,
                        IsCompleted = false
                    };
                    _context.WorkflowInstances.Add(workflowInstance);

                    // ارسال اعلان به کاربر بعدی
                    NotifyUser(userid, nextUserId, dto.Guid, "بررسی واحد صنایع");
                    NotifyObservers(model.State, userid, dto.Guid);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return Json(new { success = true, message = "داده‌ها با موفقیت ذخیره شدند.", id = model.GoodsConsignedID });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error saving form2");
                    return Json(new { success = false, message = "خطا در ذخیره داده‌ها: " + ex.Message });
                }
            }
        }


        [HttpPost] //صنایع
        public async Task<IActionResult> SaveForm3([FromBody] SaveForm3 dto)
        {
            var username = User.Identity.Name;
            var userid = await _context.Users.Where(p => p.UserName == username).Select(j => j.Id).SingleOrDefaultAsync();

            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage).ToList();
                return Json(new { success = false, message = "خطا در اعتبارسنجی داده‌ها: " + string.Join(", ", errors) });
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // پیدا کردن رکورد GoodsConsigned
                    var model = await _context.GoodsConsigneds
                        .FirstOrDefaultAsync(g => g.GoodsConsignedID == dto.GoodsConsignedID && g.Guid == dto.Guid);

                    if (model == null)
                    {
                        return Json(new { success = false, message = "رکورد موردنظر یافت نشد." });
                    }

                    // به‌روزرسانی فیلدهای مرحله دوم
                    model.Tayid_Sanaye = dto.Tayid_Sanaye;
                    model.Tozihat_TayidSanaye = dto.Tozihat_TayidSanaye;

                    // به‌روزرسانی وضعیت فعلی
                    var currentStep = await _context.WorkflowSteps
                        .Where(ws => ws.ProcessName == "GoodsConsignedWorkflow" && ws.SectionCode == "TayidSanaye2")
                        .FirstOrDefaultAsync();
                    if (currentStep == null)
                    {
                        return Json(new { success = false, message = "مرحله جریان کاری یافت نشد." });
                    }

                    // علامت‌گذاری مرحله فعلی به‌عنوان کامل‌شده
                    var currentWorkflowInstance = await _context.WorkflowInstances
                        .FirstOrDefaultAsync(wi => wi.WorkFlowID == model.GoodsConsignedID && wi.Section == "TayidSanaye2" && !wi.IsCompleted);
                    if (currentWorkflowInstance != null)
                    {
                        currentWorkflowInstance.IsCompleted = true;
                        _context.WorkflowInstances.Update(currentWorkflowInstance);
                    }
                    // تعیین مرحله بعدی
                    var nextSteps = JsonSerializer.Deserialize<Dictionary<string, string>>(currentStep.NextSteps);
                    model.State = nextSteps.TryGetValue($"TayidSanaye2 == {dto.Tayid_Sanaye}", out var nextStep) ? nextStep : (dto.Tayid_Sanaye == "0" ? "Net3" : "TayidTadarokat4");


                    // تخصیص کاربر برای مرحله بعدی
                    var nextUserId = GetNextAssignedUser(model.State);
                    if (nextUserId == null)
                    {
                        _logger.LogWarning($"No user found for section {model.State} with role Editor.");
                        return Json(new { success = false, message = "کاربری برای تخصیص در مرحله بعدی یافت نشد." });
                    }

                    // ایجاد WorkflowInstance جدید برای مرحله بعدی
                    var workflowInstance = new WorkflowInstance
                    {
                        WorkFlowID = model.GoodsConsignedID,
                        ProcessName = "GoodsConsignedWorkflow",
                        Section = model.State,
                        AssignedUserID = nextUserId,
                        IsEditor = false,
                        AssignedDate = DateTime.Now,
                        IsCompleted = false
                    };
                    _context.WorkflowInstances.Add(workflowInstance);

                    // ارسال اعلان به کاربر بعدی
                    NotifyUser(userid, nextUserId, dto.Guid, "بررسی واحد");
                    NotifyObservers(model.State, userid, dto.Guid);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return Json(new { success = true, message = "داده‌ها با موفقیت ذخیره شدند.", id = model.GoodsConsignedID });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error saving form2");
                    return Json(new { success = false, message = "خطا در ذخیره داده‌ها: " + ex.Message });
                }
            }
        }


        [HttpPost] // نت
        public async Task<IActionResult> SaveForm4([FromBody] SaveForm4 dto)
        {
            var username = User.Identity.Name;
            var userid = await _context.Users.Where(p => p.UserName == username).Select(j => j.Id).SingleOrDefaultAsync();

            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage).ToList();
                return Json(new { success = false, message = "خطا در اعتبارسنجی داده‌ها: " + string.Join(", ", errors) });
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // پیدا کردن رکورد GoodsConsigned
                    var model = await _context.GoodsConsigneds
                        .FirstOrDefaultAsync(g => g.GoodsConsignedID == dto.GoodsConsignedID && g.Guid == dto.Guid);

                    if (model == null)
                    {
                        return Json(new { success = false, message = "رکورد موردنظر یافت نشد." });
                    }

                    // به‌روزرسانی فیلدهای مرحله دوم
                    model.Tozihat_Net = dto.Tozihat_Net;

                    // به‌روزرسانی وضعیت فعلی
                    var currentStep = await _context.WorkflowSteps
                        .Where(ws => ws.ProcessName == "GoodsConsignedWorkflow" && ws.SectionCode == "Net3")
                        .FirstOrDefaultAsync();
                    if (currentStep == null)
                    {
                        return Json(new { success = false, message = "مرحله جریان کاری یافت نشد." });
                    }

                    // علامت‌گذاری مرحله فعلی به‌عنوان کامل‌شده
                    var currentWorkflowInstance = await _context.WorkflowInstances
                        .FirstOrDefaultAsync(wi => wi.WorkFlowID == model.GoodsConsignedID && wi.Section == "Net3" && !wi.IsCompleted);
                    if (currentWorkflowInstance != null)
                    {
                        currentWorkflowInstance.IsCompleted = true;
                        _context.WorkflowInstances.Update(currentWorkflowInstance);
                    }

                    // تعیین مرحله بعدی
                    var nextSteps = JsonSerializer.Deserialize<Dictionary<string, string>>(currentStep.NextSteps);
                    model.State = nextSteps.TryGetValue("next", out var nextStep) ? nextStep : "TayidTadarokat4"; // مرحله بعدی (مثلاً واحد صنایع)

                    // تخصیص کاربر برای مرحله بعدی
                    var nextUserId = GetNextAssignedUser(model.State);
                    if (nextUserId == null)
                    {
                        _logger.LogWarning($"No user found for section {model.State} with role Editor.");
                        return Json(new { success = false, message = "کاربری برای تخصیص در مرحله بعدی یافت نشد." });
                    }

                    // ایجاد WorkflowInstance جدید برای مرحله بعدی
                    var workflowInstance = new WorkflowInstance
                    {
                        WorkFlowID = model.GoodsConsignedID,
                        ProcessName = "GoodsConsignedWorkflow",
                        Section = model.State,
                        AssignedUserID = nextUserId,
                        IsEditor = false,
                        AssignedDate = DateTime.Now,
                        IsCompleted = false
                    };
                    _context.WorkflowInstances.Add(workflowInstance);

                    // ارسال اعلان به کاربر بعدی
                    NotifyUser(userid, nextUserId, dto.Guid, "بررسی واحد تدارکات");
                    NotifyObservers(model.State, userid, dto.Guid);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return Json(new { success = true, message = "داده‌ها با موفقیت ذخیره شدند.", id = model.GoodsConsignedID });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error saving form2");
                    return Json(new { success = false, message = "خطا در ذخیره داده‌ها: " + ex.Message });
                }
            }
        }

        [HttpPost] // تدارکات
        public async Task<IActionResult> SaveForm5([FromBody] SaveForm5 dto)
        {
            // تایید تدارکات
            var username = User.Identity.Name;
            var userid = await _context.Users.Where(p => p.UserName == username).Select(j => j.Id).SingleOrDefaultAsync();

            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage).ToList();
                return Json(new { success = false, message = "خطا در اعتبارسنجی داده‌ها: " + string.Join(", ", errors) });
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // پیدا کردن رکورد GoodsConsigned
                    var model = await _context.GoodsConsigneds
                        .FirstOrDefaultAsync(g => g.GoodsConsignedID == dto.GoodsConsignedID && g.Guid == dto.Guid);

                    if (model == null)
                    {
                        return Json(new { success = false, message = "رکورد موردنظر یافت نشد." });
                    }

                    // به‌روزرسانی فیلدهای مرحله دوم
                    model.Tozihat_Tadarokat = dto.Tozihat_Tadarokat;

                    // به‌روزرسانی وضعیت فعلی
                    var currentStep = await _context.WorkflowSteps
                        .Where(ws => ws.ProcessName == "GoodsConsignedWorkflow" && ws.SectionCode == "TayidTadarokat4")
                        .FirstOrDefaultAsync();
                    if (currentStep == null)
                    {
                        return Json(new { success = false, message = "مرحله جریان کاری یافت نشد." });
                    }

                    // علامت‌گذاری مرحله فعلی به‌عنوان کامل‌شده
                    var currentWorkflowInstance = await _context.WorkflowInstances
                        .FirstOrDefaultAsync(wi => wi.WorkFlowID == model.GoodsConsignedID && wi.Section == "TayidTadarokat4" && !wi.IsCompleted);
                    if (currentWorkflowInstance != null)
                    {
                        currentWorkflowInstance.IsCompleted = true;
                        _context.WorkflowInstances.Update(currentWorkflowInstance);
                    }

                    // تعیین مرحله بعدی
                    var nextSteps = JsonSerializer.Deserialize<Dictionary<string, string>>(currentStep.NextSteps);
                    model.State = nextSteps.TryGetValue("next", out var nextStep) ? nextStep : "Mali5"; // مرحله بعدی (مثلاً واحد صنایع)

                    // تخصیص کاربر برای مرحله بعدی
                    var nextUserId = GetNextAssignedUser(model.State);
                    if (nextUserId == null)
                    {
                        _logger.LogWarning($"No user found for section {model.State} with role Editor.");
                        return Json(new { success = false, message = "کاربری برای تخصیص در مرحله بعدی یافت نشد." });
                    }

                    // ایجاد WorkflowInstance جدید برای مرحله بعدی
                    var workflowInstance = new WorkflowInstance
                    {
                        WorkFlowID = model.GoodsConsignedID,
                        ProcessName = "GoodsConsignedWorkflow",
                        Section = model.State,
                        AssignedUserID = nextUserId,
                        IsEditor = false,
                        AssignedDate = DateTime.Now,
                        IsCompleted = false
                    };
                    _context.WorkflowInstances.Add(workflowInstance);

                    // ارسال اعلان به کاربر بعدی
                    NotifyUser(userid, nextUserId, dto.Guid, "بررسی");
                    NotifyObservers(model.State, userid, dto.Guid);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return Json(new { success = true, message = "داده‌ها با موفقیت ذخیره شدند.", id = model.GoodsConsignedID });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error saving form2");
                    return Json(new { success = false, message = "خطا در ذخیره داده‌ها: " + ex.Message });
                }
            }
        }

        [HttpPost] // تایید مالی
        public async Task<IActionResult> SaveForm6([FromBody] SaveForm6 dto)
        {
            var username = User.Identity.Name;
            var userid = await _context.Users.Where(p => p.UserName == username).Select(j => j.Id).SingleOrDefaultAsync();

            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage).ToList();
                return Json(new { success = false, message = "خطا در اعتبارسنجی داده‌ها: " + string.Join(", ", errors) });
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // پیدا کردن رکورد GoodsConsigned
                    var model = await _context.GoodsConsigneds
                        .FirstOrDefaultAsync(g => g.GoodsConsignedID == dto.GoodsConsignedID && g.Guid == dto.Guid);

                    if (model == null)
                    {
                        return Json(new { success = false, message = "رکورد موردنظر یافت نشد." });
                    }

                    // به‌روزرسانی فیلدهای مرحله دوم
                    model.TayidMali = dto.TayidMali;
                    model.Tozihat_Mali = dto.Tozihat_Mali;

                    // به‌روزرسانی وضعیت فعلی
                    var currentStep = await _context.WorkflowSteps
                        .Where(ws => ws.ProcessName == "GoodsConsignedWorkflow" && ws.SectionCode == "Mali5")
                        .FirstOrDefaultAsync();
                    if (currentStep == null)
                    {
                        return Json(new { success = false, message = "مرحله جریان کاری یافت نشد." });
                    }

                    // علامت‌گذاری مرحله فعلی به‌عنوان کامل‌شده
                    var currentWorkflowInstance = await _context.WorkflowInstances
                        .FirstOrDefaultAsync(wi => wi.WorkFlowID == model.GoodsConsignedID && wi.Section == "Mali5" && !wi.IsCompleted);
                    if (currentWorkflowInstance != null)
                    {
                        currentWorkflowInstance.IsCompleted = true;
                        _context.WorkflowInstances.Update(currentWorkflowInstance);
                    }

                    // تعیین مرحله بعدی
                    //پیدا کردن مرجله بعدی با جیسون

                    // تعیین مرحله بعدی
                    var nextSteps = JsonSerializer.Deserialize<Dictionary<string, string>>(currentStep.NextSteps);
                    model.State = nextSteps.TryGetValue($"Mali5 == {dto.TayidMali}", out var nextStep) ? nextStep : (dto.TayidMali == "0" ? "6" : "TayidTadarokat4");


                    // تخصیص کاربر برای مرحله بعدی
                    var nextUserId = GetNextAssignedUser(model.State);
                    if (nextUserId == null)
                    {
                        _logger.LogWarning($"No user found for section {model.State} with role Editor.");
                        return Json(new { success = false, message = "کاربری برای تخصیص در مرحله بعدی یافت نشد." });
                    }

                    // ایجاد WorkflowInstance جدید برای مرحله بعدی
                    var workflowInstance = new WorkflowInstance
                    {
                        WorkFlowID = model.GoodsConsignedID,
                        ProcessName = "GoodsConsignedWorkflow",
                        Section = model.State,
                        AssignedUserID = nextUserId,
                        IsEditor = false,
                        AssignedDate = DateTime.Now,
                        IsCompleted = false
                    };
                    _context.WorkflowInstances.Add(workflowInstance);

                    // ارسال اعلان به کاربر بعدی
                    NotifyUser(userid, nextUserId, dto.Guid, "بررسی واحد انبار");
                    NotifyObservers(model.State, userid, dto.Guid);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return Json(new { success = true, message = "داده‌ها با موفقیت ذخیره شدند.", id = model.GoodsConsignedID });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error saving form2");
                    return Json(new { success = false, message = "خطا در ذخیره داده‌ها: " + ex.Message });
                }
            }
        }
        [HttpPost] // انبار
        public async Task<IActionResult> SaveForm8([FromBody] SaveForm8 dto)
        {
            var username = User.Identity.Name;
            var userid = await _context.Users.Where(p => p.UserName == username).Select(j => j.Id).SingleOrDefaultAsync();

            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage).ToList();
                return Json(new { success = false, message = "خطا در اعتبارسنجی داده‌ها: " + string.Join(", ", errors) });
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // پیدا کردن رکورد GoodsConsigned
                    var model = await _context.GoodsConsigneds
                        .FirstOrDefaultAsync(g => g.GoodsConsignedID == dto.GoodsConsignedID && g.Guid == dto.Guid);

                    if (model == null)
                    {
                        return Json(new { success = false, message = "رکورد موردنظر یافت نشد." });
                    }

                    // به‌روزرسانی فیلدهای مرحله دوم
                    model.Tozihat_Anbar = dto.Tozihat_Anbar;

                    // به‌روزرسانی وضعیت فعلی
                    var currentStep = await _context.WorkflowSteps
                        .Where(ws => ws.ProcessName == "GoodsConsignedWorkflow" && ws.SectionCode == "Anbar6")
                        .FirstOrDefaultAsync();
                    if (currentStep == null)
                    {
                        return Json(new { success = false, message = "مرحله جریان کاری یافت نشد." });
                    }

                    // علامت‌گذاری مرحله فعلی به‌عنوان کامل‌شده
                    var currentWorkflowInstance = await _context.WorkflowInstances
                        .FirstOrDefaultAsync(wi => wi.WorkFlowID == model.GoodsConsignedID && wi.Section == "Anbar6" && !wi.IsCompleted);
                    if (currentWorkflowInstance != null)
                    {
                        currentWorkflowInstance.IsCompleted = true;
                        _context.WorkflowInstances.Update(currentWorkflowInstance);
                    }

                    // تعیین مرحله بعدی
                    //پیدا کردن مرجله بعدی با جیسون

                    // تعیین مرحله بعدی
                    var nextSteps = JsonSerializer.Deserialize<Dictionary<string, string>>(currentStep.NextSteps);
                    model.State = nextSteps.TryGetValue("next", out var nextStep) ? nextStep : "Negahbani7"; // مرحله بعدی (مثلاً واحد صنایع)


                    // تخصیص کاربر برای مرحله بعدی
                    var nextUserId = GetNextAssignedUser(model.State);
                    if (nextUserId == null)
                    {
                        _logger.LogWarning($"No user found for section {model.State} with role Editor.");
                        return Json(new { success = false, message = "کاربری برای تخصیص در مرحله بعدی یافت نشد." });
                    }

                    // ایجاد WorkflowInstance جدید برای مرحله بعدی
                    var workflowInstance = new WorkflowInstance
                    {
                        WorkFlowID = model.GoodsConsignedID,
                        ProcessName = "GoodsConsignedWorkflow",
                        Section = model.State,
                        AssignedUserID = nextUserId,
                        IsEditor = false,
                        AssignedDate = DateTime.Now,
                        IsCompleted = false
                    };
                    _context.WorkflowInstances.Add(workflowInstance);

                    // ارسال اعلان به کاربر بعدی
                    NotifyUser(userid, nextUserId, dto.Guid, "بررسی نگهبانی");
                    NotifyObservers(model.State, userid, dto.Guid);

                    // جهت اطلاع شروع کننده فرآیند
                    var StartUserID = _context.WorkflowInstances.Where(g => g.WorkFlowID == dto.GoodsConsignedID && g.Section == "start0" && g.ProcessName == "GoodsConsignedWorkflow").Select(p => p.AssignedUserID).FirstOrDefault();
                    NotifyObservers(model.State, StartUserID, dto.Guid);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return Json(new { success = true, message = "داده‌ها با موفقیت ذخیره شدند.", id = model.GoodsConsignedID });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error saving form2");
                    return Json(new { success = false, message = "خطا در ذخیره داده‌ها: " + ex.Message });
                }
            }
        }
        
      
        [HttpPost] // نگهبانی (مرحله اول - قبلاً Negahbani7 بود)
        public async Task<IActionResult> SaveForm9([FromBody] SaveForm9 dto)
        {
            var username = User.Identity.Name;
            var userid = await _context.Users.Where(p => p.UserName == username).Select(j => j.Id).SingleOrDefaultAsync();

            if (!ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage).ToList();
                return Json(new { success = false, message = "خطا در اعتبارسنجی داده‌ها: " + string.Join(", ", errors) });
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var model = await _context.GoodsConsigneds
                        .FirstOrDefaultAsync(g => g.GoodsConsignedID == dto.GoodsConsignedID && g.Guid == dto.Guid);

                    if (model == null)
                        return Json(new { success = false, message = "رکورد موردنظر یافت نشد." });

                    // بروزرسانی فیلدهای نگهبانی اول
                    model.Khoruj_persiandate = dto.Khoruj_persiandate;
                    model.TimeKhoruj_persiandate = dto.TimeKhoruj_persiandate;
                    model.Tozihat_Negahbani = dto.Tozihat_Negahbani;

                    // علامت‌گذاری مرحله فعلی (Negahbani7) به عنوان تکمیل شده
                    var currentInstance = await _context.WorkflowInstances
                        .FirstOrDefaultAsync(wi => wi.WorkFlowID == model.GoodsConsignedID
                                                && wi.Section == "Negahbani7"
                                                && !wi.IsCompleted);

                    if (currentInstance != null)
                    {
                        currentInstance.IsCompleted = true;
                        _context.WorkflowInstances.Update(currentInstance);
                    }

                    // انتقال به مرحله جدید: Negahbani8
                    model.State = "Negahbani8";

                    var nextUserId = GetNextAssignedUser("Negahbani8");
                    if (nextUserId == null)
                        return Json(new { success = false, message = "کاربر نگهبانی مرحله دوم یافت نشد." });

                    var newInstance = new WorkflowInstance
                    {
                        WorkFlowID = model.GoodsConsignedID,
                        ProcessName = "GoodsConsignedWorkflow",
                        Section = "Negahbani8",
                        AssignedUserID = nextUserId,
                        IsEditor = true, // یا false بسته به نیاز
                        AssignedDate = DateTime.Now,
                        IsCompleted = false
                    };
                    _context.WorkflowInstances.Add(newInstance);

                    NotifyUser(userid, nextUserId, dto.Guid, "بررسی نهایی نگهبانی (مرحله دوم)");
                    NotifyObservers("Negahbani8", userid, dto.Guid);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Json(new { success = true, message = "به مرحله بررسی نهایی نگهبانی منتقل شد.", id = model.GoodsConsignedID });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error in SaveForm9 (Negahbani7 -> Negahbani8)");
                    return Json(new { success = false, message = "خطا: " + ex.Message });
                }
            }
        }

        [HttpPost] // نگهبانی مرحله دوم → پایان فرآیند
        public async Task<IActionResult> SaveForm10([FromBody] SaveForm10 dto)
        {
            var username = User.Identity.Name;
            var userid = await _context.Users.Where(p => p.UserName == username).Select(j => j.Id).SingleOrDefaultAsync();

            if (ModelState.IsValid)
            {
                var errors = ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage).ToList();
                return Json(new { success = false, message = "خطا در اعتبارسنجی: " + string.Join(", ", errors) });
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var model = await _context.GoodsConsigneds
                        .FirstOrDefaultAsync(g => g.GoodsConsignedID == dto.GoodsConsignedID && g.Guid == dto.Guid);

                    if (model == null)
                        return Json(new { success = false, message = "رکورد یافت نشد." });

                    // بروزرسانی فیلدهای نگهبانی اول
                    model.S_persiandate_Enteringtheguard = dto.S_persiandate_Enteringtheguard;
                    model.Shomare_Serial_Enteringtheguard = dto.Shomare_Serial_Enteringtheguard;
                    model.TahvilDahande_Enteringtheguard = dto.TahvilDahande_Enteringtheguard;
                    model.SaateVorud_Enteringtheguard = dto.SaateVorud_Enteringtheguard;
                    model.SaateKhoruj_Enteringtheguard = dto.SaateKhoruj_Enteringtheguard;
                    model.Khodro_Enteringtheguard = dto.Khodro_Enteringtheguard;
                    model.Phone_Enteringtheguard = dto.Phone_Enteringtheguard;

                    // پاک کردن اقلام قبلی (اگر وجود داشت)
                    var existingItems = await _context.GoodsConsignedItemGuards
                        .Where(x => x.GoodsConsignedID == model.GoodsConsignedID)
                        .ToListAsync();

                    if (existingItems.Any())
                    {
                        _context.GoodsConsignedItemGuards.RemoveRange(existingItems);
                    }

                    // بررسی وجود کالا
                    if (dto.GoodsGuard_Enteringtheguard == null || !dto.GoodsGuard_Enteringtheguard.Any())
                    {
                        await transaction.RollbackAsync();
                        return Json(new { success = false, message = "حداقل یک قلم کالا باید وارد شود." });
                    }

                    // اضافه کردن اقلام جدید
                    foreach (var item in dto.GoodsGuard_Enteringtheguard)
                    {
                        var goodsItem = new GoodsConsignedItemGuard
                        {
                            GoodsConsignedID = model.GoodsConsignedID,
                            SharhKala = item.SharhKala,
                            Tedad = item.Tedad,

                            Description = item.Description,
                            Guid = dto.Guid,
                        };

                        _context.GoodsConsignedItemGuards.Add(goodsItem);
                    }

                    // فقط یکبار SaveChanges در انتها (بعد از همه تغییرات)
                    await _context.SaveChangesAsync();


                    // تکمیل مرحله Negahbani8
                    var currentInstance = await _context.WorkflowInstances
                        .FirstOrDefaultAsync(wi => wi.WorkFlowID == model.GoodsConsignedID
                                                && wi.Section == "Negahbani8"
                                                && !wi.IsCompleted);

                    if (currentInstance != null)
                    {
                        currentInstance.IsCompleted = true;
                        _context.WorkflowInstances.Update(currentInstance);
                    }

                    // پایان فرآیند
                    model.State = "End9";

                    var endInstance = new WorkflowInstance
                    {
                        WorkFlowID = model.GoodsConsignedID,
                        ProcessName = "GoodsConsignedWorkflow",
                        Section = "End9",
                        AssignedUserID = userid,
                        IsEditor = false,
                        AssignedDate = DateTime.Now,
                        IsCompleted = true
                    };
                    _context.WorkflowInstances.Add(endInstance);

                    // اطلاع‌رسانی به درخواست‌کننده و تدارکات و ...
                    var startUserId = await _context.WorkflowInstances
                        .Where(w => w.WorkFlowID == model.GoodsConsignedID && w.Section == "start0")
                        .Select(w => w.AssignedUserID)
                        .FirstOrDefaultAsync();

                    if (startUserId != null)
                        NotifyUser(userid, startUserId, dto.Guid, "فرآیند ارسال کالای امانی با موفقیت به پایان رسید", "پایان فرآیند");

                    NotifyObservers("End9", userid, dto.Guid);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Json(new { success = true, message = "فرآیند با موفقیت به پایان رسید.", id = model.GoodsConsignedID });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error in SaveForm10 (Negahbani8 -> End9)");
                    return Json(new { success = false, message = "خطا: " + ex.Message });
                }
            }
        }
        private string GetNextAssignedUser(string state)
        {
            var nextAccess = _context.WorkflowAccesses
                .Where(a => a.ProcessName == "GoodsConsignedWorkflow" && a.Section == state && a.Role == "Editor")
                .Select(a => a.UserID)
                .FirstOrDefault();

            if (nextAccess == null)
            {
                _logger.LogWarning($"No user found for section {state} with role Editor.");
                return null; // Or a default user ID
            }

            return nextAccess;
        }

        private void NotifyObservers(string section, string senderId, string guid)
        {
            var observers = _context.WorkflowAccesses
                .Where(wa => wa.Role == "Observer" && wa.ProcessName == "GoodsConsignedWorkflow" && wa.Section == section)
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
                    Type = "ارسال کالای امانی",
                    Title = "جهت اطلاع"
                };
                _context.ViewProcesss.Add(observerNotification);
            }

            _context.SaveChanges();
        }

        private void NotifyUser(string senderId, string receiverId, string guid, string title, string type = "ارسال کالای امانی")
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

        public JsonResult DownloadFile(int id)
        {
            var goodsconsigned = _context.GoodsConsigneds.Find(id);
            var guid = goodsconsigned.Guid;
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