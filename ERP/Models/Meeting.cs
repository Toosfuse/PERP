using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Models
{
    // جلسه
    public class Meeting
    {
        [Key]
        public int Id { get; set; }
        // اضافه ها
        public string MeetingNumber { get; set; }  // شماره جلسه
        public string StartTime { get; set; }  // ساعت شروع
        public string EndTime { get; set; }  // ساعت پایان

        public string Title { get; set; }  // عنوان جلسه
        public DateTime Date { get; set; } = DateTime.Now;  // تاریخ جلسه
        public String Tarikh { get; set; }  // تاریخ جلسه شمسی
        public string Input { get; set; }  // ورودی جلسه
        public string Guid { get; set; }
        public string State { get; set; }
        
        // دبیر جلسه
        [ForeignKey("Users")]
        public string SecretaryId { get; set; }
        public virtual Users Secretary { get; set; }
        public string Attendees { get; set; } // حاضرین جلسه
        public string? Absentees { get; set; } // غایبین جلسه

        // تصمیمات جلسه
        public virtual ICollection<MeetingDecision> MeetingDecisions { get; set; }
        public virtual ICollection<MeetingAccept> MeetingAccept { get; set; }
    }
}
