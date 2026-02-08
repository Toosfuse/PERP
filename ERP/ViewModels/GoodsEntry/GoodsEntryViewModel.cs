using ERP.Models;
using System.ComponentModel.DataAnnotations;

namespace ERP.ViewModels.GoodsEntry
{


    public class SaveForm1 
    {
        public int GoodsEntryID { get; set; }
        public string S_persiandate { get; set; } // تاریخ درخواست
        public string Shomare_Serial { get; set; }
        public string TahvilDahande { get; set; }
        public string SaateVorud { get; set; }
        public List<GoodsEntryItemGuardDto> GoodsGuard { get; set; } 
        public string Guid { get; set; }
        public string? SaateKhoruj { get; set; }
        public string? Khodro { get; set; }
        public string? Phone { get; set; }
    }

    public class SaveForm2 
    {
        public int GoodsEntryID { get; set; }
        public string? NoeKala { get; set; }
        public string? BarrasiQC { get; set; }
        public string? TozihatAnbar { get; set; }
        public string? Shomarekala { get; set; }
        public string? ShomareBargasht { get; set; }
        public string? S_persiandatetoQC { get; set; }
        public string? SerialAnbar { get; set; }
        public string? ShomarePPAP { get; set; }
        public string Guid { get; set; }
        public string UnitUserID { get; set; }
        public List<GoodsEntryItemDto> Goods { get; set; }
    }

    public class SaveForm3
    {
        public int GoodsEntryID { get; set; }
        public string TozihatKeyfiat { get; set; }
        public string Guid { get; set; }
    }
    public class SaveForm4
    {
        public int GoodsEntryID { get; set; }
        public string Guid { get; set; }

    }
    public class SaveForm5
    {
        public int GoodsEntryID { get; set; }
        public string Guid { get; set; }
        public string? isTahvil { get; set; }
        public string? TozihatOther { get; set; }

    }
    public class SaveForm6
    {
        public int GoodsEntryID { get; set; }
        public string Guid { get; set; }
        public string? TozihatSanaye { get; set; }

    }

    public class SaveForm7
    {
        public int GoodsEntryID { get; set; }
        public string Guid { get; set; }
        public string? isTayedTolid { get; set; }
        public string? TozihatTolid { get; set; }

    }
    public class SaveForm8
    {
        public int GoodsEntryID { get; set; }
        public string Guid { get; set; }
        public List<GoodsEntryItem> Items { get; set; }

    }
    public class SaveForm9
    {
        public int GoodsEntryID { get; set; }
        public string Guid { get; set; }

    }

    public class SaveFormEng
    {
        public int GoodsEntryID { get; set; }
        public string Guid { get; set; }

        [Required(ErrorMessage = "نظر نهایی الزامی است")]
        public bool IsApprovedByEng { get; set; }  // true = تأیید، false = رد

        [Required(ErrorMessage = "توضیحات الزامی است")]
        [MinLength(10, ErrorMessage = "توضیحات باید حداقل ۱۰ کاراکتر باشد")]
        public string TozihatEng { get; set; }  // نظر فنی مهندسی
    }

    public class GoodsEntryItemDto
    {
        public int GoodsEntryID { get; set; }
        public string NameKala { get; set; }
        public string NameTaminkonandeh { get; set; }
        public string ShonarehResid { get; set; }
        public string Tedad { get; set; }
        public string Vahed { get; set; }
        public string Parameter { get; set; }
        public string Azmoon { get; set; }
        public string TedadNemoneh { get; set; }
        public string Natigeh { get; set; }
        public string Tozihat { get; set; }
        public string Guid { get; set; }
    } 
    public class GoodsEntryItemGuardDto
    {
        public int GoodsEntryID { get; set; }
        public string SharhKala { get; set; }
        public string Tedad { get; set; }
     
        public string? Description { get; set; }
        public string Guid { get; set; }
    }
}
