namespace ERP.ViewModels.Meeting
{
    public class MeetingDecisionViewModel
    {
        public int Id { get; set; }
        public string Description { get; set; }

        // برای نمایش در MultiSelect
        public List<string> ResponsiblePersonIds { get; set; } = new List<string>();
        public List<string> FollowUpPersonIds { get; set; } = new List<string>();

        public string Deadline { get; set; }
        public string Notes { get; set; }
        public string State { get; set; }

        public string Datefollowup1 { get; set; }
        public string Resultfollowup1 { get; set; }
        public string Datefollowup2 { get; set; }
        public string Resultfollowup2 { get; set; }

        public int MeetingId { get; set; }
    }
}
