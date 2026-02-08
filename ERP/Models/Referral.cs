using System.ComponentModel.DataAnnotations;

namespace ERP.Models
{
    public class Referral
    {
        //ارجاع
        [Key]
        public int ReferralID { get; set; }
        public string SenderID { get; set; }
        public string ReceiverID { get; set; }
        public DateTime CreateON { get; set; }
        public string? Description { get; set; }
        public DateTime? FirstView { get; set; }
        public string Guid { get; set; }
        public string? GuidGroup { get; set; }
    }
}
