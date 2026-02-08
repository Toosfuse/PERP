namespace ERP.Models
{
    public class WorkflowInstance
    {
        public int WorkflowInstanceID { get; set; } // کلید اصلی
        public int WorkFlowID { get; set; }      // لینک به فرم GuestEntry
        public string ProcessName { get; set; }   // نام فرایند (مثلاً "GuestEntryWorkflow")
        public string Section { get; set; }        // بخش فعلی (مثل "TayidModirVahed1")
        public string AssignedUserID { get; set; } // کاربر assigned برای این بخش
        public bool IsEditor { get; set; }
        public DateTime AssignedDate { get; set; } // زمان ارجاع
        public bool IsCompleted { get; set; }
    }
}
