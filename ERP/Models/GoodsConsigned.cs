using System.ComponentModel.DataAnnotations;

namespace ERP.Models
{


    public class GoodsConsigned
    {
        public int GoodsConsignedID { get; set; }
        public string? State { get; set; } // e.g., "start0", "TayidMasool1", "TayidSanaye2", "TayidNet3", "TayidTadarokat4", "TayidMali5", "TayidModirAmel6", "Anbar7", "Negahbani8", "End9"
        public string? S_persiandate { get; set; } // تاریخ درخواست
        public string? Shomare_Serial { get; set; } // شماره سریال
        public string? Darkhast_persiandate { get; set; } // تاریخ ارسال
        public string? Name_Darkhastkonnande { get; set; } // نام درخواست کننده
        public string? MahalDaryaft { get; set; } // محل دریافت کالا
        public int TypeSending { get; set; }
        public List<ConsignedItem>? ConsignedItem { get; set; } = new List<ConsignedItem>();
        public virtual ICollection<GoodsConsignedItemGuard> ReturnedItems { get; set; } = new List<GoodsConsignedItemGuard>();
        // تایید مسئول درخواست کننده
        public string? TayidMasool { get; set; } // Unit manager approval (0: Yes, 1: No) // آیا درخواست مورد تایید میباشد
        public string? Tozihat_Masool { get; set; } // توضیحات مسئول

        // بررسی و تایید - واحد صنایع
        public string? Tayid_Sanaye { get; set; } // Industries approval (0: Yes, 1: No)
        public string? Tozihat_TayidSanaye { get; set; } // Industries comments
        public string? Tozihat_Net { get; set; } // Net comments
        public string? TayidTadarokat { get; set; } // Procurement approval (0: Yes, 1: No)
        public string? Tozihat_Tadarokat { get; set; } // Procurement comments
        public string? TayidMali { get; set; } // Finance approval (0: Yes, 1: No)
        public string? Tozihat_Mali { get; set; } // Finance comments
        public string? TayidModirAmel { get; set; } // CEO approval (0: Yes, 1: No)
        public string? Tozihat_ModirAmel { get; set; } // CEO comments
        public string? Tozihat_Anbar { get; set; } // Warehouse comments
        public string? Khoruj_persiandate { get; set; } // Exit date (Persian)
        public string? TimeKhoruj_persiandate { get; set; } // Exit Time date (Persian)
        public string? Tozihat_Negahbani { get; set; } // Security comments




        public string? S_persiandate_Enteringtheguard { get; set; } // تاریخ درخواست
        public string? Shomare_Serial_Enteringtheguard { get; set; }//شماره سریال
        public string? TahvilDahande_Enteringtheguard { get; set; }//تاریخ تحویل
        public string? SaateVorud_Enteringtheguard { get; set; }//ساعت ورود
        public string? SaateKhoruj_Enteringtheguard { get; set; }//ساعت خروج
        public string? Khodro_Enteringtheguard { get; set; }//خودرو
        public string? Phone_Enteringtheguard { get; set; }//شماره تلفن
         
        public string Guid { get; set; } // Workflow instance GUID
    }
    public class ConsignedItem
    {
        //public string ItemCode { get; set; } // Assuming fields for gridVar001; adjust based on actual grid structure
        public string ItemName { get; set; } // مشخصات فنی
        public int Quantity { get; set; }   // تعداد
        public string Description { get; set; } // علت ارسال
    }

}
