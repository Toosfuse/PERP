using System.ComponentModel.DataAnnotations;

namespace ERP.ViewModels
{
    public class PackageViewModel
    {
        public int Id { get; set; } // برای ویرایش و ارسال به کنترلر

        // فیلدهای ID که به سرور ارسال می‌شوند
        
        [Display(Name = "نوع کنتور")]
        public int? MeterTypeId { get; set; } = 2;

        [Display(Name = "نوع پایه")]
        public int? MeterBaseId { get; set; } = 2;

       
        [Display(Name = "نوع پوشش")]
        public int? CoverTypeId { get; set; } = 2;

    
        [Display(Name = "نوع بسته‌بندی")]
        public int? PackageTypeId { get; set; } = 2;

        [Display(Name = "نوع ماژول")]
        public int? ModuleTypeId { get; set; }=2;

        [Display(Name = "نوع برد")]
        public int? BoardTypeId { get; set; } = 2;

        [Display(Name = "ملحقات")]
        public int? AccessoriesTypeId { get; set; } = 2;

        [Display(Name = "پورت RS")]
        public int? RsPortTypeId { get; set; }=2;

 
        [Display(Name = "سریال شروع")]
        public string StartSerial { get; set; }

        [Display(Name = "سریال پایان")]
        public string EndSerial { get; set; }

        [Display(Name = "تعداد کنتور")]
       
        public int? MeterCount { get; set; }

        [Display(Name = "تعداد بسته")]
     
        public int? PackageCount { get; set; } = 1;

        [Display(Name = "نسخه فریم‌ور")]
        public string FrimWareVersion { get; set; }

        [Display(Name = "نام پروفایل")]
        public string ProfileName { get; set; }

        [Display(Name = "چک‌سام")]
        public string CheckSum { get; set; }

        [Display(Name = "نمایش در اسکن بارکد")]
        public bool ShowInBarcodeScan { get; set; }



        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string MeterTypeName { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string MeterBaseName { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string CoverTypeName { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string PackageTypeName { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string ModuleTypeName { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string BoardTypeName { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string AccessoriesTypeName { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string RsPortTypeName { get; set; }
    }
}