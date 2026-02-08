using System.ComponentModel.DataAnnotations;

namespace ERP.Models
{
    public class FileDB
    {
        [Key]
        public int FileID { get; set; }
        public string Name { get; set; }
        public string FileType { get; set; }
        public string Extension { get; set; }
        public string Description { get; set; }
        public string Guid { get; set; }
        public DateTime CreatedOn { get; set; }
        public byte[] Data { get; set; }
        public bool IsTemp { get; set; }
    }

}
