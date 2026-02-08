using System.ComponentModel.DataAnnotations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ERP.Models
{
    public class CoTenderItem
    {
        [Key]
        public int CoTenderItemID { get; set; }
        public int ProductID { get; set; }
        public double Quantity { get; set; }
        public string? Description { get; set; }
        public string Guid { get; set; }
        public double Price { get; set; }
        public double? Score { get; set; } // امتیاز کیفی توس فیوز
        public double? Price1 { get; set; } // افزار آزما
        public double? Score1 { get; set; }
        public double? Price2 { get; set; } // بهینه سازان
        public double? Score2 { get; set; }
        public double? Price3 { get; set; } // راد نیرو کرمان
        public double? Score3 { get; set; }
        public double? Price4 { get; set; } // سنجش نیروی هوشمند
        public double? Score4 { get; set; }
        public double? Price5 { get; set; } // کنتور سازی ایران
        public double? Score5 { get; set; }
        public double? Price6 { get; set; } //سنجش افزار آسیا
        public double? Score6 { get; set; }
        public bool IsTemp { get; set; }

    }
}
