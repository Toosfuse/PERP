namespace ERP.Models
{
    public class WorkflowSection
    {
        public int WorkflowSectionID { get; set; }              // کلید اصلی
        public string ProcessName { get; set; }   // نام فرایند (مثلاً "GuestEntryWorkflow")
        public string SectionCode { get; set; }   // کد بخش (مثلاً "start0", "TayidModirVahed1")
        public string SectionName { get; set; }   // نام نمایشی بخش (مثلاً "اعلام حضور مهمان")
        public int OrderIndex { get; set; }       // ترتیب نمایش
        public DateTime CreatedDate { get; set; } // تاریخ ایجاد
    }
}
