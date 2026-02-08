using System.ComponentModel.DataAnnotations;

namespace ERP.Models
{
    public class CompanyTender
    {
        [Key]
        public int CompanyTenderID { get; set; }
        public string City { get; set; }
        public string TenderNumber { get; set; } // شماره مناقصه
        public double TotalPrice { get; set; } // قیمت کل
        public string? AlefDate { get; set; } // ارسال پاکت الف و نمونه
        public string? AllPocketDate { get; set; } // ارسال پاکت الف و نمونه
        public string? TenderResult { get; set; } // نتیجه 
        public string? Winner { get; set; }
        public double? FeeCost { get; set; } // هزینه کارمزد
        public string Guid { get; set; }
        public string? Status { get; set; }
        public string CreateDate {  get; set; }
    }
}
