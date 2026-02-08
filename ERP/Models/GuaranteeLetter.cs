using System.ComponentModel.DataAnnotations;

namespace ERP.Models
{
    public class GuaranteeLetter
    {
        [Key]
        public int GuaranteeLetterId { get; set; }
        public string DistributionCompanyName { get; set; }
        public string GuaranteeNumber { get; set; }
        public string GuaranteeType { get; set; }
        public string Amount { get; set; }
        public string IssueDate { get; set; }
        public string ExpirationDate { get; set; }
        public DateTime IssueDateGregorian { get; set; } // تاریخ میلادی
        public DateTime ExpirationDateGregorian { get; set; } // تاریخ میلادی
        public string BankName { get; set; }
        public string ContractNumber { get; set; }
        public string ContractStatus { get; set; }
        public string GuaranteeStatus { get; set; }
        public string OverdueProgressPercentage { get; set; }
        public string? FollowUpResult { get; set; }
        public string RegDate { get; set; }
        public DateTime RegDateGregorian { get; set; } // تاریخ میلادی
    }
}