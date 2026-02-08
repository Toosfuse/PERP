using System.ComponentModel.DataAnnotations;

namespace ERP.ViewModels
{
    public class OrderViewModel
    {
     
        [Display(Name = "شماره سفارش")]
        public string OrderNumber { get; set; }

        [Display(Name = "شماره ثبت سفارش")]
        public string OrderRegNumber { get; set; }

    
        [Display(Name = "شهر مقصد")]
        public int? CityId { get; set; }

       
        [Display(Name = "سریال اولیه")]
        public string StartSerial { get; set; }

       
        [Display(Name = "سریال انتهایی")]
        public string EndSerial { get; set; }

 
        [Display(Name = "شماره محصول")]
        public string CodeProduct { get; set; }

        public long SerialCount { get; set; }

        [Display(Name = "تاریخ ثبت")]
        public string? RegDate { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string RegDatePersian { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string CustomerName { get; set; }

        public Progress Progress {  get; set; }

        public PackageViewModel Packages { get; set; } = new PackageViewModel();
        
        // برای نمایش در لیست (Index)
        public int Id { get; set; }
        
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string CityName { get; set; }
    }
    public enum Progress
    {
        NotComplate=0, 
        IsWorking=1,
        ISComplate = 2,
    }
}