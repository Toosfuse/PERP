using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Models
{
    public class SMTSecondary
    {
        [Key]
        public long Id { get; set; }

        [ForeignKey("SMT")]
        public long SMTId { get; set; }
        public SMT SMT { get; set; }

        public DateTime SecondaryDate { get; set; }
        public string SecondaryDatePersian { get; set; }
        public string Username { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsDelete { get; set; } = false;
    }
}
