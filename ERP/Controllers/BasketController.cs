using ERP.Data;
using ERP.Interface;
using ERP.Models;
using ERP.ViewModels.Basket;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ERP.Controllers
{
    [Authorize]
    public class BasketController : Controller
    {
        private readonly IBasketRepository _basketRepository;
        private readonly UserManager<Users> _userManager;
        private readonly ERPContext _dbContext;

        private string BasketId => User.Identity?.IsAuthenticated == true
            ? $"basket:{User.Identity.Name}"
            : $"basket:{HttpContext.Session.Id}";

        public BasketController(
            IBasketRepository basketRepository,
            UserManager<Users> userManager,
            ERPContext dbContext)
        {
            _basketRepository = basketRepository;
            _userManager = userManager;
            _dbContext = dbContext;
        }

        // GET: Index
        public async Task<IActionResult> Index()
        {
            var basket = await _basketRepository.GetBasketAsync(BasketId)
                         ?? new UserBasketCheck(BasketId);
            basket.basketCheckItems ??= new List<UserBasketCheckItem>();

            var viewModel = new BasketIndexViewModel
            {
                Basket = basket,
                AvailableUsers = await GetUsersSelectListAsync()
            };

            // بازیابی تنظیمات ثابت از TempData (بعد از اسکن و رفرش)
            if (TempData["CurrentUserName"] != null)
            {
                viewModel.CurrentUserName = TempData["CurrentUserName"]?.ToString();
                viewModel.CurrentStepProcedure = TempData["CurrentStepProcedure"]?.ToString();
                viewModel.CurrentNumberDevice = TempData["CurrentNumberDevice"]?.ToString();
            }
            else
            {
                // اگر سبد خالی نیست، از آخرین آیتم تنظیمات رو بگیر
                var lastItem = basket.basketCheckItems.LastOrDefault();
                if (lastItem != null)
                {
                    viewModel.CurrentUserName = lastItem.UserName;
                    viewModel.CurrentStepProcedure = lastItem.Step_Procedure;
                    viewModel.CurrentNumberDevice = lastItem.Number_Device;
                }
                else
                {
                    viewModel.CurrentUserName = User.Identity?.Name;
                }
            }

            return View(viewModel);
        }

        // POST: AddScannedItem (اسکن بارکد با AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> AddScannedItem([FromBody] AddScannedItemModel model)
        {
            if (!ModelState.IsValid ||  string.IsNullOrEmpty(model.UserName))
            {
                return Json(new { success = false, message = "داده نامعتبر" });
            }

            var basket = await _basketRepository.GetBasketAsync(BasketId)
                         ?? new UserBasketCheck(BasketId) { basketCheckItems = new List<UserBasketCheckItem>() };

            if (basket.basketCheckItems.Any(i => i.Serial_Product == model.SerialProduct))
            {
                return Json(new { success = false, message = "این سریال قبلاً اضافه شده" });
            }

            basket.basketCheckItems.Add(new UserBasketCheckItem
            {
                Serial_Product = model.SerialProduct,
                UserName = model.UserName,
                Step_Procedure = model.StepProcedure,
                Number_Device = model.NumberDevice,
                DateGenerate = DateTime.Today
            });

            await _basketRepository.UpdateBasketAsync(basket);

            // حفظ تنظیمات برای بعد از رفرش
            TempData["CurrentUserName"] = model.UserName;
            TempData["CurrentStepProcedure"] = model.StepProcedure;
            TempData["CurrentNumberDevice"] = model.NumberDevice;

            return Json(new { success = true });
        }

        // مدل کمکی برای JSON
        public class AddScannedItemModel
        {
            public string SerialProduct { get; set; }
            public string UserName { get; set; }
            public string StepProcedure { get; set; }
            public string NumberDevice { get; set; }
        }

        // POST: RemoveItem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem(string serialProduct)
        {
            var basket = await _basketRepository.GetBasketAsync(BasketId);
            if (basket != null)
            {
                var item = basket.basketCheckItems.FirstOrDefault(i => i.Serial_Product == serialProduct);
                if (item != null)
                {
                    basket.basketCheckItems.Remove(item);
                    await _basketRepository.UpdateBasketAsync(basket);
                    TempData["SuccessMessage"] = $"محصول با سریال {serialProduct} حذف شد.";
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Clear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear()
        {
            await _basketRepository.DeleteBasketAsync(BasketId);
            TempData["SuccessMessage"] = "سبد پاک شد.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizeBasket()
        {
            var basket = await _basketRepository.GetBasketAsync(BasketId);
            if (basket == null || !basket.basketCheckItems.Any())
            {
                TempData["ErrorMessage"] = "سبد خالی است!";
                return RedirectToAction(nameof(Index));
            }

            // تبدیل آیتم‌های سبد به مدل دیتابیس
            var productChecks = basket.basketCheckItems.Select(item => new ProductCheck
            {
                Serial_Product = item.Serial_Product,
                DateGenerate = item.DateGenerate,
                Step_Procedure = item.Step_Procedure,
                Number_Device = item.Number_Device,
                UserName = item.UserName
            }).ToList();

            // ذخیره با EF Core (ایمن، سریع، بدون SQL Injection)
            _dbContext.ProductChecks.AddRange(productChecks);
            await _dbContext.SaveChangesAsync();

            // پاک کردن سبد از Redis
            await _basketRepository.DeleteBasketAsync(BasketId);

            TempData["SuccessMessage"] = $"{productChecks.Count} محصول با موفقیت در سیستم ثبت شد.";
            return RedirectToAction(nameof(Index));
        }
        // لیست کاربران
        private async Task<List<SelectListItem>> GetUsersSelectListAsync()
        {
            var users = await _userManager.Users
                .AsNoTracking()
                .Select(u => new { u.UserName, u.FirstName, u.LastName })
                .OrderBy(u => u.FirstName ?? "")
                .ThenBy(u => u.LastName ?? "")
                .ToListAsync();

            return users.Select(u => new SelectListItem
            {
                Value = u.UserName,
                Text = string.IsNullOrEmpty(u.FirstName + u.LastName)
                    ? u.UserName
                    : $"{u.FirstName} {u.LastName} ({u.UserName})"
            }).ToList();
        }
    }
}