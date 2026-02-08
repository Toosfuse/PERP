
using ERP.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Models
{
    public class GoodsConsignedItemGuard
    {
        [Key]
        public int GoodsConsignedItemGuardID { get; set; }

        public int GoodsConsignedID { get; set; }

        [ForeignKey("GoodsConsignedID")]
        public virtual GoodsConsigned GoodsConsigned { get; set; } = null!;

        public string SharhKala { get; set; } = null!;
        public string Tedad { get; set; }
        public string? Vahed { get; set; }
        public string? Description { get; set; }
        public string Guid { get; set; } = null!;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
