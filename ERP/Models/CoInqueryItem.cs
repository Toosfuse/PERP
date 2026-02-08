using System.ComponentModel.DataAnnotations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ERP.Models
{
    public class CoInqueryItem
    {
        [Key]
        public int CoInqueryItemID { get; set; }
        public int ProductID { get; set; }
        public double Quantity { get; set; }
        public string? Description { get; set; }
        public string Guid { get; set; }
        public double Price { get; set; }
        public double? Price1 { get; set; } // افزار آزما
        public double? Price2 { get; set; } // بهینه سازان
        public double? Price3 { get; set; } // راد نیرو کرمان
        public double? Price4 { get; set; } // سنجش نیروی هوشمند
        public double? Price5 { get; set; } // کنتور سازی ایران
        public double? Price6 { get; set; } //سنجش افزار آسیا
        public bool IsTemp { get; set; }
    }
}
