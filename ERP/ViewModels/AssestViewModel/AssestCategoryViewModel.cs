namespace ERP.ViewModels.AssestViewModel
{
    public class AssestCategoryViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int? ParentId { get; set; }
        public string? ParentTitle { get; set; } // برای نمایش parent در Grid
        public bool IsActive { get; set; }
    }

    public class AssestCategoryPageViewModel
    {
        public List<AssestCategoryViewModel> Categories { get; set; } = new List<AssestCategoryViewModel>();
    }

}
