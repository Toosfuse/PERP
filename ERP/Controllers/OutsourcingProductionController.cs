using ERP.Data;
using ERP.Models;
using ERP.ViewModels.OutsourcingProduction;
using ERP.ViewModels.ViewProcess;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ERP.Controllers
{
    [Authorize]
    public class OutsourcingProductionController : Controller
    {
        private readonly ERPContext _context;
        private readonly ILogger<OutsourcingProductionController> _logger;

        public OutsourcingProductionController(ERPContext context, ILogger<OutsourcingProductionController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var username = User.Identity.Name;
            var userid = await _context.Users.Where(p => p.UserName == username).Select(j => j.Id).FirstOrDefaultAsync();

            bool isSuperAdminOrManagement = User.IsInRole("SuperAdmin") || User.IsInRole("Management");
            IQueryable<int> accessibleIds;

            var workflowSteps = await _context.WorkflowSteps
                .Where(ws => ws.ProcessName == "OutsourcingProductionWorkflow")
                .OrderBy(ws => ws.OrderIndex)
                .ToListAsync();

            if (isSuperAdminOrManagement)
            {
                accessibleIds = _context.OutsourcingProductions.Select(g => g.OutsourcingProductionID).Distinct();
            }
            else
            {
                var observerStages = await _context.WorkflowAccesses
                    .Where(a => a.UserID == userid && a.ProcessName == "OutsourcingProductionWorkflow" && a.Role == "Observer")
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

                accessibleIds = _context.OutsourcingProductions
                    .Where(g => accessibleStages.Contains(g.State) ||
                                _context.WorkflowInstances.Any(wi => wi.WorkFlowID == g.OutsourcingProductionID && wi.ProcessName == "OutsourcingProductionWorkflow" && wi.AssignedUserID == userid))
                    .Select(g => g.OutsourcingProductionID)
                    .Distinct();
            }

            ViewBag.all = await _context.OutsourcingProductions
                .Where(g => accessibleIds.Contains(g.OutsourcingProductionID))
                .CountAsync();

            ViewBag.s1 = await _context.OutsourcingProductions
                .Where(g => g.State == "TayidMasool1" && accessibleIds.Contains(g.OutsourcingProductionID))
                .CountAsync();

            return View();
        }

        // نمونه Data action مشابه GoodsConsigned (تطبیق کامل از کنترلر مرجع پیشنهاد می‌شود)
        public async Task<DataSourceResult> Data_Outsourcing([DataSourceRequest] DataSourceRequest request, string SelectTab)
        {
            var username = User.Identity.Name;
            var userid = await _context.Users.Where(p => p.UserName == username).Select(j => j.Id).FirstOrDefaultAsync();
            bool isSuperAdminOrManagement = User.IsInRole("SuperAdmin") || User.IsInRole("Management");

            var workflowSteps = await _context.WorkflowSteps
                .Where(ws => ws.ProcessName == "OutsourcingProductionWorkflow")
                .OrderBy(ws => ws.OrderIndex)
                .ToListAsync();

            IQueryable<IndexOutsourcingProductionVM> model1;
            if (isSuperAdminOrManagement)
            {
                if (SelectTab == "همه لیست")
                {
                    model1 = _context.OutsourcingProductions
                        .Select(u => new IndexOutsourcingProductionVM
                        {
                            OutsourcingProductionID = u.OutsourcingProductionID,
                            State = _context.WorkflowSteps
                                .Where(p => p.SectionCode == u.State && p.ProcessName == "OutsourcingProductionWorkflow")
                                .Select(p => p.SectionName)
                                .FirstOrDefault() ?? u.State ?? string.Empty,
                            S_persiandate = u.S_persiandate ?? string.Empty,
                            Guid = u.Guid,
                            SourceTable = 0
                        }).OrderByDescending(u => u.OutsourcingProductionID);
                }
                else
                {
                    var selectedState = SelectTab;
                    model1 = _context.OutsourcingProductions
                        .Where(g => g.State == selectedState)
                        .Select(u => new IndexOutsourcingProductionVM
                        {
                            OutsourcingProductionID = u.OutsourcingProductionID,
                            State = _context.WorkflowSteps
                                .Where(p => p.SectionCode == u.State && p.ProcessName == "OutsourcingProductionWorkflow")
                                .Select(p => p.SectionName)
                                .FirstOrDefault() ?? u.State ?? string.Empty,
                            S_persiandate = u.S_persiandate ?? string.Empty,
                            Guid = u.Guid,
                            SourceTable = 1
                        }).OrderByDescending(u => u.OutsourcingProductionID);
                }
            }
            else
            {
                var observerStages = await _context.WorkflowAccesses
                    .Where(a => a.UserID == userid && a.ProcessName == "OutsourcingProductionWorkflow" && a.Role == "Observer")
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
                    model1 = _context.OutsourcingProductions
                        .Where(g => accessibleStages.Contains(g.State) ||
                                    _context.WorkflowInstances.Any(wi => wi.WorkFlowID == g.OutsourcingProductionID && wi.ProcessName == "OutsourcingProductionWorkflow" && wi.AssignedUserID == userid))
                        .Select(u => new IndexOutsourcingProductionVM
                        {
                            OutsourcingProductionID = u.OutsourcingProductionID,
                            State = _context.WorkflowSteps
                                .Where(p => p.SectionCode == u.State && p.ProcessName == "OutsourcingProductionWorkflow")
                                .Select(p => p.SectionName)
                                .FirstOrDefault() ?? u.State ?? string.Empty,
                            S_persiandate = u.S_persiandate ?? string.Empty,
                            Guid = u.Guid,
                            SourceTable = 0
                        }).OrderByDescending(u => u.OutsourcingProductionID);
                }
                else
                {
                    var selectedState = SelectTab;
                    model1 = _context.OutsourcingProductions
                        .Where(g => g.State == selectedState && (
                            accessibleStages.Contains(g.State) ||
                            _context.WorkflowInstances.Any(wi => wi.WorkFlowID == g.OutsourcingProductionID && wi.ProcessName == "OutsourcingProductionWorkflow" && wi.AssignedUserID == userid)))
                        .Select(u => new IndexOutsourcingProductionVM
                        {
                            OutsourcingProductionID = u.OutsourcingProductionID,
                            State = _context.WorkflowSteps
                                .Where(p => p.SectionCode == u.State && p.ProcessName == "OutsourcingProductionWorkflow")
                                .Select(p => p.SectionName)
                                .FirstOrDefault() ?? u.State ?? string.Empty,
                            S_persiandate = u.S_persiandate ?? string.Empty,
                            Guid = u.Guid,
                            SourceTable = 1
                        }).OrderByDescending(u => u.OutsourcingProductionID);
                }
            }
            return await model1.ToDataSourceResultAsync(request);
        }

        public async Task<IActionResult> OutsourcingProductionView(int? id)
        {
            OutsourcingProduction model;
            var username = User.Identity.Name;
            var userid = await _context.Users.Where(p => p.UserName == username).Select(j => j.Id).SingleOrDefaultAsync();

            var workflowSteps = await _context.WorkflowSteps
                .Where(ws => ws.ProcessName == "OutsourcingProductionWorkflow")
                .OrderBy(ws => ws.OrderIndex)
                .ToListAsync();

            if (id.HasValue && id > 0)
            {
                model = await _context.OutsourcingProductions.FirstOrDefaultAsync(g => g.OutsourcingProductionID == id);
                if (model == null) return NotFound();
                ViewBag.Guid = model.Guid;
            }
            else
            {
                model = new OutsourcingProduction
                {
                    OutsourcingProductionID = 0,
                    State = "start0",
                    S_persiandate = DateTime.Now.ToPersianDateTime().ToString("yyyy/MM/dd")
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

        // متدهای کمکی مثل NotifyUser, NotifyObservers, GetNextAssignedUser و SaveForm2..SaveForm9
        // را می‌توانید از GoodsConsignedController کپی کنید و ProcessName/DbSetها را به "OutsourcingProductionWorkflow"/OutsourcingProductions تطبیق دهید.
    }
}