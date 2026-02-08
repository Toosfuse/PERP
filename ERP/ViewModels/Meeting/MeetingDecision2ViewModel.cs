namespace ERP.ViewModels.Meeting
{
    public class MeetingDecision2ViewModel
    {
        public string Description { get; set; }
        public string ResponsibleNames { get; set; } // رشته با <br/> جدا شده
        public string FollowUpNames { get; set; } // رشته با <br/> جدا شده
        public string Deadline { get; set; }
        public string Notes { get; set; }
    }
}
