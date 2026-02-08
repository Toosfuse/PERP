
using ERP.Data;
using ERP.Models;

using ERP.ViewModels.ManageUser;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;
using SixLabors.ImageSharp;

namespace ERP.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class ManageUserController : Controller
    {
        private readonly UserManager<Users> _userManager;
        private readonly RoleManager<Roles> _roleManager;
        private readonly ERPContext _context;

        public ManageUserController(UserManager<Users> userManager, RoleManager<Roles> roleManager, ERPContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }


        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        public async Task<DataSourceResult> Data_User([DataSourceRequest] DataSourceRequest request)
        {
            var users = _context.Users.ToList();
            var model = _context.Users
            .Select(u => new Users()
            {
                Id = u.Id,
                UserName = u.UserName,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Image = u.Image,
                Post = u.Post,
            }).ToList().Select(m =>
            {
                var roleid = _context.UserRoles.Where(p => p.UserId == m.Id).Select(c => c.RoleId).ToList();
                List<string> listrolename = new List<string>();
                foreach (var role in roleid)
                {
                    var x = _context.Roles.Where(p => p.Id == role).Select(c => c.Title).SingleOrDefault();
                    listrolename.Add(x);
                }
                var rolename = "";
                rolename = string.Join(",", listrolename);

                m.Email = rolename;
                return m;
            }).ToList();
            return await model.ToDataSourceResultAsync(request);
        }
        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new EditUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Post = user.Post,
                NationalCode = user.NationalCode,
                PhoneNumber = user.PhoneNumber,
                CustCodeAmel = user.CustCodeAmel,
                Address = user.Address,
                Image = user.Image
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            // حذف فیلدهای اختیاری از اعتبارسنجی
            ModelState.Remove("FirstName");
            ModelState.Remove("LastName");
            ModelState.Remove("Post");
            ModelState.Remove("NewPassword");
            ModelState.Remove("NationalCode");
            ModelState.Remove("PhoneNumber");
            ModelState.Remove("CustCodeAmel");
            ModelState.Remove("Address");
            ModelState.Remove("ImageMain");

            // بررسی یکتایی نام کاربری
            var existingUser = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == model.UserName && u.Id != model.Id);
            if (existingUser != null)
            {
                ModelState.AddModelError("UserName", "این نام کاربری قبلاً استفاده شده است.");
            }

            if (!ModelState.IsValid)
            {
                // لاگ خطاها برای دیباگ
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                foreach (var error in errors)
                {
                    Console.WriteLine($"Validation Error: {error}");
                }
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return NotFound();
            }

            // به‌روزرسانی فیلدهای غیرخالی
            if (!string.IsNullOrEmpty(model.UserName)) user.UserName = model.UserName;
            if (!string.IsNullOrEmpty(model.FirstName)) user.FirstName = model.FirstName;
            if (!string.IsNullOrEmpty(model.LastName)) user.LastName = model.LastName;
            if (!string.IsNullOrEmpty(model.Post)) user.Post = model.Post;
            if (!string.IsNullOrEmpty(model.PhoneNumber)) user.PhoneNumber = model.PhoneNumber;
            if (!string.IsNullOrEmpty(model.Address)) user.Address = model.Address;
            if (!string.IsNullOrEmpty(model.CustCodeAmel)) user.CustCodeAmel = model.CustCodeAmel;
            if (!string.IsNullOrEmpty(model.NationalCode)) user.NationalCode = model.NationalCode;

            // مدیریت تصویر
            if (!string.IsNullOrEmpty(model.ImageMain))
            {
                var imageName = Guid.NewGuid() + ".jpg";
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "UserImage", imageName);

                if (!string.IsNullOrEmpty(model.Image) && model.Image != "Male.png" && model.Image != "Shemale.png")
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "UserImage", model.Image);
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                using (var fs = new FileStream(path, FileMode.Create))
                {
                    var data = Convert.FromBase64String(model.ImageMain);
                    await fs.WriteAsync(data, 0, data.Length);
                }
                user.Image = imageName;
            }

            // مدیریت رمز عبور
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }
            }

            // به‌روزرسانی کاربر
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            return RedirectToAction("Index", "ManageUser");
        }

        [HttpGet]
        public async Task<IActionResult> CheckUserName(string userName, string id)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName == userName && u.Id != id);
            return Json(user == null);
        }


        [HttpGet]
        public async Task<IActionResult> AddUserToRole(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            var roles = _roleManager.Roles.ToList();
            var model = new AddUserToRoleViewModel() { UserId = id };

            foreach (var role in roles)
            {
                if (!await _userManager.IsInRoleAsync(user, role.Name))
                {
                    model.UserRoles.Add(new UserRolesViewModel()
                    {
                        RoleName = role.Name
                    });
                }
            }

            return View(model);
        }

        [HttpPost]
         
        public async Task<IActionResult> AddUserToRole(AddUserToRoleViewModel model)
        {
            if (model == null) return NotFound();
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return NotFound();
            var requestRoles = model.UserRoles.Where(r => r.IsSelected)
                .Select(u => u.RoleName)
                .ToList();
            var result = await _userManager.AddToRolesAsync(user, requestRoles);

            if (result.Succeeded) return RedirectToAction("index");

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> RemoveUserFromRole(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            var roles = _roleManager.Roles.ToList();
            var model = new AddUserToRoleViewModel() { UserId = id };

            foreach (var role in roles)
            {
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    model.UserRoles.Add(new UserRolesViewModel()
                    {
                        RoleName = role.Name
                    });
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveUserFromRole(AddUserToRoleViewModel model)
        {
            if (model == null) return NotFound();
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return NotFound();
            var requestRoles = model.UserRoles.Where(r => r.IsSelected)
                .Select(u => u.RoleName)
                .ToList();
            var result = await _userManager.RemoveFromRolesAsync(user, requestRoles);

            if (result.Succeeded) return RedirectToAction("index");

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpPost]
         
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            await _userManager.DeleteAsync(user);

            return RedirectToAction("Index");
        }
    }
}