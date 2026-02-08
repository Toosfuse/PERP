using System.ComponentModel.DataAnnotations;

namespace ERP.Models
{
    public class UserBasketCheckItem
    {
        public string Serial_Product { get; set; }
        public string UserName { get; set; }  // کاربر واردکننده
        public DateTime DateGenerate { get; set; } = DateTime.Today;

        // مرحله فرآیند و شماره دستگاه اختیاری در ابتدا
        public string Step_Procedure { get; set; }
        public string Number_Device { get; set; }


    }
}
