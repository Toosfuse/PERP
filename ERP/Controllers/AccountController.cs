using ERP.Data;
using ERP.Models;
using ERP.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stimulsoft.Blockly.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static Stimulsoft.Report.StiRecentConnections;

namespace ERP.Controllers
{

    public class AccountController : Controller
    {
        private readonly UserManager<Users> _userManager;
        private readonly SignInManager<Users> _signInManager;
        private readonly ERPContext _context;


        public AccountController(UserManager<Users> userManager, SignInManager<Users> signInManager, ERPContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model, string ImageMain/* List<int> selectedGroups*/)
        {
                if (ImageMain != null)
                {
                    string fileNamefake = "vahid.jpg";
                    var imagename = Guid.NewGuid() + Path.GetExtension(fileNamefake);

                    var user = new Users()
                    {
                        UserName = model.UserName,
                        Email = "vhadadan@gmail.com",
                        EmailConfirmed = true,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Post = model.Post,
                        Image = imagename,
                        InTFC=true
                    };

                    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "UserImage", imagename);
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        using (BinaryWriter bw = new BinaryWriter(fs))
                        {
                            byte[] data = Convert.FromBase64String(ImageMain);
                            bw.Write(data);
                            bw.Close();
                        }
                        fs.Close();
                    }
                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Home");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
                if (ImageMain == null)
                {
                    var user = new Users()
                    {
                        UserName = model.UserName,
                        Email = "vhadadan@gmail.com",
                        EmailConfirmed = true,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Post = model.Post,
                        Image = "Male.png",
                        InTFC = true
                    };
                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Home");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
                //foreach (int selectedGroup in selectedGroups)
                //{
                //    _context.News_Selected_Group.Add(new News_Selected_Group()
                //    {
                //        NewsID = news.NewsID,
                //        GroupID = selectedGroup
                //    });
                //}
            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            if (_signInManager.IsSignedIn(User))
                return RedirectToAction("Index", "Home");
            ViewData["returnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (_signInManager.IsSignedIn(User))
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = true });
                return RedirectToAction("Index", "Home");
            }

            ViewData["returnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    model.UserName, model.Password, model.RememberMe, true);

                if (result.Succeeded)
                {
                    string getIP = HttpContext.Connection.RemoteIpAddress.ToString();
                    var username = User.Identity.Name;
                    var userid = _context.Users.Where(p => p.UserName == username).Select(p => p.Id).Single();

                    await _context.SaveChangesAsync();
                    
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        return Json(new { success = true });
                    
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);
                    return RedirectToAction("Index", "Home");
                }

                if (result.IsLockedOut)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        return Json(new { success = false, error = "اکانت شما به دلیل پنج بار ورود ناموفق به مدت پنج دقیقه قفل شده است" });
                    
                    ViewData["ErrorMessage"] = "اکانت شما به دلیل پنج بار ورود ناموفق به مدت پنج دقیقه قفل شده است";
                    return View(model);
                }

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, error = "رمزعبور یا نام کاربری اشتباه است" });
                
                ModelState.AddModelError("", "رمزعبور یا نام کاربری اشتباه است");
            }
            
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = false, error = "اطلاعات ورود نامعتبر است" });
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM model)
        {
            if (ModelState.IsValid)
            {
                var username = User.Identity.Name;
                var userid = _userManager.Users.Where(p => p.UserName == username).Select(p => p.Id).Single();
                var user = await _userManager.FindByIdAsync(userid);
                model.Token = await _userManager.GeneratePasswordResetTokenAsync(user);
                if (user == null) return RedirectToAction("Login");
                var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
                if (result.Succeeded)
                {
                    ViewData["ErrorMessage"] = "رمزعبور شما با موفقیت تغییر یافت";
                    return View("Login");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        [HttpPost]
         
        public async Task<IActionResult> IsEmailInUse(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return Json(true);
            return Json("ایمیل وارد شده از قبل موجود است");
        }

        [HttpPost]
         
        public async Task<IActionResult> IsUserNameInUse(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null) return Json(true);
            return Json("نام کاربری وارد شده از قبل موجود است");
        }

        /// <summary>
        /// بررسی وضعیت جلسه کاری - برای استفاده در AJAX
        /// </summary>
        [HttpGet]
        public IActionResult CheckSession()
        {
            bool isAuthenticated = User.Identity.IsAuthenticated;
            
            if (isAuthenticated)
            {
                return Json(new { isAuthenticated = true, userName = User.Identity.Name });
            }
            else
            {
                return Json(new { isAuthenticated = false, redirectUrl = "/Identity/Account/Login" });
            }
        }
    }
}