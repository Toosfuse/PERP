namespace ERP.Models
{
    public class WorkflowAccess
    {
        public int WorkflowAccessID { get; set; }              // کلید اصلی
        public string UserID{ get; set; } // شناسه داخلی کاربر 
        public string ProcessName { get; set; }   // نام فرایند (مثلاً "GuestEntryWorkflow")
        public string Section { get; set; }       // کد بخش (مثلاً "start0", "TayidModirVahed1"، یا "*")
        public string Role { get; set; }          // نقش کاربر (مثلاً "Observer", "Requester")
        public DateTime CreatedDate { get; set; } // تاریخ ایجاد دسترسی
    }
}
