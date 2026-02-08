using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static NPOI.HSSF.Util.HSSFColor;

namespace ERP.Models
{
    public class GoodsEntry
    {
        //ورود کالا
        [Key]
        public int GoodsEntryID { get; set; }

        public string State { get; set; } // وضعیت workflow


        public string S_persiandate { get; set; } // تاریخ درخواست
        public string Shomare_Serial { get; set; }
        public string TahvilDahande { get; set; }
        public string SaateVorud { get; set; }
        public string? SaateKhoruj { get; set; }
        public string? Khodro { get; set; }
        public string? Phone { get; set; }

        //بخش دوم
        public string? NoeKala { get; set; }
        public string? BarrasiQC { get; set; }
        public string? TozihatAnbar { get; set; }
        public string? Shomarekala { get; set; }
        public string? ShomareBargasht { get; set; }
        public string? S_persiandatetoQC { get; set; } 
        public string? SerialAnbar { get; set; } 
        public string? ShomarePPAP { get; set; } 
        public string Guid { get; set; }

        // بخش سوم کنترل کیفیت
        public string? TozihatKeyfiat { get; set; }

        //بخش چهارم موردی ندارد


        //بخش پنجم Other
        public string? isTahvil { get; set; }
        public string? TozihatOther { get; set; }


        //بخش ششم واحد صنایع
        public string? TozihatSanaye { get; set; }

        //بخش هفتم واحد تولید
        public string? isTayedTolid { get; set; }
        public string? TozihatTolid { get; set; }

        public string? TozihatEng { get; set; }        
        public bool? IsApprovedByEng { get; set; }

       
    }
    
}