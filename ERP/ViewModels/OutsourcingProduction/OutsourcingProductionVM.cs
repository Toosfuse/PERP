using ERP.Models;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ERP.ViewModels.OutsourcingProduction
{
    public class OutsourcingProductionVM
    {
        public class SaveForm1
        {
            public int OutsourcingProductionID { get; set; }
            [Required(ErrorMessage = "تاریخ درخواست الزامی است")]
            public string S_persiandate { get; set; } = string.Empty;
            [Required(ErrorMessage = "شماره سریال الزامی است")]
            public string Shomare_Serial { get; set; } = string.Empty;
            [Required(ErrorMessage = "محل دریافت الزامی است")]
            public string MahalDaryaft { get; set; } = string.Empty;
            [Required(ErrorMessage = "تاریخ ارسال الزامی است")]
            public string Darkhast_persiandate { get; set; } = string.Empty;
            [Required(ErrorMessage = "نام درخواست‌کننده الزامی است")]
            public string Name_Darkhastkonnande { get; set; } = string.Empty;
            public string ManagerUserID { get; set; } = string.Empty;
            public string Guid { get; set; } = string.Empty;
            public List<OutsourcingItem> Goods { get; set; } = new();
        }

        public class SaveForm2
        {
            public int OutsourcingProductionID { get; set; }
            public string TayidMasool { get; set; } = string.Empty;
            public string Tozihat_Masool { get; set; } = string.Empty;
            public string Guid { get; set; } = string.Empty;
        }

        public class SaveForm3
        {
            public int OutsourcingProductionID { get; set; }
            public string Tayid_Sanaye { get; set; } = string.Empty;
            public string Tozihat_TayidSanaye { get; set; } = string.Empty;
            public string Guid { get; set; } = string.Empty;
        }

        public class SaveForm4
        {
            public int OutsourcingProductionID { get; set; }
            public string Tozihat_Net { get; set; } = string.Empty;
            public string Guid { get; set; } = string.Empty;
        }

        public class SaveForm5
        {
            public int OutsourcingProductionID { get; set; }
            public string Tozihat_Tadarokat { get; set; } = string.Empty;
            public string Guid { get; set; } = string.Empty;
        }

        public class SaveForm6
        {
            public int OutsourcingProductionID { get; set; }
            public string TayidMali { get; set; } = string.Empty;
            public string Tozihat_Mali { get; set; } = string.Empty;
            public string Guid { get; set; } = string.Empty;
        }

        public class SaveForm7
        {
            public int OutsourcingProductionID { get; set; }
            public string TayidModirAmel { get; set; } = string.Empty;
            public string Tozihat_ModirAmel { get; set; } = string.Empty;
            public string Guid { get; set; } = string.Empty;
        }

        public class SaveForm8
        {
            public int OutsourcingProductionID { get; set; }
            public string Tozihat_Anbar { get; set; } = string.Empty;
            public string Guid { get; set; } = string.Empty;
        }

        public class SaveForm9
        {
            public int OutsourcingProductionID { get; set; }
            public string Khoruj_persiandate { get; set; } = string.Empty;
            public string TimeKhoruj_persiandate { get; set; } = string.Empty;
            public string Tozihat_Negahbani { get; set; } = string.Empty;
            public string Guid { get; set; } = string.Empty;
        }

        public class SaveForm10
        {
            public int OutsourcingProductionID { get; set; }
            public string Tozihat_ModirAmel { get; set; } = string.Empty;
            public string Guid { get; set; } = string.Empty;
        }
    }
}