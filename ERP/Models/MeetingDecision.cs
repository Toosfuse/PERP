using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Models
{
    // تصمیمات جلسه
    public class MeetingDecision
    {
        [Key]
        public int Id { get; set; }
        public string Description { get; set; }  // شرح تصمیم
        public string ResponsiblePersonId { get; set; }  // مسئول انجام
        public string FollowUpPersonId { get; set; }  // مسئول پیگیری

        public String Deadline { get; set; }  // مهلت انجام
        public string Notes { get; set; }  // توضیحات

        public string State { get; set; } = "در حال بررسی"; // وضعیت = در حال انجام، انجام شده با تاخیر، ابتال صورت جلسه، در حال بررسی
        public string Datefollowup1 { get; set; } = ""; // تاریخ پیگیری اول
        public string Resultfollowup1 { get; set; } = "";  // نتیجه پیگیری اول
        public string Datefollowup2 { get; set; } = "";  // تاریخ پیگیری دوم
        public string Resultfollowup2 { get; set; } = "";  // نتیجه پیگیری دوم

        [ForeignKey("Meeting")]
        public int MeetingId { get; set; }
        public virtual Meeting Meeting { get; set; }

    }
}
