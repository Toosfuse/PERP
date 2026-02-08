using ERP.Models;

namespace ERP.Models
{
    public class GoodsEntryItemGuard
    {
        public int GoodsEntryItemGuardID { get; set; }
        public int GoodsEntryID { get; set; }
        public string SharhKala { get; set; }
        public string Tedad { get; set; }
        public string Vahed { get; set; }
        public string? Description { get; set; }
        public string Guid { get; set; }
    }
}
