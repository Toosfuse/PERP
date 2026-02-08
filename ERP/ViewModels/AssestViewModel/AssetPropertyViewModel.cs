namespace ERP.ViewModels.AssestViewModel
{
    public class AssetPropertyViewModel
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
        public string AssetName { get; set; }
        public string PropertyName { get; set; }
        public string PropertyValue { get; set; }
        public string SerialNumber { get; set; }
        public string Model { get; set; }
        public string Brand { get; set; }
        public bool IsActive { get; set; }
        public string CreatedAt { get; set; }
        public int? LastOwnerUserId { get; set; }
        public string LastOwnerUserName { get; set; }
    }
}
