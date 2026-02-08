using ERP.Data;
using ERP.Models;
using ERP.ViewModels.GoodsDeparture;
using ERP.ViewModels.ViewProcess;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using ERP.ViewModels.WorkReport;

namespace ERP.Controllers
{
    [Authorize]
    public class GoodsDepartureController : Controller
    {
        private readonly ERPContext _context;
        private readonly ILogger<GoodsDepartureController> _logger;

        public GoodsDepartureController(ERPContext context, ILogger<GoodsDepartureController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var username = User.Identity.Name;
            var userid = await _context.Users.Where(p => p.UserName == username).Select(j => j.Id).FirstOrDefaultAsync();

            bool isSuperAdminOrManagement = User.IsInRole("SuperAdmin") || User.IsInRole("Management");
            IQueryable<int> accessibleGoodsDepartureIds;

            // گرفتن مراحل از WorkflowStep
            var workflowSteps = await _context.WorkflowSteps
                .Where(ws => ws.ProcessName == "GoodsDepartureWorkflow")
                .OrderBy(ws => ws.OrderIndex)
                .ToListAsync();

            if (isSuperAdminOrManagement)
            {
                accessibleGoodsDepartureIds = _context.GoodsDepartures.Select(g => g.GoodsDepartureID).Distinct();
            }
            else
            {
                var observerStages = await _context.WorkflowAccesses
                    .Where(a => a.UserID == userid && a.ProcessName == "GoodsDepartureWorkflow" && a.Role == "Observer")
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

                accessibleGoodsDepartureIds = _context.GoodsDepartures
                    .Where(g => accessibleStages.Contains(g.State) ||
                                _context.WorkflowInstances.Any(wi => wi.WorkFlowID == g.GoodsDepartureID && wi.ProcessName == "GoodsDepartureWorkflow" && wi.AssignedUserID == userid))
                    .Select(g => g.GoodsDepartureID)
                    .Distinct();
            }

            ViewBag.all = await _context.GoodsDepartures
                .Where(g => accessibleGoodsDepartureIds.Contains(g.GoodsDepartureID))
                .CountAsync();

            ViewBag.s1 = await _context.GoodsDepartures
                .Where(g => g.State == "TayidMali1" && accessibleGoodsDepartureIds.Contains(g.GoodsDepartureID))
                .CountAsync();
            ViewBag.s2 = await _context.GoodsDepartures
                .Where(g => g.State == "Negahbani2" && accessibleGoodsDepartureIds.Contains(g.GoodsDepartureID))
                .CountAsync();
            ViewBag.s3 = await _context.GoodsDepartures
                .Where(g => g.State == "End3" && accessibleGoodsDepartureIds.Contains(g.GoodsDepartureID))
                .CountAsync();

            return View();
        }

        public async Task<DataSourceResult> Data_GoodsDeparture([DataSourceRequest] DataSourceRequest request, string SelectTab)
        {
            var username = User.Identity.Name;
            var userid = await _context.Users.Where(p => p.UserName == username).Select(j => j.Id).FirstOrDefaultAsync();
            bool isSuperAdminOrManagement = User.IsInRole("SuperAdmin") || User.IsInRole("Management");

            var workflowSteps = await _context.WorkflowSteps
                .Where(ws => ws.ProcessName == "GoodsDepartureWorkflow")
                .OrderBy(ws => ws.OrderIndex)
                .ToListAsync();

            IQueryable<IndexGoodsDepartureViewModel> model1;
            if (isSuperAdminOrManagement)
            {
                if (SelectTab == "همه لیست")
                {
                    model1 = _context.GoodsDepartures
                        .Select(u => new IndexGoodsDepartureViewModel
                        {
                            GoodsDepartureID = u.GoodsDepartureID,
                            State = _context.WorkflowSteps
                                .Where(p => p.SectionCode == u.State && p.ProcessName == "GoodsDepartureWorkflow")
                                .Select(p => p.SectionName)
                                .FirstOrDefault() ?? u.State,
                            S_persiandate = u.S_persiandate,
                            Nam_TahvilGirandeh = u.Nam_TahvilGirandeh,
                            Guid = u.Guid,
                            Tozihat_Entezamat =u.Tozihat_Entezamat,
                            SourceTable = 0
                        }).OrderByDescending(u => u.GoodsDepartureID);
                }
                else
                {
                    var selectedState = SelectTab;
                    model1 = _context.GoodsDepartures
                        .Where(g => g.State == selectedState)
                        .Select(u => new IndexGoodsDepartureViewModel
                        {
                            GoodsDepartureID = u.GoodsDepartureID,
                            State = _context.WorkflowSteps
                                .Where(p => p.SectionCode == u.State && p.ProcessName == "GoodsDepartureWorkflow")
                                .Select(p => p.SectionName)
                                .FirstOrDefault() ?? u.State,
                            S_persiandate = u.S_persiandate,
                            Nam_TahvilGirandeh = u.Nam_TahvilGirandeh,
                            Guid = u.Guid,
                            Tozihat_Entezamat = u.Tozihat_Entezamat,
                            SourceTable = 1
                        }).OrderByDescending(u => u.GoodsDepartureID);
                }
            }
            else
            {
                var observerStages = await _context.WorkflowAccesses
                    .Where(a => a.UserID == userid && a.ProcessName == "GoodsDepartureWorkflow" && a.Role == "Observer")
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
                    model1 = _context.GoodsDepartures
                        .Where(g => accessibleStages.Contains(g.State) ||
                                    _context.WorkflowInstances.Any(wi => wi.WorkFlowID == g.GoodsDepartureID && wi.ProcessName == "GoodsDepartureWorkflow" && wi.AssignedUserID == userid))
                        .Select(u => new IndexGoodsDepartureViewModel
                        {
                            GoodsDepartureID = u.GoodsDepartureID,
                            State = _context.WorkflowSteps
                                .Where(p => p.SectionCode == u.State && p.ProcessName == "GoodsDepartureWorkflow")
                                .Select(p => p.SectionName)
                                .FirstOrDefault() ?? u.State,
                            S_persiandate = u.S_persiandate,
                            Nam_TahvilGirandeh = u.Nam_TahvilGirandeh,
                            Guid = u.Guid,
                            Tozihat_Entezamat = u.Tozihat_Entezamat,
                            SourceTable = 0
                        }).OrderByDescending(u => u.GoodsDepartureID);
                }
                else
                {
                    var selectedState = SelectTab;
                    model1 = _context.GoodsDepartures
                        .Where(g => g.State == selectedState && (
                            accessibleStages.Contains(g.State) ||
                            _context.WorkflowInstances.Any(wi => wi.WorkFlowID == g.GoodsDepartureID && wi.ProcessName == "GoodsDepartureWorkflow" && wi.AssignedUserID == userid)))
                        .Select(u => new IndexGoodsDepartureViewModel
                        {
                            GoodsDepartureID = u.GoodsDepartureID,
                            State = _context.WorkflowSteps
                                .Where(p => p.SectionCode == u.State && p.ProcessName == "GoodsDepartureWorkflow")
                                .Select(p => p.SectionName)
                                .FirstOrDefault() ?? u.State,
                            S_persiandate = u.S_persiandate,
                            Nam_TahvilGirandeh = u.Nam_TahvilGirandeh,
                            Guid = u.Guid,
                            Tozihat_Entezamat = u.Tozihat_Entezamat,
                            SourceTable = 1
                        }).OrderByDescending(u => u.GoodsDepartureID);
                }
            }
            return await model1.ToDataSourceResultAsync(request);
        }

        public async Task<IActionResult> GoodsDepartureView(int? id)
        {
            GoodsDeparture model;
            var username = User.Identity.Name;
            var userid = await _context.Users.Where(p => p.UserName == username).Select(j => j.Id).SingleOrDefaultAsync();

            var workflowSteps = await _context.WorkflowSteps
                .Where(ws => ws.ProcessName == "GoodsDepartureWorkflow")
                .OrderBy(ws => ws.OrderIndex)
                .ToListAsync();

            // لاگ برای دیباگ
            foreach (var step in workflowSteps)
            {
                _logger.LogInformation($"WorkflowStep: ID={step.WorkflowStepID}, SectionCode={step.SectionCode}, SectionName={step.SectionName}, NextSteps={step.NextSteps}");
            }

            // ادامه کد...
            if (id.HasValue && id > 0)
            {
                model = await _context.GoodsDepartures.FirstOrDefaultAsync(g => g.GoodsDepartureID == id);
                if (model == null)
                {
                    return NotFound();
                }
                ViewBag.Guid = model.Guid;

                var currentUserId = userid;
                var currentState = model.State ?? "start0";
                bool isSuperAdminOrManagement = User.IsInRole("SuperAdmin") || User.IsInRole("Management");

                var workflowHistory = await _context.WorkflowInstances
                    .Where(w => w.WorkFlowID == id.Value && w.ProcessName == "GoodsDepartureWorkflow")
                    .OrderBy(w => w.AssignedDate)
                    .ToListAsync();

                var completedSections = workflowHistory
                    .Where(w => w.IsCompleted)
                    .Select(w => w.Section)
                    .Distinct()
                    .ToList();

                if (!completedSections.Contains(currentState) && currentState != "End3")
                {
                    completedSections.Add(currentState);
                }

                var validStages = completedSections.ToList();
                if (currentState == "End3")
                {
                    validStages = completedSections
                        .Where(s => workflowSteps.Any(ws => ws.SectionCode == s))
                        .ToList();
                }

                var isSuccessfulCompletion = currentState == "End3" &&
                                             workflowHistory.Any(w => w.Section == "Negahbani2");
                ViewBag.IsSuccessfulCompletion = isSuccessfulCompletion;

                ViewBag.CompletedSections = completedSections;
                ViewBag.ValidStages = validStages;

                var isObserver = await _context.WorkflowAccesses
                    .Where(a => a.UserID == currentUserId &&
                                a.ProcessName == "GoodsDepartureWorkflow" &&
                                a.Role == "Observer" &&
                                validStages.Contains(a.Section))
                    .AnyAsync();

                var isAssignedUser = await _context.WorkflowInstances
                    .AnyAsync(w => w.WorkFlowID == id.Value &&
                                  w.AssignedUserID == currentUserId &&
                                  w.ProcessName == "GoodsDepartureWorkflow");

                var isStartUser = await _context.WorkflowInstances
                    .AnyAsync(w => w.WorkFlowID == id.Value &&
                                  w.Section == "start0" &&
                                  w.AssignedUserID == currentUserId);
                if (isStartUser && currentState == "End3")
                {
                    isObserver = true;
                }

                ViewBag.IsObserver = isSuperAdminOrManagement || isObserver || isAssignedUser;

                var workflowInstance = await _context.WorkflowInstances
                    .Where(w => w.WorkFlowID == id.Value &&
                               w.Section == currentState &&
                               w.ProcessName == "GoodsDepartureWorkflow")
                    .OrderByDescending(w => w.AssignedDate)
                    .FirstOrDefaultAsync();

                var isEditorFromAccess = await _context.WorkflowAccesses
                    .AnyAsync(a => a.UserID == currentUserId &&
                                  a.ProcessName == "GoodsDepartureWorkflow" &&
                                  a.Role == "Editor" &&
                                  a.Section == currentState);
                var isEditorFromInstance = workflowInstance?.IsEditor == true &&
                                         workflowInstance.AssignedUserID == currentUserId;

                ViewBag.CanEdit = (isEditorFromAccess || isEditorFromInstance) &&
                                 currentState == workflowInstance?.Section &&
                                 currentState != "End3";
                ViewBag.WorkflowInstance = workflowInstance;
                ViewBag.WorkflowHistory = workflowHistory;

                var endReason = "";
                if (currentState == "End3" && !isSuccessfulCompletion)
                {
                    endReason = model.Taeed == "1" ? "عدم تأیید مالی" : "";
                }
                ViewBag.EndReason = endReason;
            }
            else
            {
                model = new GoodsDeparture
                {
                    GoodsDepartureID = 0,
                    State = "start0",
                    S_persiandate = DateTime.Now.ToPersianDateTime().ToString("yyyy/MM/dd"),
                };
                ViewBag.Guid = Guid.NewGuid().ToString();
                ViewBag.CanEdit = User.Identity.IsAuthenticated;
                ViewBag.IsObserver = false;
                ViewBag.WorkflowInstance = null;
                ViewBag.WorkflowHistory = null;
                ViewBag.IsSuccessfulCompletion = false;
                ViewBag.EndReason = "";
            }

            ViewBag.WorkflowSteps = workflowSteps;
            return View(model);
        }

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
                    var model = new GoodsDeparture
                    {
                        S_persiandate = dto.S_persiandate,
                        Shomareh_Anabr = dto.Shomareh_Anabr,
                        Name_Anbar = dto.Name_Anbar,
                        Shomareh_havale = dto.Shomareh_havale,
                        Date_persiandate = dto.Date_persiandate,
                        Code_TahvilGirandeh = dto.Code_TahvilGirandeh,
                        Nam_TahvilGirandeh = dto.Nam_TahvilGirandeh,
                        Tozihat = dto.Tozihat,
                        State = "TayidMali1",
                        Guid = dto.Guid
                    };

                    _context.GoodsDepartures.Add(model);
                    await _context.SaveChangesAsync();

                    var workflowInstance = new WorkflowInstance
                    {
                        WorkFlowID = model.GoodsDepartureID,
                        ProcessName = "GoodsDepartureWorkflow",
                        Section = "start0",
                        AssignedUserID = userid,
                        IsEditor = false,
                        AssignedDate = DateTime.Now,
                        IsCompleted = true
                    };
                    _context.WorkflowInstances.Add(workflowInstance);

                    var currentStep = await _context.WorkflowSteps
                        .Where(ws => ws.ProcessName == "GoodsDepartureWorkflow" && ws.SectionCode == "start0")
                        .FirstOrDefaultAsync();
                    var nextSteps = JsonSerializer.Deserialize<Dictionary<string, string>>(currentStep.NextSteps);
                    model.State = nextSteps.TryGetValue("next", out var nextStep) ? nextStep : "TayidMali1";

                    var workflowInstance1 = new WorkflowInstance
                    {
                        WorkFlowID = model.GoodsDepartureID,
                        ProcessName = "GoodsDepartureWorkflow",
                        Section = model.State,
                        AssignedUserID = dto.FinanceUserID,
                        IsEditor = true,
                        AssignedDate = DateTime.Now,
                        IsCompleted = false
                    };
                    _context.WorkflowInstances.Add(workflowInstance1);

                    var filelist = await _context.FileDBs.Where(x => x.Guid == dto.Guid).ToListAsync();
                    foreach (var item in filelist)
                    {
                        if (item.IsTemp == true)
                        {
                            item.IsTemp = false;
                            _context.FileDBs.Update(item);
                        }
                    }

                    NotifyUser(userid, dto.FinanceUserID, dto.Guid, "بررسی خروج کالا");
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return Json(new { success = true, message = "داده‌ها با موفقیت ذخیره شدند.", id = model.GoodsDepartureID });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error saving form1");
                    return Json(new { success = false, message = "خطا در ذخیره داده‌ها: " + ex.Message });
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveForm2([FromBody] SaveForm2 dto)
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
                    var startUserID = await _context.WorkflowInstances
                        .Where(g => g.WorkFlowID == dto.GoodsDepartureID &&
                                    g.Section == "start0" &&
                                    g.ProcessName == "GoodsDepartureWorkflow")
                        .Select(p => p.AssignedUserID)
                        .SingleOrDefaultAsync();
                    var model = await _context.GoodsDepartures.FirstOrDefaultAsync(g => g.GoodsDepartureID == dto.GoodsDepartureID);
                    if (model == null)
                    {
                        return Json(new { success = false, message = "رکورد یافت نشد." });
                    }

                    if (string.IsNullOrEmpty(dto.Taeed) || (dto.Taeed != "0" && dto.Taeed != "1"))
                    {
                        return Json(new { success = false, message = "مقدار تأیید مالی نامعتبر است." });
                    }

                    model.Taeed = dto.Taeed;
                    model.Tozihat_Mali = dto.Tozihat_Mali;

                    // گرفتن NextSteps و پارس JSON
                    var currentStep = await _context.WorkflowSteps
                        .Where(ws => ws.ProcessName == "GoodsDepartureWorkflow" && ws.SectionCode == "TayidMali1")
                        .FirstOrDefaultAsync();
                    if (currentStep == null || string.IsNullOrEmpty(currentStep.NextSteps))
                    {
                        return Json(new { success = false, message = "مرحله جریان کاری یافت نشد یا تنظیمات گیت‌وی نامعتبر است." });
                    }

                    var nextSteps = JsonSerializer.Deserialize<Dictionary<string, string>>(currentStep.NextSteps);
                    model.State = nextSteps.TryGetValue($"Taeed == {dto.Taeed}", out var nextStep) ? nextStep : (dto.Taeed == "0" ? "Negahbani2" : "End3");

                    var workflowInstance = await _context.WorkflowInstances
                        .FirstOrDefaultAsync(g => g.WorkFlowID == dto.GoodsDepartureID &&
                                                g.Section == "TayidMali1" &&
                                                g.ProcessName == "GoodsDepartureWorkflow");
                    if (workflowInstance == null)
                    {
                        await transaction.RollbackAsync();
                        return Json(new { success = false, message = "نمونه جریان کاری یافت نشد." });
                    }
                    workflowInstance.IsCompleted = true;
                    if (model.State == "End3")
                    {
                        var nextUserId = startUserID;
                        var workflowInstance1 = new WorkflowInstance
                        {
                            WorkFlowID = model.GoodsDepartureID,
                            ProcessName = "GoodsDepartureWorkflow",
                            Section = model.State,
                            AssignedUserID = nextUserId,
                            IsEditor = model.State != "End3",
                            AssignedDate = DateTime.Now,
                            IsCompleted = false
                        };
                        _context.WorkflowInstances.Add(workflowInstance1);
                        NotifyUser(userid, nextUserId, dto.Guid, "عدم تایید");
                    }
                    else
                    {
                        var nextUserId = GetNextAssignedUser(model.State);
                        var workflowInstance1 = new WorkflowInstance
                        {
                            WorkFlowID = model.GoodsDepartureID,
                            ProcessName = "GoodsDepartureWorkflow",
                            Section = model.State,
                            AssignedUserID = nextUserId,
                            IsEditor = model.State != "End3",
                            AssignedDate = DateTime.Now,
                            IsCompleted = false
                        };
                        _context.WorkflowInstances.Add(workflowInstance1);
                        NotifyUser(userid, nextUserId, dto.Guid, "ثبت نگهبانی");
                    }

                    NotifyObservers(model.State, userid, dto.Guid);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return Json(new { success = true, message = "داده‌ها با موفقیت ذخیره شدند." });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error saving form2");
                    return Json(new { success = false, message = "خطا در ذخیره داده‌ها: " + ex.Message });
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveForm3([FromBody] SaveForm3 dto)
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
                    var model = await _context.GoodsDepartures.FirstOrDefaultAsync(g => g.GoodsDepartureID == dto.GoodsDepartureID);
                    if (model == null)
                    {
                        return Json(new { success = false, message = "رکورد یافت نشد." });
                    }

                    model.IsBazrasi = dto.IsBazrasi;
                    model.Noe_Hamel = dto.Noe_Hamel;
                    model.VehicleExit = dto.VehicleExit ?? new List<VehicleExit>();
                    model.Tozihat_Entezamat = dto.Tozihat_Entezamat;


                    var currentStep = await _context.WorkflowSteps
                        .Where(ws => ws.ProcessName == "GoodsDepartureWorkflow" && ws.SectionCode == "Negahbani2")
                        .FirstOrDefaultAsync();
                    if (currentStep == null || string.IsNullOrEmpty(currentStep.NextSteps))
                    {
                        return Json(new { success = false, message = "مرحله جریان کاری یافت نشد یا تنظیمات گیت‌وی نامعتبر است." });
                    }

                    var nextSteps = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(currentStep.NextSteps);
                    model.State = nextSteps.TryGetValue("next", out var nextStep) ? nextStep : "End3";



                    var workflowInstance = await _context.WorkflowInstances
                        .FirstOrDefaultAsync(g => g.WorkFlowID == dto.GoodsDepartureID &&
                                                g.Section == "Negahbani2" &&
                                                g.ProcessName == "GoodsDepartureWorkflow");
                    if (workflowInstance == null)
                    {
                        await transaction.RollbackAsync();
                        return Json(new { success = false, message = "نمونه جریان کاری یافت نشد." });
                    }
                    workflowInstance.IsCompleted = true;

                    var startUserID = await _context.WorkflowInstances
                        .Where(g => g.WorkFlowID == dto.GoodsDepartureID &&
                                   g.Section == "start0" &&
                                   g.ProcessName == "GoodsDepartureWorkflow")
                        .Select(p => p.AssignedUserID)
                        .SingleOrDefaultAsync();

                    var workflowInstance1 = new WorkflowInstance
                    {
                        WorkFlowID = model.GoodsDepartureID,
                        ProcessName = "GoodsDepartureWorkflow",
                        Section = model.State,
                        AssignedUserID = startUserID,
                        IsEditor = false,
                        AssignedDate = DateTime.Now,
                        IsCompleted = true
                    };
                    _context.WorkflowInstances.Add(workflowInstance1);

                    NotifyUser(userid, startUserID, dto.Guid, "پایان");
                    NotifyObservers(model.State, userid, dto.Guid);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return Json(new { success = true, message = "داده‌ها با موفقیت ذخیره شدند." });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error saving form3");
                    return Json(new { success = false, message = "خطا در ذخیره داده‌ها: " + ex.Message });
                }
            }
        }


        private string GetNextAssignedUser(string state)
        {
            var nextAccess = _context.WorkflowAccesses
                .Where(a => a.ProcessName == "GoodsDepartureWorkflow" && a.Section == state && a.Role == "Editor")
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
                .Where(wa => wa.Role == "Observer" && wa.ProcessName == "GoodsDepartureWorkflow" && wa.Section == section)
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
                    Type = "خروج کالا",
                    Title = "جهت اطلاع"
                };
                _context.ViewProcesss.Add(observerNotification);
            }

            _context.SaveChanges();
        }

        private void NotifyUser(string senderId, string receiverId, string guid, string title, string type = "خروج کالا")
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
            var goodsdeparture = _context.GoodsDepartures.Find(id);
            var guid = goodsdeparture.Guid;
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
        public JsonResult GetFinanceUser()
        {
            var roleId = _context.Roles
                .Where(r => r.Name == "Finance")
                .Select(r => r.Id)
                .FirstOrDefault();

            var users = _context.UserRoles
                .Where(ur => ur.RoleId == roleId)
                .Join(_context.Users,
                    ur => ur.UserId,
                    u => u.Id,
                    (ur, u) => new
                    {
                        id = u.Id,
                        text = u.FirstName + " " + u.LastName
                    })
                .ToList();

            return Json(users);
        }
    }
}