using System.ComponentModel.DataAnnotations;

namespace ERP.Models
{
    public class ViewProcess
    {
        [Key]
        public int ViewProcessID { get; set; }
        public string SenderID { get; set; }
        public string ReceiverID { get; set; }
        public DateTime SendDateTime { get; set; }
        public string Guid { get; set; }
        public bool IsView { get; set; }
        public DateTime? ViewDateTime { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }

    }

}
