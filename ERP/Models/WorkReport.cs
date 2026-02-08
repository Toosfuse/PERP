using System.ComponentModel.DataAnnotations;

namespace ERP.Models
{
    public class WorkReport
    {
        //کالاهای ارسالی
        [Key]
        public int WorkReportID { get; set; }

        public string UserID { get; set; }

        public String DateWorkReport { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? FinishTime { get; set; }
        public DateTime CreateON { get; set; }

        public string? TimeHours { get; set; }

        public string? Member { get; set; }

        public string Description { get; set; }

        public string GuidFile { get; set; }
    }
}
