using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ERP.Data;
using ERP.Models;
using Microsoft.AspNetCore.Authorization;
using Stimulsoft.Blockly.Model;

using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using ERP.ViewModels.WorkReport;

using Microsoft.AspNetCore.Identity;

namespace ERP.Controllers
{
    [Authorize]
    public class WorkReportsController : Controller
    {
        private readonly ERPContext _context;
        public WorkReportsController(ERPContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            return View();
        }
        public async Task<DataSourceResult> Data_WorkReport([DataSourceRequest] DataSourceRequest request)
        {
            var username = User.Identity.Name;
            var userid = _context.Users.Where(p => p.UserName == username).Select(p => p.Id).Single();
            if (User.IsInRole("SuperAdmin") || User.IsInRole("Management") || User.IsInRole("ModirEdari"))
            {
                var model = _context.WorkReports
               .Select(u => new IndexWorkReportVM()
               {
                   WorkReportID = u.WorkReportID,
                   Name = _context.Users.Where(p => p.Id == u.UserID).Select(p => p.FirstName).SingleOrDefault() + " " + _context.Users.Where(p => p.Id == u.UserID).Select(p => p.LastName).SingleOrDefault(),
                   CreateON = u.CreateON.ToPersianDateTime().ToString("yyyy/MM/dd"),
                   DateWorkReport = u.DateWorkReport,
                   Description = u.Description,
               }).ToList().OrderByDescending(p => p.WorkReportID);
                return await model.ToDataSourceResultAsync(request);

            }
            else if (User.IsInRole("SalesMgr"))
            {
                //var roleidsales = _context.Roles.Where(p => p.Name == "Sales").Select(c => c.Id).SingleOrDefault();
                var roleidsalesmeter = _context.Roles.Where(p => p.Name == "SalesMeter").Select(c => c.Id).SingleOrDefault();
                var roleidaftersales = _context.Roles.Where(p => p.Name == "AfterSales").Select(c => c.Id).SingleOrDefault();

                //var usersinrolesales = _context.UserRoles.Where(p => p.RoleId == roleidsales).Select(p => p.UserId).ToList();
                var usersinrolesalesmeter = _context.UserRoles.Where(p => p.RoleId == roleidsalesmeter).Select(p => p.UserId).ToList();
                var usersinroleaftersales = _context.UserRoles.Where(p => p.RoleId == roleidaftersales).Select(p => p.UserId).ToList();

                var model = _context.WorkReports
                .Where(u => usersinroleaftersales.Contains(u.UserID) || usersinrolesalesmeter.Contains(u.UserID))
               .Select(u => new IndexWorkReportVM()
               {
                   WorkReportID = u.WorkReportID,
                   Name = _context.Users.Where(p => p.Id == u.UserID).Select(p => p.FirstName).SingleOrDefault() + " " + _context.Users.Where(p => p.Id == u.UserID).Select(p => p.LastName).SingleOrDefault(),
                   CreateON = u.CreateON.ToPersianDateTime().ToString("yyyy/MM/dd"),
                   DateWorkReport = u.DateWorkReport,
                   Description = u.Description,
               }).ToList().OrderByDescending(p => p.WorkReportID);
                return await model.ToDataSourceResultAsync(request);

            }
            else
            {
                var model = _context.WorkReports.Where(p => p.UserID == userid)
            .Select(u => new IndexWorkReportVM()
            {
                WorkReportID = u.WorkReportID,
                Name = _context.Users.Where(p => p.Id == u.UserID).Select(p => p.FirstName).SingleOrDefault() + " " + _context.Users.Where(p => p.Id == u.UserID).Select(p => p.LastName).SingleOrDefault(),
                CreateON = u.CreateON.ToPersianDateTime().ToString("yyyy/MM/dd"),
                DateWorkReport = u.DateWorkReport,
                Description = u.Description,
            }).ToList().OrderByDescending(p => p.WorkReportID);
                return await model.ToDataSourceResultAsync(request);
            }
            return (null);
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workReport = await _context.WorkReports.FirstOrDefaultAsync(m => m.WorkReportID == id);
            if (workReport == null)
            {
                return NotFound();
            }
            ViewBag.files = _context.FileDBs.Where(p => p.Guid == workReport.GuidFile).ToList();
            return View(workReport);
        }
        [HttpGet]
        public IActionResult Create()
        {
            Guid guid = Guid.NewGuid();
            ViewBag.guid = guid;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(WorkReport workReport, string username, string guid)
        {
            var users = _context.Users.OrderBy(x => x.UserName).ToList();
            var userid = users.Where(p => p.UserName == username).Select(p => p.Id).Single();
            workReport.GuidFile = guid;
            workReport.CreateON = DateTime.Now;

            if (ModelState.IsValid)
            {
                // به‌روزرسانی فایل‌های موجود با IsTemp = true به IsTemp = false
                var filelist = _context.FileDBs.Where(x => x.Guid == guid && x.IsTemp == true).ToList();
                foreach (var file in filelist)
                {
                    file.IsTemp = false; // تغییر وضعیت به false
                    _context.FileDBs.Update(file); // به‌روزرسانی رکورد
                }

                workReport.UserID = userid;
                _context.Add(workReport);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(workReport);
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var workReport = await _context.WorkReports.FindAsync(id);
            if (workReport == null)
            {
                return NotFound();
            }
            return View(workReport);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id, WorkReport workReport, string guid)
        {
            if (id != workReport.WorkReportID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // به‌روزرسانی فایل‌های موجود با IsTemp = true به IsTemp = false
                var filelist = _context.FileDBs.Where(c => c.IsTemp == true && c.Guid == guid).ToList();
                foreach (var file in filelist)
                {
                    file.IsTemp = false; // تغییر وضعیت به false
                    _context.FileDBs.Update(file); // به‌روزرسانی رکورد
                }

                _context.Update(workReport);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(workReport);
        }
        [HttpPost] 
        public async Task<JsonResult> Delete(int id)
        {
            try
            {
                var workReport = await _context.WorkReports.FindAsync(id); // async برای Find
                if (workReport == null)
                {
                    return Json(new { status = "NotFound", message = "رکورد یافت نشد" });
                }

                var guid = workReport.GuidFile;
                var filelist = await _context.FileDBs.Where(x => x.Guid == guid).ToListAsync(); // async برای ToList

                _context.WorkReports.Remove(workReport);

                foreach (var item in filelist) 
                {
                    _context.FileDBs.Remove(item);
                }
                await _context.SaveChangesAsync();

                return Json(new { status = "OK", message = "حذف با موفقیت انجام شد" });
            }
            catch (Exception ex)
            {
                return Json(new { status = "Error", message = ex.Message }); // به JS برگردون
            }
        }
        public JsonResult DownloadFile(int id)
        {
            var workReport = _context.WorkReports.Find(id);
            var guid = workReport.GuidFile;
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
        private bool WorkReportExists(int id)
        {
            return _context.WorkReports.Any(e => e.WorkReportID == id);
        }
    }
}
