using ERP.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERP.Models
{
    public class GoodsConsignedItem
    {
        [Key]
        public int ConsignedItemID { get; set; }

        public int GoodsConsignedID { get; set; }

        // این دو خط حتماً اضافه شود
        [ForeignKey("GoodsConsignedID")]
        public virtual GoodsConsigned GoodsConsigned { get; set; } = null!;

        public string ItemName { get; set; } = null!;
        public int Quantity { get; set; }
        public string Description { get; set; } = null!;
    }
}
