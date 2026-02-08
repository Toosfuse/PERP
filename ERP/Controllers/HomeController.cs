using ERP.Data;
using ERP.Models;

using ERP.ViewModels.Referral;

using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Stimulsoft.Blockly.Model;
using System.Globalization;
using System.Security.Claims;


namespace ERP.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ERPContext _context;
        private readonly UserManager<Users> _userManager;
        public HomeController(ILogger<HomeController> logger, ERPContext context, UserManager<Users> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }



        public IActionResult Error()
        {
            return View();
        }

        public JsonResult ListViewProcessNotif(string id)
        {
            var ListViewProcesslist =
                _context.ViewProcesss.Where(p => p.ReceiverID == id && p.IsView == false).ToList();
            var model = ListViewProcesslist.Select(u => new ViewProcess()
            {
                ViewProcessID = u.ViewProcessID,
                SenderID = (_context.Users.Where(p => p.Id == u.SenderID).Select(p => p.FirstName).FirstOrDefault() ?? "") + " " +
                           (_context.Users.Where(p => p.Id == u.SenderID).Select(p => p.LastName).FirstOrDefault() ?? ""),
                ReceiverID = u.SendDateTime.ToPersianDateString().ToString(),
                Type = u.Type,
                Title = u.Title,
            }).ToList().OrderByDescending(p => p.SendDateTime);
            return Json(new { data = model, status = "ok" }, new Newtonsoft.Json.JsonSerializerSettings());
        }

        public async Task<IActionResult> Cartable(string type = null, int unread = 0)
        {
            ViewBag.SelectedType = type;
            ViewBag.IsUnreadOnly = unread == 1;
            return View();
        }

        public async Task<IActionResult> SendCartable()
        {
            return View();
        }

        public async Task<DataSourceResult> Data_Cartable([DataSourceRequest] DataSourceRequest request,
            string type = null, int unread = 0)
        {
            var username = User.Identity.Name;
            var userid = _context.Users.Where(p => p.UserName == username).Select(p => p.Id).Single();
            var query = _context.ViewProcesss.Where(p => p.ReceiverID == userid);

            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(p => p.Type == type);
            }

            if (unread == 1)
            {
                query = query.Where(p => !p.IsView);
            }

            var ListViewProcesslist = query.ToList();
            var model = ListViewProcesslist
                .Select(u => new ViewProcess
                {
                    ViewProcessID = u.ViewProcessID,
                    SenderID = _context.Users.Where(p => p.Id == u.SenderID).Select(p => p.FirstName).First() + " " +
                               _context.Users.Where(p => p.Id == u.SenderID).Select(p => p.LastName).First() + " " +
                               _context.Users.Where(p => p.Id == u.SenderID).Select(p => p.Post).First(),
                    ReceiverID = u.SendDateTime.ToPersianDateString(),
                    Type = u.Type,
                    Guid = _context.Users.Where(p => p.Id == u.SenderID).Select(p => p.Image).First(),
                    IsView = u.IsView,
                    SendDateTime = u.SendDateTime,
                    Title = u.Title
                })
                .OrderByDescending(p => p.SendDateTime)
                .ToList();

            return await model.ToDataSourceResultAsync(request);
        }

        public IActionResult GetCartableTypes()
        {
            var username = User.Identity.Name;
            var userid = _context.Users.Where(p => p.UserName == username).Select(p => p.Id).Single();
            var types = _context.ViewProcesss
                .Where(p => p.ReceiverID == userid)
                .Select(p => p.Type)
                .Distinct()
                .ToList();

            return Json(types);
        }

        public IActionResult GetUnreadCounts()
        {
            var username = User.Identity.Name;
            var userid = _context.Users.Where(p => p.UserName == username).Select(p => p.Id).Single();

            // تعداد کل unreadها
            var totalUnread = _context.ViewProcesss.Count(p => p.ReceiverID == userid && !p.IsView);

            // تعداد unread برای هر type
            var unreadByType = _context.ViewProcesss
                .Where(p => p.ReceiverID == userid && !p.IsView)
                .GroupBy(p => p.Type)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToList();

            return Json(new { totalUnread, unreadByType });
        }

        public async Task<DataSourceResult> Data_SendCartable([DataSourceRequest] DataSourceRequest request)
        {
            var username = User.Identity.Name;
            var userid = _context.Users.Where(p => p.UserName == username).Select(p => p.Id).Single();

            // query IQueryable نگه دار (بدون ToList() زودرس)
            var query = _context.ViewProcesss.Where(p => p.SenderID == userid);

            // projection مستقیم روی query (برای efficiency)
            var model = query.Select(u => new ViewProcess
            {
                ViewProcessID = u.ViewProcessID,
                // SenderID = نام فرستنده (خود کاربر – ثابت)
                SenderID = _context.Users.Where(p => p.Id == userid).Select(p => p.FirstName).First() + " " +
                               _context.Users.Where(p => p.Id == userid).Select(p => p.LastName).First() + " " +
                               _context.Users.Where(p => p.Id == userid).Select(p => p.Post).First(),
                // ReceiverID = تاریخ شمسی (extension method استاندارد)
                ReceiverID = u.SendDateTime.ToPersianDateString(),  // یا u.SendDateTime.ToString("yyyy/MM/dd") اگر extension نداری
                SendDateTime = u.SendDateTime,
                Type = u.Type,
                // Guid = Image گیرنده (برای consistency با Data_Cartable – Image فرستنده/گیرنده)
                Guid = _context.Users.Where(p => p.Id == u.ReceiverID).Select(p => p.Image).First(),
                Title = u.Title,
                IsView = u.IsView  // اضافه کن اگر لازم (برای unread)
            })
                .OrderByDescending(p => p.SendDateTime)  // OrderByDescending روی IQueryable
                .ToList();  // فقط در انتها ToList() اگر لازم، اما ToDataSourceResultAsync خودش handle می‌کنه

            // ToDataSourceResultAsync روی model (Kendo sorting/paging اعمال می‌شه)
            return await model.ToDataSourceResultAsync(request);
        }
        public async Task<IActionResult> VisitCartable(int ID)
        {
            var username = User.Identity.Name;
            var userid = _context.Users.Where(p => p.UserName == username).Select(p => p.Id).Single();
            var viewprocess = _context.ViewProcesss.Find(ID);
   
            if (viewprocess.Type == "گزارش ماموریت")
            {
                var missionreportid = _context.MissionReports.Where(p => p.Guid == viewprocess.Guid)
                    .Select(p => p.MissionReportID).First();
                viewprocess.IsView = true;
                viewprocess.ViewDateTime = DateTime.Now;
                _context.ViewProcesss.Update(viewprocess);
                await _context.SaveChangesAsync();
                var missionreport = _context.MissionReports.Find(missionreportid);
                var referalidlist = _context.Referrals
                    .Where(p => p.ReceiverID == userid && p.Guid == missionreport.Guid).Select(p => p.ReferralID)
                    .ToList();
                foreach (var item in referalidlist)
                {
                    var referal = _context.Referrals.Find(item);
                    if (referal.FirstView == null)
                    {
                        referal.FirstView = DateTime.Now;
                    }

                    _context.Referrals.Update(referal);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("MissionReportView", "MissionReport", new { id = missionreportid });
            }

            if (viewprocess.Type == "ورود مهمان")
            {
                var guestentryid = _context.GuestEntries.Where(p => p.Guid == viewprocess.Guid)
                    .Select(p => p.GuestEntryID).First();
                viewprocess.IsView = true;
                viewprocess.ViewDateTime = DateTime.Now;
                _context.ViewProcesss.Update(viewprocess);
                await _context.SaveChangesAsync();
                var guestentry = _context.GuestEntries.Find(guestentryid);
                var referalidlist = _context.Referrals.Where(p => p.ReceiverID == userid && p.Guid == guestentry.Guid)
                    .Select(p => p.ReferralID).ToList();
                foreach (var item in referalidlist)
                {
                    var referal = _context.Referrals.Find(item);
                    if (referal.FirstView == null)
                    {
                        referal.FirstView = DateTime.Now;
                    }

                    _context.Referrals.Update(referal);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("GuestEntryView", "GuestEntry", new { id = guestentryid });
            }

            if (viewprocess.Type == "خروج کالا")
            {
                var goodsdepartureid = _context.GoodsDepartures.Where(p => p.Guid == viewprocess.Guid)
                    .Select(p => p.GoodsDepartureID).First();
                viewprocess.IsView = true;
                viewprocess.ViewDateTime = DateTime.Now;
                _context.ViewProcesss.Update(viewprocess);
                await _context.SaveChangesAsync();
                var goodsdeparture = _context.GoodsDepartures.Find(goodsdepartureid);
                var referalidlist = _context.Referrals
                    .Where(p => p.ReceiverID == userid && p.Guid == goodsdeparture.Guid).Select(p => p.ReferralID)
                    .ToList();
                foreach (var item in referalidlist)
                {
                    var referal = _context.Referrals.Find(item);
                    if (referal.FirstView == null)
                    {
                        referal.FirstView = DateTime.Now;
                    }

                    _context.Referrals.Update(referal);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("GoodsDepartureView", "GoodsDeparture", new { id = goodsdepartureid });
            }

            if (viewprocess.Type == "ورود کالا")
            {
                var goodsentryid = _context.GoodsEntries.Where(p => p.Guid == viewprocess.Guid)
                    .Select(p => p.GoodsEntryID).First();
                viewprocess.IsView = true;
                viewprocess.ViewDateTime = DateTime.Now;
                _context.ViewProcesss.Update(viewprocess);
                await _context.SaveChangesAsync();
                var goodsdentry = _context.GoodsEntries.Find(goodsentryid);
                var referalidlist = _context.Referrals
                    .Where(p => p.ReceiverID == userid && p.Guid == goodsdentry.Guid).Select(p => p.ReferralID)
                    .ToList();
                foreach (var item in referalidlist)
                {
                    var referal = _context.Referrals.Find(item);
                    if (referal.FirstView == null)
                    {
                        referal.FirstView = DateTime.Now;
                    }

                    _context.Referrals.Update(referal);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("GoodsEntryView", "GoodsEntry", new { id = goodsentryid });
            }

            if (viewprocess.Type == "ارسال کالای امانی")
            {
                var goodsconsignedid = _context.GoodsConsigneds.Where(p => p.Guid == viewprocess.Guid)
                    .Select(p => p.GoodsConsignedID).First();
                viewprocess.IsView = true;
                viewprocess.ViewDateTime = DateTime.Now;
                _context.ViewProcesss.Update(viewprocess);
                await _context.SaveChangesAsync();
                var goodsconsigned = _context.GoodsConsigneds.Find(goodsconsignedid);
                var referalidlist = _context.Referrals
                    .Where(p => p.ReceiverID == userid && p.Guid == goodsconsigned.Guid).Select(p => p.ReferralID)
                    .ToList();
                foreach (var item in referalidlist)
                {
                    var referal = _context.Referrals.Find(item);
                    if (referal.FirstView == null)
                    {
                        referal.FirstView = DateTime.Now;
                    }

                    _context.Referrals.Update(referal);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("GoodsConsignedView", "GoodsConsigned", new { id = goodsconsignedid });
            }

            return View();
        }

        public async Task<IActionResult> VisitSendCartable(int ID)
        {
            var username = User.Identity.Name;
            var userid = _context.Users.Where(p => p.UserName == username).Select(p => p.Id).Single();
            var viewprocess = _context.ViewProcesss.Find(ID);
            

            if (viewprocess.Type == "گزارش ماموریت")
            {
                var missionreportid = _context.MissionReports.Where(p => p.Guid == viewprocess.Guid)
                    .Select(p => p.MissionReportID).First();
                return RedirectToAction("MissionReportView", "MissionReport", new { id = missionreportid });
            }

            if (viewprocess.Type == "ورود مهمان")
            {
                var guestentryid = _context.GuestEntries.Where(p => p.Guid == viewprocess.Guid)
                    .Select(p => p.GuestEntryID).First();
                return RedirectToAction("GuestEntryView", "GuestEntry", new { id = guestentryid });
            }

            if (viewprocess.Type == "خروج کالا")
            {
                var goodsdepartureid = _context.GoodsDepartures.Where(p => p.Guid == viewprocess.Guid)
                    .Select(p => p.GoodsDepartureID).First();
                return RedirectToAction("GoodsDepartureView", "GoodsDeparture", new { id = goodsdepartureid });
            }

            if (viewprocess.Type == "ورود کالا")
            {
                var goodsentryid = _context.GoodsEntries.Where(p => p.Guid == viewprocess.Guid)
                    .Select(p => p.GoodsEntryID).First();
                return RedirectToAction("GoodsDepartureView", "GoodsDeparture", new { id = goodsentryid });
            }

            if (viewprocess.Type == "ارسال کالای امانی")
            {
                var goodsconsignedid = _context.GoodsConsigneds.Where(p => p.Guid == viewprocess.Guid)
                    .Select(p => p.GoodsConsignedID).First();
                return RedirectToAction("GoodsConsignedView", "GoodsConsigned", new { id = goodsconsignedid });
            }

            return View();
        }
     







    }
}