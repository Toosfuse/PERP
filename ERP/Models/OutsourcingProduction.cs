using System;
using System.Collections.Generic;

namespace ERP.Models
{
    public class OutsourcingProduction
    {
        public int OutsourcingProductionID { get; set; }
        public string? State { get; set; }
        public string? S_persiandate { get; set; }
        public string? Shomare_Serial { get; set; }
        public string? Darkhast_persiandate { get; set; }
        public string? Name_Darkhastkonnande { get; set; }
        public string? MahalDaryaft { get; set; }
        public List<OutsourcingItem>? OutsourcingItems { get; set; } = new();

        public string? TayidMasool { get; set; }
        public string? Tozihat_Masool { get; set; }

        public string? Tayid_Sanaye { get; set; }
        public string? Tozihat_TayidSanaye { get; set; }
        public string? Tozihat_Net { get; set; }
        public string? TayidTadarokat { get; set; }
        public string? Tozihat_Tadarokat { get; set; }
        public string? TayidMali { get; set; }
        public string? Tozihat_Mali { get; set; }
        public string? TayidModirAmel { get; set; }
        public string? Tozihat_ModirAmel { get; set; }
        public string? Tozihat_Anbar { get; set; }
        public string? Khoruj_persiandate { get; set; }
        public string? TimeKhoruj_persiandate { get; set; }
        public string? Tozihat_Negahbani { get; set; }
        public string Guid { get; set; } = string.Empty;
    }

    public class OutsourcingItem
    {
        public int OutsourcingItemID { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
