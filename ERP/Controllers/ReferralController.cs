using ERP.Data;
using ERP.Models;
using ERP.ViewModels.Referral;
using ERP.ViewModels.WorkReport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ERP.Controllers
{
    public class ReferralController : Controller
    {

        private readonly ERPContext _context;

        public ReferralController(ERPContext context)
        {
            _context = context;
        }
        [HttpPost]
        public async Task<JsonResult> ListReferral(string guid)
        {
            try
            {
                if (string.IsNullOrEmpty(guid))
                {
                    return Json(new
                    { data = new List<ReferralGroupVM>(), status = "error", message = "شناسه معتبر نیست" });
                }

                var model = await (from r in _context.Referrals
                                   join sender in _context.Users on r.SenderID equals sender.Id
                                   join receiver in _context.Users on r.ReceiverID equals receiver.Id
                                   where r.Guid == guid
                                   group new { r, sender, receiver } by new { r.GuidGroup }
                    into g
                                   orderby g.Min(x => x.r.CreateON) descending
                                   select new ReferralGroupVM
                                   {
                                       GuidGroup = g.Key.GuidGroup,
                                       vahid = g.Select(e => new ReferralVM
                                       {
                                           ReferralID = e.r.ReferralID,
                                           FullName = e.sender.FirstName + " " + e.sender.LastName + " " + e.sender.Post,
                                           SenderID = e.r.SenderID,
                                           CreateON = e.r.CreateON.ToPersianDigitalDateTimeString(),
                                           Description = string.IsNullOrWhiteSpace(e.r.Description) ? "" : e.r.Description,
                                           FirstView = e.r.FirstView,
                                           Guid = guid,
                                           Image = e.sender.Image,
                                           ReceiverFullName2 = e.receiver.FirstName + " " + e.receiver.LastName,
                                           ReceiverPost = e.receiver.Post,
                                           ReceiverImage = e.receiver.Image,
                                           ReceiverID = e.r.ReceiverID
                                       }).ToList()
                                   }).ToListAsync();

                Console.WriteLine($"Records found for Guid {guid}: {model.Count}");
                return Json(new { data = model, status = "ok" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ListReferral: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return Json(new { data = new List<ReferralGroupVM>(), status = "error", message = ex.Message });
            }
        }
        [Authorize]
        public JsonResult InsertReferral(string Blocks, Guid Guid, string username, string Type, string Title)
        {
            try
            {
                // Parse Blocks به لیست مدل‌ها
                var blocks = JsonConvert.DeserializeObject<List<ReferralBlockModel>>(Blocks);

                if (blocks == null || !blocks.Any())
                {
                    return Json(new { status = "Error", message = "بلوک‌های ارجاع خالی است." });
                }

                var userid = _context.Users.Where(p => p.UserName == username).Select(p => p.Id).SingleOrDefault();
                if (userid == null)
                {
                    return Json(new { status = "Error", message = "کاربر یافت نشد." });
                }

                foreach (var block in blocks)
                {
                    if (block.ReceiverIDs == null || !block.ReceiverIDs.Any())  // فقط ReceiverIDs رو چک کن (Description اختیاری)
                    {
                        continue;  // بلوک بدون گیرنده رو رد کن
                    }
                    var description = string.IsNullOrEmpty(block.Description) ? "" : block.Description;

                    Guid groupGuid = Guid.NewGuid();  // برای هر بلوک یک گروه جدید

                    foreach (var receiverId in block.ReceiverIDs)
                    {
                        var Model = new ViewProcess
                        {
                            SenderID = userid,
                            ReceiverID = receiverId,
                            SendDateTime = DateTime.Now,
                            Guid = Guid.ToString(),  // Guid اصلی از پارامتر
                            IsView = false,
                            Type = Type,
                            Title = Title
                        };

                        var Model2 = new Referral()
                        {
                            SenderID = userid,
                            ReceiverID = receiverId,
                            CreateON = DateTime.Now,
                            Description = description,  // حالا حتی اگر خالی بود، پیش‌فرض داره
                            FirstView = null,
                            Guid = Guid.ToString(),  // Guid اصلی
                            GuidGroup = groupGuid.ToString(),
                        };

                        _context.ViewProcesss.Add(Model);
                        _context.Referrals.Add(Model2);
                    }
                }

                _context.SaveChanges();

                return Json(new { status = "OK" }, new Newtonsoft.Json.JsonSerializerSettings());
            }
            catch (Exception ex)
            {
                return Json(new { status = "Error", message = ex.Message });
            }
        }
        [Authorize]
        public JsonResult ListFile(string Guid)
        {
            var filelist = _context.FileDBs.Where(x => x.Guid == Guid && x.IsTemp == false).ToList();
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
            return Json(new { status = "NOT OK" });
        }
        [Authorize]
        public JsonResult listFileNotif(string Guid)
        {
            var filelist = _context.FileDBs.Where(x => x.Guid == Guid && x.IsTemp == false).ToList();
            if (filelist != null)
            {
                var model = filelist.Count();
                return Json(new { data = model, status = "OK" }, new Newtonsoft.Json.JsonSerializerSettings());
            }
            return Json(new { status = "NOT OK" });
        }
        [Authorize]
        public JsonResult UploadFile(string Guid)
        {
            var filelist = _context.FileDBs.Where(x => x.Guid == Guid).ToList();
            if (ModelState.IsValid)
            {
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
                return Json(new { status = "OK" }, new Newtonsoft.Json.JsonSerializerSettings());
            }
            return Json(new { status = "NOT OK" });
        }
        [Authorize]
        public JsonResult GetUsers()
        {
            var users = _context.Users.Where(p => p.InTFC == true).Select(u => new Users()
            {
                Id = u.Id,
                LastName = u.FirstName + " " + u.LastName,
                Image = u.Image,
                Post = u.Post,
            }).ToList();
            return Json(users);

        }
    }
}
public class ReferralBlockModel
{
    public List<string> ReceiverIDs { get; set; }  // یا List<int> اگر Id عددی باشه
    public string Description { get; set; }
}