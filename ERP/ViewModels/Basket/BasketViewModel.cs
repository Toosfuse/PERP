using Microsoft.AspNetCore.Mvc.Rendering;
using ERP.Models;

namespace ERP.ViewModels.Basket
{
    public class BasketIndexViewModel
    {
        public UserBasketCheck Basket { get; set; } = new UserBasketCheck(string.Empty);

        public UserBasketCheckItem NewItem { get; set; } = new UserBasketCheckItem(); // برای آینده

        public List<SelectListItem> AvailableUsers { get; set; } = new List<SelectListItem>();

        // جدید: ذخیره تنظیمات ثابت برای حفظ بعد از رفرش
        public string CurrentUserName { get; set; }
        public string CurrentStepProcedure { get; set; }
        public string CurrentNumberDevice { get; set; }
    }
}