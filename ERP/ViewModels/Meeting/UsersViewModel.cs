namespace ERP.ViewModels.Meeting
{
    public class UsersViewModel
    {
        public string Id { get; set; } 
        public string FullName { get; set; } // برای نمایش نام و نام خانوادگی
        public string Image { get; set; } // مسیر تصویر
        public string Post { get; set; } // سمت یا نقش
        public string Type { get; set; } // "User" یا "NonUser" برای تمایز
    }
}
