using ERP.Models.asset;

namespace ERP.ViewModels.AssestViewModel
{
    public class AssetViewModel
    {
        public int Id { get; set; }
        public string AssetCode { get; set; }
        public string AssetName { get; set; }
        public string PurchaseDate { get; set; }
        public int CategoryId { get; set; }
        public string CategoryTitle { get; set; }
        public int CurrentOwnerId { get; set; }
        public string CurrentOwnerName { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public string CreatedAt { get; set; }
        public string LastModifiedDate { get; set; }
        public string LastModifiedBy { get; set; }
    }
}
