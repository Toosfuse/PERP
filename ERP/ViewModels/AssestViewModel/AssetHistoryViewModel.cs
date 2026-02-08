namespace ERP.ViewModels.AssestViewModel
{
    public class AssetHistoryViewModel
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
        public string AssetName { get; set; }
        public int? AssetPropertyId { get; set; }
        public string PropertyName { get; set; }
        public string FromUser { get; set; }
        public string ToUser { get; set; }
        public string AssignDate { get; set; }
        public string Description { get; set; }
    }
}
