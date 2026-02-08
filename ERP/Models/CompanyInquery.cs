using System.ComponentModel.DataAnnotations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ERP.Models
{
    public class CompanyInquery
    {
        [Key]
        public int CompanyInqueryID { get; set; }
        public string NeedNumber { get; set; } // شماره نیاز
        public string ResponseNumber { get; set; } // شماره پاسخ
        public string? RegDate { get; set; }
        public string? NeedDate { get; set; }
        public string City { get; set; }
        public double TotalPrice { get; set; }
        public string? InqueryResult { get; set; }
        public string? Winner { get; set; }
        public double? FeeCost { get; set; } // هزینه کارمزد
        public string Guid { get; set; }
        public string? Status { get; set; }
        public string CreateDate { get; set; }

    }
}
