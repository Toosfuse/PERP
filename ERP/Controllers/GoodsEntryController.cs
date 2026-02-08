using ERP.Data;
using ERP.Models;
using ERP.ViewModels.GoodsEntry;
using ERP.ViewModels.ViewProcess;
using ERP.ViewModels.WorkReport;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text.Json;


namespace ERP.Controllers
{
    [Authorize]
    public class GoodsEntryController : Controller
    {
        private readonly ERPContext _context;
        private readonly ILogger<GoodsEntryController> _logger;

        public GoodsEntryController(ERPContext context, ILogger<GoodsEntryController> logger)
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
                .Where(ws => ws.ProcessName == "GoodsEntryWorkflow")
                .OrderBy(ws => ws.OrderIndex)
                .ToListAsync();

            if (isSuperAdminOrManagement)
            {
                accessibleGoodsDepartureIds = _context.GoodsEntries.Select(g => g.GoodsEntryID).Distinct();
            }
            else
            {
                var observerStages = await _context.WorkflowAccesses
                    .Where(a => a.UserID == userid && a.ProcessName == "GoodsEntryWorkflow" && a.Role == "Observer")
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

                accessibleGoodsDepartureIds = _context.GoodsEntries
                    .Where(g => accessibleStages.Contains(g.State) ||
                                _context.WorkflowInstances.Any(wi => wi.WorkFlowID == g.GoodsEntryID && wi.ProcessName == "GoodsEntryWorkflow" && wi.AssignedUserID == userid))
                    .Select(g => g.GoodsEntryID)
                    .Distinct();
            }

            ViewBag.all = await _context.GoodsEntries
                .Where(g => accessibleGoodsDepartureIds.Contains(g.GoodsEntryID))
                .CountAsync();

            ViewBag.s1 = await _context.GoodsEntries
                .Where(g => g.State == "BarrasiKala1" && accessibleGoodsDepartureIds.Contains(g.GoodsEntryID))
                .CountAsync();
            ViewBag.s2 = await _context.GoodsEntries
                .Where(g => g.State == "ControlKeyfiat2" && accessibleGoodsDepartureIds.Contains(g.GoodsEntryID))
                .CountAsync();
            ViewBag.s3 = await _context.GoodsEntries
                .Where(g => g.State == "TazminKeyfiat3" && accessibleGoodsDepartureIds.Contains(g.GoodsEntryID))
                .CountAsync();
            ViewBag.s4 = await _context.GoodsEntries
                .Where(g => g.State == "Other4" && accessibleGoodsDepartureIds.Contains(g.GoodsEntryID))
                .CountAsync();
            ViewBag.s5 = await _context.GoodsEntries
                .Where(g => g.State == "BarrasiSanaye5" && accessibleGoodsDepartureIds.Contains(g.GoodsEntryID))
                .CountAsync();
            ViewBag.s6 = await _context.GoodsEntries
                .Where(g => g.State == "BaarasiTolid6" && accessibleGoodsDepartureIds.Contains(g.GoodsEntryID))
                .CountAsync();
            ViewBag.s7 = await _context.GoodsEntries
                .Where(g => g.State == "BaarasiKeyfiat7" && accessibleGoodsDepartureIds.Contains(g.GoodsEntryID))
                .CountAsync();
            ViewBag.s8 = await _context.GoodsEntries
                .Where(g => g.State == "Anbar8" && accessibleGoodsDepartureIds.Contains(g.GoodsEntryID))
                .CountAsync();
            ViewBag.s9 = await _context.GoodsEntries
                .Where(g => g.State == "End9" && accessibleGoodsDepartureIds.Contains(g.GoodsEntryID))
                .CountAsync();
            return View();
        }

        public async Task<DataSourceResult> Data_GoodsEntry([DataSourceRequest] DataSourceRequest request, string SelectTab)
        {
            var username = User.Identity.Name;
            var userid = await _context.Users.Where(p => p.UserName == username).Select(j => j.Id).FirstOrDefaultAsync();
            bool isSuperAdminOrManagement = User.IsInRole("SuperAdmin") || User.IsInRole("Management");

            var workflowSteps = await _context.WorkflowSteps
                .Where(ws => ws.ProcessName == "GoodsEntryWorkflow")
                .OrderBy(ws => ws.OrderIndex)
                .ToListAsync();

            IQueryable<IndexGoodsEntryViewModel> model1;
            if (isSuperAdminOrManagement)
            {
                if (SelectTab == "همه لیست")
                {
                    model1 = _context.GoodsEntries
                        .Select(u => new IndexGoodsEntryViewModel
                        {
                            GoodsEntryID = u.GoodsEntryID,
                            State = _context.WorkflowSteps
                                .Where(p => p.SectionCode == u.State && p.ProcessName == "GoodsEntryWorkflow")
                                .Select(p => p.SectionName)
                                .FirstOrDefault() ?? u.State,
                            S_persiandate = u.S_persiandate,
                            TahvilDahande=u.TahvilDahande,
                            Guid = u.Guid,
                            SourceTable = 0
                        }).OrderByDescending(u => u.GoodsEntryID);
                }
                else
                {
                    var selectedState = SelectTab;
                    model1 = _context.GoodsEntries
                        .Where(g => g.State == selectedState)
                        .Select(u => new IndexGoodsEntryViewModel
                        {
                            GoodsEntryID = u.GoodsEntryID,
                            State = _context.WorkflowSteps
                                .Where(p => p.SectionCode == u.State && p.ProcessName == "GoodsEntryWorkflow")
                                .Select(p => p.SectionName)
                                .FirstOrDefault() ?? u.State,
                            S_persiandate = u.S_persiandate,
                            TahvilDahande = u.TahvilDahande,
                            Guid = u.Guid,
                            SourceTable = 1
                        }).OrderByDescending(u => u.GoodsEntryID);
                }
            }
            else
            {
                var observerStages = await _context.WorkflowAccesses
                    .Where(a => a.UserID == userid && a.ProcessName == "GoodsEntryWorkflow" && a.Role == "Observer")
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
                    model1 = _context.GoodsEntries
                        .Where(g => accessibleStages.Contains(g.State) ||
                                    _context.WorkflowInstances.Any(wi => wi.WorkFlowID == g.GoodsEntryID && wi.ProcessName == "GoodsEntryWorkflow" && wi.AssignedUserID == userid))
                        .Select(u => new IndexGoodsEntryViewModel
                        {
                            GoodsEntryID = u.GoodsEntryID,
                            State = _context.WorkflowSteps
                                .Where(p => p.SectionCode == u.State && p.ProcessName == "GoodsEntryWorkflow")
                                .Select(p => p.SectionName)
                                .FirstOrDefault() ?? u.State,
                            S_persiandate = u.S_persiandate,
                            TahvilDahande = u.TahvilDahande,
                            Guid = u.Guid,
                            SourceTable = 0
                        }).OrderByDescending(u => u.GoodsEntryID);
                }
                else
                {
                    var selectedState = SelectTab;
                    model1 = _context.GoodsEntries
                        .Where(g => g.State == selectedState && (
                            accessibleStages.Contains(g.State) ||
                            _context.WorkflowInstances.Any(wi => wi.WorkFlowID == g.GoodsEntryID && wi.ProcessName == "GoodsEntryWorkflow" && wi.AssignedUserID == userid)))
                        .Select(u => new IndexGoodsEntryViewModel
                        {
                            GoodsEntryID = u.GoodsEntryID,
                            State = _context.WorkflowSteps
                                .Where(p => p.SectionCode == u.State && p.ProcessName == "GoodsEntryWorkflow")
                                .Select(p => p.SectionName)
                                .FirstOrDefault() ?? u.State,
                            S_persiandate = u.S_persiandate,
                            TahvilDahande = u.TahvilDahande,
                            Guid = u.Guid,
                            SourceTable = 1
                        }).OrderByDescending(u => u.GoodsEntryID);
                }
            }
            return await model1.ToDataSourceResultAsync(request);
        }



        public async Task<IActionResult> GoodsEntryView(int? id)
        {
            GoodsEntry model;
            var username = User.Identity.Name;
            var userid = await _context.Users.Where(p => p.UserName == username).Select(j => j.Id).SingleOrDefaultAsync();

            var workflowSteps = await _context.WorkflowSteps
                .Where(ws => ws.ProcessName == "GoodsEntryWorkflow")
                .OrderBy(ws => ws.OrderIndex)
                .ToListAsync();

            // لاگ برای دیباگ
            foreach (var step in workflowSteps)
            {
                _logger.LogInformation($"WorkflowStep: ID={step.WorkflowStepID}, SectionCode={step.SectionCode}, SectionName={step.SectionName}, NextSteps={step.NextSteps}");
            }

            if (id.HasValue && id > 0)
            {
                model = await _context.GoodsEntries.FirstOrDefaultAsync(g => g.GoodsEntryID == id);
                if (model == null)
                {
                    return NotFound();
                }
                ViewBag.Guid = model.Guid;

                var currentUserId = userid;
                var currentState = model.State ?? "start0";
                bool isSuperAdminOrManagement = User.IsInRole("SuperAdmin") || User.IsInRole("Management");

                var workflowHistory = await _context.WorkflowInstances
                    .Where(w => w.WorkFlowID == id.Value && w.ProcessName == "GoodsEntryWorkflow")
                    .OrderBy(w => w.AssignedDate)
                    .ToListAsync();

                var completedSections = workflowHistory
                    .Where(w => w.IsCompleted)
                    .Select(w => w.Section)
                    .Distinct()
                    .ToList();

                if (!completedSections.Contains(currentState) && currentState != "End9")
                {
                    completedSections.Add(currentState);
                }

                var validStages = completedSections.ToList();
                if (currentState == "End9")
                {
                    validStages = completedSections
                        .Where(s => workflowSteps.Any(ws => ws.SectionCode == s))
                        .ToList();
                }

                //var isSuccessfulCompletion = currentState == "End9" && workflowHistory.Any(w => w.Section == "Negahbani2");
                //ViewBag.IsSuccessfulCompletion = isSuccessfulCompletion;

                ViewBag.CompletedSections = completedSections;
                ViewBag.ValidStages = validStages;

                var isObserver = await _context.WorkflowAccesses
                    .Where(a => a.UserID == currentUserId &&
                                a.ProcessName == "GoodsEntryWorkflow" &&
                                a.Role == "Observer" &&
                                validStages.Contains(a.Section))
                    .AnyAsync();

                var isAssignedUser = await _context.WorkflowInstances
                    .AnyAsync(w => w.WorkFlowID == id.Value &&
                                  w.AssignedUserID == currentUserId &&
                                  w.ProcessName == "GoodsEntryWorkflow");

                var isStartUser = await _context.WorkflowInstances
                    .AnyAsync(w => w.WorkFlowID == id.Value &&
                                  w.Section == "start0" &&
                                  w.AssignedUserID == currentUserId);
                if (isStartUser && currentState == "End9")
                {
                    isObserver = true;
                }

                ViewBag.IsObserver = isSuperAdminOrManagement || isObserver || isAssignedUser;

                var workflowInstance = await _context.WorkflowInstances
                    .Where(w => w.WorkFlowID == id.Value &&
                               w.Section == currentState &&
                               w.ProcessName == "GoodsEntryWorkflow")
                    .OrderByDescending(w => w.AssignedDate)
                    .FirstOrDefaultAsync();

                var isEditorFromAccess = await _context.WorkflowAccesses
                    .AnyAsync(a => a.UserID == currentUserId &&
                                  a.ProcessName == "GoodsEntryWorkflow" &&
                                  a.Role == "Editor" &&
                                  a.Section == currentState);
                var isEditorFromInstance = workflowInstance?.IsEditor == true &&
                                         workflowInstance.AssignedUserID == currentUserId;

                ViewBag.CanEdit = (isEditorFromAccess || isEditorFromInstance) &&
                                 currentState == workflowInstance?.Section &&
                                 currentState != "End9";
                ViewBag.WorkflowInstance = workflowInstance;
                ViewBag.WorkflowHistory = workflowHistory;

                //var endReason = "";
                //if (currentState == "End9" && !isSuccessfulCompletion)
                //{
                //    endReason = model.Taeed == "1" ? "عدم تأیید مالی" : "";
                //}
                //ViewBag.EndReason = endReason;
            }
            else
            {
                model = new GoodsEntry()
                {
                    GoodsEntryID = 0,
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
                    // ایجاد GoodsEntry جدید
                    var model = new GoodsEntry
                    {
                        S_persiandate = dto.S_persiandate,
                        Shomare_Serial = dto.Shomare_Serial,
                        TahvilDahande = dto.TahvilDahande,
                        SaateVorud = dto.SaateVorud,
                        SaateKhoruj = dto.SaateKhoruj,
                        Khodro = dto.Khodro,
                        Phone = dto.Phone,
                        State = "BarrasiKala1",
                        Guid = dto.Guid
                    };
                    _context.GoodsEntries.Add(model);
                    await _context.SaveChangesAsync();

                    // ذخیره اقلام کالا
                    if (dto.GoodsGuard != null && dto.GoodsGuard.Any())
                    {
                        foreach (var item in dto.GoodsGuard)
                        {
                            var goodsItem = new GoodsEntryItemGuard
                            {
                                GoodsEntryID = model.GoodsEntryID,
                                SharhKala = item.SharhKala,
                                Tedad = item.Tedad,
                             
                                Description = item.Description,
                                Guid = dto.Guid
                            };
                            _context.GoodsEntryItemGuards.Add(goodsItem);
                        }
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                        return Json(new { success = false, message = "حداقل یک کالا باید وارد شود." });
                    }

                    // ثبت WorkflowInstance برای مرحله فعلی
                    var workflowInstance = new WorkflowInstance
                    {
                        WorkFlowID = model.GoodsEntryID,
                        ProcessName = "GoodsEntryWorkflow",
                        Section = "start0",
                        AssignedUserID = userid,
                        IsEditor = false,
                        AssignedDate = DateTime.Now,
                        IsCompleted = true
                    };
                    _context.WorkflowInstances.Add(workflowInstance);

                    // دریافت مرحله بعدی
                    var currentStep = await _context.WorkflowSteps
                        .Where(ws => ws.ProcessName == "GoodsEntryWorkflow" && ws.SectionCode == "start0")
                        .FirstOrDefaultAsync();
                    var nextSteps = JsonSerializer.Deserialize<Dictionary<string, string>>(currentStep.NextSteps);
                    model.State = nextSteps.TryGetValue("next", out var nextStep) ? nextStep : "BarrasiKala1";

                    // ثبت WorkflowInstance برای مرحله بعدی
                    var workflowInstance1 = new WorkflowInstance
                    {
                        WorkFlowID = model.GoodsEntryID,
                        ProcessName = "GoodsEntryWorkflow",
                        Section = model.State,
                        AssignedUserID = GetNextAssignedUser(model.State),
                        IsEditor = false,
                        AssignedDate = DateTime.Now,
                        IsCompleted = false
                    };
                    _context.WorkflowInstances.Add(workflowInstance1);

                    // به‌روزرسانی فایل‌های موقت
                    var filelist = await _context.FileDBs.Where(x => x.Guid == dto.Guid).ToListAsync();
                    foreach (var item in filelist)
                    {
                        if (item.IsTemp == true)
                        {
                            item.IsTemp = false;
                            _context.FileDBs.Update(item);
                        }
                    }

                    // ارسال اعلان
                    NotifyUser(userid, GetNextAssignedUser(model.State), dto.Guid, "بررسی ورود کالا");
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Json(new { success = true, message = "داده‌ها با موفقیت ذخیره شدند.", id = model.GoodsEntryID });
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
            var username = User.Identity?.Name;
            var userId = await _context.Users
                .Where(p => p.UserName == username)
                .Select(j => j.Id)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(username) || userId == default)
            {
                return Json(new { success = false, message = "کاربر معتبر یافت نشد." });
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var model = await _context.GoodsEntries
                        .FirstOrDefaultAsync(g => g.GoodsEntryID == dto.GoodsEntryID);

                    if (model == null)
                    {
                        return Json(new { success = false, message = "رکورد ورود کالا یافت نشد." });
                    }

                    // به‌روزرسانی مقادیر اصلی
                    model.NoeKala = dto.NoeKala;
                    model.BarrasiQC = dto.BarrasiQC;
                    model.TozihatAnbar = dto.TozihatAnbar;
                    model.S_persiandatetoQC = dto.S_persiandatetoQC;
                    model.ShomareBargasht = dto.ShomareBargasht;
                    model.Shomarekala = dto.Shomarekala;
                    model.ShomarePPAP = dto.ShomarePPAP;
                    model.SerialAnbar = dto.SerialAnbar;
                    model.Guid = dto.Guid;

                    // مدیریت لیست کالاها (GoodsEntryItems)
                    if (dto.Goods != null && dto.Goods.Any())
                    {
                        // حذف کالاهای قدیمی مرتبط با این GoodsEntryID
                        var oldItems = _context.GoodsEntryItems
                            .Where(g => g.GoodsEntryID == dto.GoodsEntryID)
                            .ToList();
                        _context.GoodsEntryItems.RemoveRange(oldItems);

                        // افزودن کالاهای جدید
                        foreach (var item in dto.Goods)
                        {
                            _context.GoodsEntryItems.Add(new GoodsEntryItem
                            {
                                GoodsEntryID = dto.GoodsEntryID,
                                NameKala = item.NameKala,
                                NameTaminkonandeh = item.NameTaminkonandeh,
                                ShonarehResid = item.ShonarehResid,
                                Tedad = item.Tedad,
                                Vahed = item.Vahed,
                                Parameter = item.Parameter,
                                Azmoon = item.Azmoon,
                                TedadNemoneh = item.TedadNemoneh,
                                Natigeh = item.Natigeh,
                                Tozihat = item.Tozihat,
                                Guid = item.Guid
                            });
                        }
                    }

                    // مدیریت جریان کاری
                    var currentWorkflow = await _context.WorkflowInstances
                        .FirstOrDefaultAsync(w => w.WorkFlowID == dto.GoodsEntryID &&
                                               w.Section == "BarrasiKala1" &&
                                               w.ProcessName == "GoodsEntryWorkflow");

                    if (currentWorkflow == null)
                    {
                        return Json(new { success = false, message = "جریان کاری فعلی یافت نشد." });
                    }

                    currentWorkflow.IsCompleted = true;


                    //پیدا کردن مرجله بعدی با جیسون
                    var currentStep = await _context.WorkflowSteps
                        .Where(ws => ws.ProcessName == "GoodsEntryWorkflow" && ws.SectionCode == "BarrasiKala1")
                        .FirstOrDefaultAsync();

                    if (currentStep == null || string.IsNullOrEmpty(currentStep.NextSteps))
                    {
                        return Json(new { success = false, message = "مرحله جریان کاری یافت نشد." });
                    }

                    var nextSteps = JsonSerializer.Deserialize<Dictionary<string, string>>(currentStep.NextSteps);
                    string nextSection;

                    if (dto.NoeKala == "0")
                    {
                        // کلید رو بر اساس مقدار واقعی BarrasiQC بساز
                        string qcCondition = dto.BarrasiQC == "0" ? "BarrasiQC == 0" : "BarrasiQC == 1";
                        string key = $"NoeKala == 0 && {qcCondition}";

                        nextSection = nextSteps.TryGetValue(key, out var section) ? section : "End9";
                    }
                    else
                    {
                        nextSection = nextSteps.TryGetValue($"NoeKala == {dto.NoeKala}", out var section) ? section : "End9";
                    }


                    model.State = nextSection;
                    var nextUserId =  GetNextAssignedUser2(nextSection,dto.UnitUserID);
                    var isEditor = GetNextAssignedUserEditor(nextSection);

                    var newWorkflowInstance = new WorkflowInstance
                    {
                        WorkFlowID = dto.GoodsEntryID,
                        ProcessName = "GoodsEntryWorkflow",
                        Section = nextSection,
                        AssignedUserID = nextUserId,
                        IsEditor = isEditor,
                        AssignedDate = DateTime.Now,
                        IsCompleted = false
                    };
                    _context.WorkflowInstances.Add(newWorkflowInstance);

                    // اطلاع‌رسانی
                    NotifyUser(userId, nextUserId, dto.Guid, "بررسی کالای ورودی");
                    NotifyObservers(nextSection, userId, dto.Guid);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Json(new { success = true, message = "داده‌ها با موفقیت ذخیره شدند." });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "خطا در ذخیره فرم 2: {Message}", ex.Message);
                    return Json(new { success = false, message = "خطا در ذخیره داده‌ها: " + ex.Message });
                }
            }
        }
        [HttpPost]
        public async Task<IActionResult> SaveForm3([FromBody] SaveForm3 dto)
        {
            var username = User.Identity.Name;
            var userid = await _context.Users.Where(p => p.UserName == username).Select(j => j.Id).SingleOrDefaultAsync();
            if (ModelState.IsValid)
            {
                var model = await _context.GoodsEntries.FirstOrDefaultAsync(g => g.GoodsEntryID == dto.GoodsEntryID);
                if (model == null) return Json(new { success = false, message = "رکورد یافت نشد." });

                model.TozihatKeyfiat = dto.TozihatKeyfiat;
                model.State = "BarrasiSanaye5";


                var workflowInstance = await _context.WorkflowInstances.FirstOrDefaultAsync(g => g.WorkFlowID == dto.GoodsEntryID && g.Section == "ControlKeyfiat2" && g.ProcessName == "GoodsEntryWorkflow");
                workflowInstance.IsCompleted = true;

                var workflowInstance1 = new WorkflowInstance
                {
                    WorkFlowID = model.GoodsEntryID,
                    ProcessName = "GoodsEntryWorkflow",
                    Section = model.State,
                    AssignedUserID = GetNextAssignedUser(model.State),
                    IsEditor = false,
                    AssignedDate = DateTime.Now,
                    IsCompleted = false
                };
                _context.WorkflowInstances.Add(workflowInstance1);

                NotifyUser(userid, GetNextAssignedUser(model.State), dto.Guid, "بررسی واحد صنایع");
                NotifyObservers(model.State, userid, dto.Guid);


                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "داده‌ها با موفقیت ذخیره شدند.", id = model.GoodsEntryID });
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
                var model = await _context.GoodsEntries.FirstOrDefaultAsync(g => g.GoodsEntryID == dto.GoodsEntryID);
                if (model == null) return Json(new { success = false, message = "رکورد یافت نشد." });

                model.State = "End9";

                // مدیریت جریان کاری
                var currentWorkflow = await _context.WorkflowInstances
                    .FirstOrDefaultAsync(w => w.WorkFlowID == dto.GoodsEntryID &&
                                              w.Section == "TazminKeyfiat3" &&
                                              w.ProcessName == "GoodsEntryWorkflow");

                if (currentWorkflow == null)
                {
                    return Json(new { success = false, message = "جریان کاری فعلی یافت نشد." });
                }

                currentWorkflow.IsCompleted = true;

                var workflowInstance1 = new WorkflowInstance
                {
                    WorkFlowID = model.GoodsEntryID,
                    ProcessName = "GoodsEntryWorkflow",
                    Section = model.State,
                    AssignedUserID = userid,
                    IsEditor = false,
                    AssignedDate = DateTime.Now,
                    IsCompleted = true
                };
                _context.WorkflowInstances.Add(workflowInstance1);

                //NotifyUser(userid, GetNextAssignedUser(model.State), dto.Guid, "پایان");
                //NotifyObservers(model.State, userid, dto.Guid);

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "داده‌ها با موفقیت ذخیره شدند.", id = model.GoodsEntryID });
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
                var model = await _context.GoodsEntries.FirstOrDefaultAsync(g => g.GoodsEntryID == dto.GoodsEntryID);
                if (model == null) return Json(new { success = false, message = "رکورد یافت نشد." });

                model.State = "End9";
                model.isTahvil = dto.isTahvil;
                model.TozihatOther = dto.TozihatOther;
                // مدیریت جریان کاری
                var currentWorkflow = await _context.WorkflowInstances
                    .FirstOrDefaultAsync(w => w.WorkFlowID == dto.GoodsEntryID &&
                                              w.Section == "Other4" &&
                                              w.ProcessName == "GoodsEntryWorkflow");

                if (currentWorkflow == null)
                {
                    return Json(new { success = false, message = "جریان کاری فعلی یافت نشد." });
                }

                currentWorkflow.IsCompleted = true;

                var workflowInstance1 = new WorkflowInstance
                {
                    WorkFlowID = model.GoodsEntryID,
                    ProcessName = "GoodsEntryWorkflow",
                    Section = model.State,
                    AssignedUserID = userid,
                    IsEditor = false,
                    AssignedDate = DateTime.Now,
                    IsCompleted = true
                };
                _context.WorkflowInstances.Add(workflowInstance1);

                //NotifyUser(userid, GetNextAssignedUser(model.State), dto.Guid, "پایان");
                //NotifyObservers(model.State, userid, dto.Guid);

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "داده‌ها با موفقیت ذخیره شدند.", id = model.GoodsEntryID });
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
                var model = await _context.GoodsEntries.FirstOrDefaultAsync(g => g.GoodsEntryID == dto.GoodsEntryID);
                if (model == null) return Json(new { success = false, message = "رکورد یافت نشد." });

                model.TozihatSanaye = dto.TozihatSanaye;
                model.State = "BaarasiTolid6";


                var workflowInstance = await _context.WorkflowInstances.FirstOrDefaultAsync(g => g.WorkFlowID == dto.GoodsEntryID && g.Section == "BarrasiSanaye5" && g.ProcessName == "GoodsEntryWorkflow");
                workflowInstance.IsCompleted = true;

                var workflowInstance1 = new WorkflowInstance
                {
                    WorkFlowID = model.GoodsEntryID,
                    ProcessName = "GoodsEntryWorkflow",
                    Section = model.State,
                    AssignedUserID = GetNextAssignedUser(model.State),
                    IsEditor = false,
                    AssignedDate = DateTime.Now,
                    IsCompleted = false
                };
                _context.WorkflowInstances.Add(workflowInstance1);

                NotifyUser(userid, GetNextAssignedUser(model.State), dto.Guid, "بررسی واحد تولید");
                NotifyObservers(model.State, userid, dto.Guid);


                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "داده‌ها با موفقیت ذخیره شدند.", id = model.GoodsEntryID });
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
                var model = await _context.GoodsEntries.FirstOrDefaultAsync(g => g.GoodsEntryID == dto.GoodsEntryID);
                if (model == null) return Json(new { success = false, message = "رکورد یافت نشد." });

                model.isTayedTolid = dto.isTayedTolid;
                model.TozihatTolid = dto.TozihatTolid;
                model.State = "BaarasiKeyfiat7";


                var workflowInstance = await _context.WorkflowInstances.FirstOrDefaultAsync(g => g.WorkFlowID == dto.GoodsEntryID && g.Section == "BaarasiTolid6" && g.ProcessName == "GoodsEntryWorkflow");
                workflowInstance.IsCompleted = true;

                var workflowInstance1 = new WorkflowInstance
                {
                    WorkFlowID = model.GoodsEntryID,
                    ProcessName = "GoodsEntryWorkflow",
                    Section = model.State,
                    AssignedUserID = GetNextAssignedUser(model.State),
                    IsEditor = false,
                    AssignedDate = DateTime.Now,
                    IsCompleted = false
                };
                _context.WorkflowInstances.Add(workflowInstance1);

                NotifyUser(userid, GetNextAssignedUser(model.State), dto.Guid, "بررسی واحد کنترل کیفیت");
                NotifyObservers(model.State, userid, dto.Guid);


                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "داده‌ها با موفقیت ذخیره شدند.", id = model.GoodsEntryID });
            }
            return Json(new { success = false, message = "خطا در ذخیره داده‌ها." });
        }

        //[HttpPost]
        //public async Task<IActionResult> SaveForm8([FromBody] SaveForm8 dto)
        //{
        //    var username = User.Identity.Name;
        //    var userid = await _context.Users.Where(p => p.UserName == username).Select(j => j.Id).SingleOrDefaultAsync();
        //    if (ModelState.IsValid)
        //    {
        //        var model = await _context.GoodsEntries.FirstOrDefaultAsync(g => g.GoodsEntryID == dto.GoodsEntryID);
        //        if (model == null)
        //            return Json(new { success = false, message = "رکورد یافت نشد." });

        //        // به‌روزرسانی داده‌های فرم
        //        model.State = "Anbar8"; 

        //        // به‌روزرسانی وضعیت Workflow قبلی
        //        var workflowInstance = await _context.WorkflowInstances
        //            .FirstOrDefaultAsync(g => g.WorkFlowID == dto.GoodsEntryID && g.Section == "BaarasiKeyfiat7" && g.ProcessName == "GoodsEntryWorkflow");
        //        if (workflowInstance != null)
        //        {
        //            workflowInstance.IsCompleted = true;
        //        }

        //        // ایجاد Workflow جدید
        //        var workflowInstance1 = new WorkflowInstance
        //        {
        //            WorkFlowID = model.GoodsEntryID,
        //            ProcessName = "GoodsEntryWorkflow",
        //            Section = model.State,
        //            AssignedUserID = GetNextAssignedUser(model.State),
        //            IsEditor = false,
        //            AssignedDate = DateTime.Now,
        //            IsCompleted = false
        //        };
        //        _context.WorkflowInstances.Add(workflowInstance1);

        //        // نوتیفیکیشن‌ها
        //        NotifyUser(userid, GetNextAssignedUser(model.State), dto.Guid, "جهت اطلاع انبار");
        //        NotifyObservers(model.State, userid, dto.Guid);

        //        // به‌روزرسانی آیتم‌های جدول
        //        if (dto.Items != null && dto.Items.Any())
        //        {
        //            foreach (var item in dto.Items)
        //            {
        //                var dbItem = await _context.GoodsEntryItems
        //                    .FirstOrDefaultAsync(x => x.GoodsEntryItemID == item.GoodsEntryItemID);
        //                if (dbItem != null)
        //                {
        //                    dbItem.Parameter = item.Parameter;
        //                    dbItem.Azmoon = item.Azmoon;
        //                    dbItem.TedadNemoneh = item.TedadNemoneh;
        //                    dbItem.Natigeh = item.Natigeh;
        //                    dbItem.Tozihat = item.Tozihat;
        //                    dbItem.iSENG = item.iSENG;

        //                }
        //            }
        //        }

        //        await _context.SaveChangesAsync();
        //        return Json(new { success = true, message = "داده‌ها با موفقیت ذخیره شدند.", id = model.GoodsEntryID });
        //    }
        //    return Json(new { success = false, message = "خطا در ذخیره داده‌ها." });
        //}
        [HttpPost]
        public async Task<IActionResult> SaveForm8([FromBody] SaveForm8 dto)
        {
            var username = User.Identity.Name;
            var userid = await _context.Users.Where(p => p.UserName == username).Select(j => j.Id).FirstOrDefaultAsync();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var model = await _context.GoodsEntries.FirstOrDefaultAsync(g => g.GoodsEntryID == dto.GoodsEntryID);
                if (model == null) return Json(new { success = false, message = "رکورد یافت نشد." });

                var needsEngReview = dto.Items?.Any(i => i.Natigeh == "مردود" && i.iSENG == true) == true;
                model.State = "Anbar8";
                // به‌روزرسانی آیتم‌ها
                if (dto.Items != null && dto.Items.Any())
                {
                    foreach (var item in dto.Items)
                    {
                        var dbItem = await _context.GoodsEntryItems
                            .FirstOrDefaultAsync(x => x.GoodsEntryItemID == item.GoodsEntryItemID);
                        if (dbItem != null)
                        {
                            dbItem.Parameter = item.Parameter;
                            dbItem.Azmoon = item.Azmoon;
                            dbItem.TedadNemoneh = item.TedadNemoneh;
                            dbItem.Natigeh = item.Natigeh;
                            dbItem.Tozihat = item.Tozihat;
                            dbItem.iSENG = item.iSENG;

                        }
                    }
                }

                // بستن مرحله QC
                var qcWorkflow = await _context.WorkflowInstances

                    .FirstOrDefaultAsync(w => w.WorkFlowID == dto.GoodsEntryID && w.Section == "BaarasiKeyfiat7");
                if (qcWorkflow != null) qcWorkflow.IsCompleted = true;

                var anbarInstance = new WorkflowInstance
                {
                    WorkFlowID = model.GoodsEntryID,
                    ProcessName = "GoodsEntryWorkflow",
                    Section = "Anbar8",
                    AssignedUserID = GetNextAssignedUser("Anbar8"),
                    IsEditor = false,
                    AssignedDate = DateTime.Now,
                    IsCompleted = false
                };
                _context.WorkflowInstances.Add(anbarInstance);
                NotifyUser(userid, anbarInstance.AssignedUserID, dto.Guid, "کالاها بررسی شدند - جهت اطلاع انبار");

                // اگر نیاز به بررسی فنی بود → شاخه موازی
                if (needsEngReview)
                {
                    var engInstance = new WorkflowInstance
                    {
                        WorkFlowID = model.GoodsEntryID,
                        ProcessName = "GoodsEntryWorkflow",
                        Section = "EngReview9",
                        AssignedUserID = GetNextAssignedUser("EngReview9"),
                        IsEditor = true,
                        AssignedDate = DateTime.Now,
                        IsCompleted = false,
                    };
                    _context.WorkflowInstances.Add(engInstance);
                    NotifyUser(userid, engInstance.AssignedUserID, dto.Guid, "بررسی فنی مهندسی - کالای مردود در QC");
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "خطا در SaveForm8");
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> SaveFormEng([FromBody] SaveFormEng dto)
        {
            var username = User.Identity.Name;
            var userid = await _context.Users
                .Where(u => u.UserName == username)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(username) || userid == default)
                return Json(new { success = false, message = "کاربر معتبر یافت نشد." });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // ۱. پیدا کردن رکورد اصلی ورود کالا
                var goodsEntry = await _context.GoodsEntries
                    .FirstOrDefaultAsync(g => g.GoodsEntryID == dto.GoodsEntryID);

                if (goodsEntry == null)
                    return Json(new { success = false, message = "رکورد ورود کالا یافت نشد." });

                // ۲. ذخیره نظر و نتیجه فنی مهندسی در جدول اصلی GoodsEntry
                goodsEntry.TozihatEng = dto.TozihatEng;           // توضیحات فنی
                goodsEntry.IsApprovedByEng = dto.IsApprovedByEng;      // true = تأیید، false = رد

                // ۳. بستن مرحله بررسی فنی مهندسی
                var engWorkflow = await _context.WorkflowInstances
                    .FirstOrDefaultAsync(w => w.WorkFlowID == dto.GoodsEntryID
                                           && w.Section == "EngReview9"
                                           && w.ProcessName == "GoodsEntryWorkflow");

                if (engWorkflow != null)
                {
                    engWorkflow.IsCompleted = true;
                }

                // ۴. اگر تأیید کرد → فقط اطلاع (یا هیچی)
                if (dto.IsApprovedByEng)
                {
                    // مثلاً فقط به انبار اطلاع بده که فنی تأیید کرد
                    var anbarUserId = GetNextAssignedUser("Anbar8");
                    if (!string.IsNullOrEmpty(anbarUserId))
                    {
                        NotifyUser(userid, anbarUserId, dto.Guid,
                            "فنی مهندسی تأیید کرد - کالای مردود با شرایط قابل استفاده است");
                    }
                }
                else
                {
                    // ۵. اگر رد کرد → شاخه موازی: تدارکات + تضمین کیفیت مجدد

                    // تدارکات
                    var tadarakatInstance = new WorkflowInstance
                    {
                        WorkFlowID = dto.GoodsEntryID,
                        ProcessName = "GoodsEntryWorkflow",
                        Section = "Tadarakat10",
                        AssignedUserID = GetNextAssignedUser("Tadarakat10"),
                        IsEditor = false,
                        AssignedDate = DateTime.Now,
                        IsCompleted = true,
                    };
                    _context.WorkflowInstances.Add(tadarakatInstance);
                    NotifyUser(userid, tadarakatInstance.AssignedUserID, dto.Guid,
                        "اقدام تدارکات: کالای مردود توسط فنی مهندسی - نیاز به مرجوع به تأمین‌کننده");

                    // تضمین کیفیت مجدد
                    var tazminInstance = new WorkflowInstance
                    {
                        WorkFlowID = dto.GoodsEntryID,
                        ProcessName = "GoodsEntryWorkflow",
                        Section = "TazminKeyfiat11",
                        AssignedUserID = GetNextAssignedUser("TazminKeyfiat11"),
                        IsEditor = false,
                        AssignedDate = DateTime.Now,
                        IsCompleted = true,
                    };
                    _context.WorkflowInstances.Add(tazminInstance);
                    NotifyUser(userid, tazminInstance.AssignedUserID, dto.Guid,
                        "بررسی مجدد کیفیت: کالای مردود توسط فنی مهندسی");
                    NotifyObservers("TazminKeyfiat11", userid, dto.Guid);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "نظر فنی مهندسی با موفقیت ثبت شد." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "خطا در ثبت نظر فنی مهندسی");
                return Json(new { success = false, message = "خطای سرور: " + ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> SaveForm9([FromBody] SaveForm9 dto)
        {
            var username = User.Identity.Name;
            var userid = await _context.Users.Where(p => p.UserName == username).Select(j => j.Id).SingleOrDefaultAsync();
            if (ModelState.IsValid)
            {
                var model = await _context.GoodsEntries.FirstOrDefaultAsync(g => g.GoodsEntryID == dto.GoodsEntryID);
                if (model == null) return Json(new { success = false, message = "رکورد یافت نشد." });

                model.State = "End9";


                var workflowInstance = await _context.WorkflowInstances.FirstOrDefaultAsync(g => g.WorkFlowID == dto.GoodsEntryID && g.Section == "Anbar8" && g.ProcessName == "GoodsEntryWorkflow");
                workflowInstance.IsCompleted = true;

                var StartUserID = _context.WorkflowInstances.Where(g => g.WorkFlowID == dto.GoodsEntryID && g.Section == "start0" && g.ProcessName == "GoodsEntryWorkflow").Select(p => p.AssignedUserID).FirstOrDefault();

                var workflowInstance1 = new WorkflowInstance
                {
                    WorkFlowID = model.GoodsEntryID,
                    ProcessName = "GoodsEntryWorkflow",
                    Section = model.State,
                    AssignedUserID = StartUserID,
                    IsEditor = false,
                    AssignedDate = DateTime.Now,
                    IsCompleted = false
                };
                _context.WorkflowInstances.Add(workflowInstance1);

                //NotifyUser(userid, GetNextAssignedUser(model.State), dto.Guid, "پایان");
                NotifyObservers(model.State, userid, dto.Guid);


                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "داده‌ها با موفقیت ذخیره شدند.", id = model.GoodsEntryID });
            }
            return Json(new { success = false, message = "خطا در ذخیره داده‌ها." });
        }
        private string GetNextAssignedUser(string state)
        {
            var nextAccess = _context.WorkflowAccesses
                .Where(a => a.ProcessName == "GoodsEntryWorkflow" && a.Section == state && a.Role == "Editor")
                .Select(a => a.UserID)
                .FirstOrDefault();

            if (nextAccess == null)
            {
                _logger.LogWarning($"No user found for section {state} with role Editor.");
                return null; // Or a default user ID
            }

            return nextAccess;
        }
        private string GetNextAssignedUser2(string state,string userid)
        {
            var nextAccess = _context.WorkflowAccesses
                .Where(a => a.ProcessName == "GoodsEntryWorkflow" && a.Section == state && a.Role == "Editor")
                .Select(a => a.UserID)
                .FirstOrDefault();

            if (nextAccess == null)
            {
                return userid; // Or a default user ID
            }

            return nextAccess;
        }
        private bool GetNextAssignedUserEditor(string state)
        {
            var nextAccess = _context.WorkflowAccesses
                .Where(a => a.ProcessName == "GoodsEntryWorkflow" && a.Section == state && a.Role == "Editor")
                .Select(a => a.UserID)
                .FirstOrDefault();

            return nextAccess == null; // اگر null بود true، اگر مقدار داشت false
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

        private void NotifyUser(string senderId, string receiverId, string guid, string title, string type = "ورود کالا")
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
            var goodsentry = _context.GoodsEntries.Find(id);
            var guid = goodsentry.Guid;
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

        [HttpGet]
        public async Task<IActionResult> GetItems(int goodsEntryId, string guid)
        {
            var items = await _context.GoodsEntryItems
                .Where(g => g.GoodsEntryID == goodsEntryId && g.Guid == guid)
                .Select(g => new
                {
                    g.NameKala,
                    g.NameTaminkonandeh,
                    g.ShonarehResid,
                    g.Tedad,
                    g.Vahed
                })
                .ToListAsync();

            return Json(items);
        }
        [HttpGet]
        public async Task<IActionResult> GetItems2(int goodsEntryId, string guid)
        {
            var items = await _context.GoodsEntryItems
                .Where(x => x.GoodsEntryID == goodsEntryId && x.Guid == guid)
                .ToListAsync();
            return Json(items);
        }
        [HttpGet]
        public IActionResult GetGoodsItems(int goodsEntryId, string guid)
        {
            var items = _context.GoodsEntryItemGuards
                .Where(g => g.GoodsEntryID == goodsEntryId && g.Guid == guid)
                .Select(g => new
                {
                    g.SharhKala,
                    g.Tedad,
                    g.Vahed,
                    g.Description
                })
                .ToList();

            return Json(items);
        }
        // GoodsEntryController.cs
        [HttpGet]
        public IActionResult GetAcceptedItems(int goodsEntryId)
        {
            // گرفتن آیتم‌ها از دیتابیس
            var items = _context.GoodsEntryItems
                .Where(x => x.GoodsEntryID == goodsEntryId && x.Natigeh == "قبول")
                .Select(x => new
                {
                    x.GoodsEntryItemID,
                    x.NameKala,
                    x.NameTaminkonandeh,
                    x.ShonarehResid,
                    x.Tedad,
                    x.Vahed,
                    x.Natigeh,
                    x.Tozihat,
                    x.Parameter,
                    x.TedadNemoneh,
                    x.Azmoon
                }).ToList();

            return Json(items);
        }
        [HttpGet]
        public IActionResult GetRejectedItemsForEng(int goodsEntryId)
        {
            var items = _context.GoodsEntryItems
                .Where(x => x.GoodsEntryID == goodsEntryId && x.Natigeh == "مردود" && x.iSENG == true)
                .Select(x => new
                {
                    x.GoodsEntryItemID,
                    x.NameKala,
                    x.NameTaminkonandeh,
                    x.ShonarehResid,
                    x.Tedad,
                    x.Vahed,
                    x.Natigeh,
                    x.Tozihat,
                    x.Parameter,
                    x.TedadNemoneh,
                    x.Azmoon
                })
                .ToList();

            return Json(items);
        }
    }
}