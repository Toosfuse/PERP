namespace ERP.Models
{
    public class WorkflowStep
    {
        public int WorkflowStepID { get; set; }
        public string ProcessName { get; set; }
        public string SectionCode { get; set; }
        public string SectionName { get; set; }
        public int OrderIndex { get; set; }
        public string? NextSteps { get; set; } // JSON برای گیت‌وی‌ها
        public DateTime CreatedDate { get; set; }
    }
}
