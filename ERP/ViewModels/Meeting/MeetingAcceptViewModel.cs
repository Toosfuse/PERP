using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.ViewModels.Meeting
{
    public class MeetingAcceptViewModel
    {
        public int MeetingAcceptID { get; set; }
        public string UserInMeetingID { get; set; }
        public string UserID { get; set; }
        public int MeetingId { get; set; }
        public bool Seen { get; set; } = false;
        public string AcknowledgedAt { get; set; } = "";

    }
}
