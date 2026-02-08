using System.ComponentModel.DataAnnotations;

namespace ERP.ViewModels.ViewProcess
{
    public class ViewProcessViewModel
    {
        [Key]
        public int ViewProcessID { get; set; }
        public string SenderID { get; set; }
        public string ReceiverID { get; set; }
        public string SendDateTime { get; set; }
        public string Guid { get; set; }
        public bool IsView { get; set; }
        public string? ViewDateTime { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
    }
}
