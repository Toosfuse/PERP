namespace ERP.ViewModels.AssestViewModel
{
    public class AssetViewModel
    {
        public int Id { get; set; }
        public string AssetCode { get; set; }
        public string AssetName { get; set; }
        public int CategoryId { get; set; }
        public string CategoryTitle { get; set; }
        public bool IsActive { get; set; }
        public string CreatedAt { get; set; }
    }
}
