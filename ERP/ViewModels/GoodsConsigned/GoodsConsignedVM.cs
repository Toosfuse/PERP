
using ERP.Models;
using ERP.ViewModels.GoodsEntry;
using System.ComponentModel.DataAnnotations;

namespace ERP.ViewModels.GoodsConsigned
{
    public class GoodsConsignedVM
    {
        public class SaveForm1
        {
            public int GoodsConsignedID { get; set; }
            [Required(ErrorMessage = "تاریخ درخواست الزامی است")]
            public string S_persiandate { get; set; }
            [Required(ErrorMessage = "شماره سریال الزامی است")]
            public string Shomare_Serial { get; set; }
            [Required(ErrorMessage = "محل دریافت الزامی است")]
            public string MahalDaryaft { get; set; }
            [Required(ErrorMessage = "تاریخ ارسال الزامی است")]
            public string Darkhast_persiandate { get; set; }
            [Required(ErrorMessage = "نام درخواست‌کننده الزامی است")]

         

            public string Name_Darkhastkonnande { get; set; }
            [Required(ErrorMessage = "شناسه Guid الزامی است")]


            public string ManagerUserID { get; set; }

            [Required(ErrorMessage = "لطفاً نوع ارسال را انتخاب کنید.")]
            public int TypeSending { get; set; } 


            public string Guid { get; set; }
            public List<ConsignedItem> Goods { get; set; } = new List<ConsignedItem>();
        }

        public class SaveForm2
        {
            public int GoodsConsignedID { get; set; }
            public string TayidMasool { get; set; }
            public string Tozihat_Masool { get; set; }
            public string Guid { get; set; }
        }

        public class SaveForm3
        {
            public int GoodsConsignedID { get; set; }
            public string Tayid_Sanaye { get; set; }
            public string Tozihat_TayidSanaye { get; set; }
            public string Guid { get; set; }
        }

        public class SaveForm4
        {
            public int GoodsConsignedID { get; set; }
            public string Tozihat_Net { get; set; }
            public string Guid { get; set; }
        }

        public class SaveForm5
        {
            public int GoodsConsignedID { get; set; }
           // public string TayidTadarokat { get; set; }
            public string Tozihat_Tadarokat { get; set; }
            public string Guid { get; set; }
        }

        public class SaveForm6
        {
            public int GoodsConsignedID { get; set; }
            public string TayidMali { get; set; }
            public string Tozihat_Mali { get; set; }
            public string Guid { get; set; }
        }

        public class SaveForm7
        {
            public int GoodsConsignedID { get; set; }
            public string TayidModirAmel { get; set; }
            public string Tozihat_ModirAmel { get; set; }
            public string Guid { get; set; }
        }

        public class SaveForm8
        {
            public int GoodsConsignedID { get; set; }
            public string Tozihat_Anbar { get; set; }
            public string Guid { get; set; }
        }

        public class SaveForm9
        {
            public int GoodsConsignedID { get; set; }
            public string Khoruj_persiandate { get; set; }
            public string TimeKhoruj_persiandate { get; set; }
            public string Tozihat_Negahbani { get; set; }
            public string Guid { get; set; }
        }


        public class SaveForm10
        {
            public int GoodsConsignedID { get; set; }
            public string S_persiandate_Enteringtheguard { get; set; } // تاریخ درخواست
            public string Shomare_Serial_Enteringtheguard { get; set; }
            public string TahvilDahande_Enteringtheguard { get; set; }
            public string SaateVorud_Enteringtheguard { get; set; }
            public List<GoodsEntryItemGuardDto> GoodsGuard_Enteringtheguard { get; set; }
            public string Guid_Enteringtheguard { get; set; }
            public string? SaateKhoruj_Enteringtheguard { get; set; }
            public string? Khodro_Enteringtheguard { get; set; }
            public string? Phone_Enteringtheguard { get; set; }
            public string Guid { get; set; }
        }

        // Additional forms for branches like rejection handling
        public class SaveForm11
        {
            public int GoodsConsignedID { get; set; }
            public string Tozihat_ModirAmel { get; set; } // Reuse or add specific fields for rejection reason
            public string Guid { get; set; }
        }

    }
}
