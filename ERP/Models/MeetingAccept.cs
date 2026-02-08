using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Models
{
    public class MeetingAccept
    {
        [Key]
        public int MeetingAcceptID { get; set; }
        public string UserInMeetingID { get; set; }

        [ForeignKey("Meeting")]
        public int MeetingId { get; set; }
        public bool Seen { get; set; } = false; 
        public string AcknowledgedAt { get; set; } = "";
        public virtual Meeting Meeting { get; set; }

    }
}
