using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Models
{
    [Table("ProductChecks")] // اسم دقیق جدول در دیتابیس
    public class ProductCheck
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public string Serial_Product { get; set; }

        public DateTime DateGenerate { get; set; } = DateTime.Today;

        public string? Step_Procedure { get; set; }

        public string? Number_Device { get; set; }

        public string? UserName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}