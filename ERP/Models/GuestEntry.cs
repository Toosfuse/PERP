
using System.ComponentModel.DataAnnotations;

namespace ERP.Models
{
    public class GuestEntry
    {
        public int GuestEntryID { get; set; }
        public string State { get; set; }
        public string S_persiandate { get; set; }
        public string Req_UserID { get; set; }
        public string Req_Section { get; set; }
        public string? Time { get; set; }
        public string? MahalPazirayi { get; set; }
        public List<GuestInfo> Guests { get; set; } = new List<GuestInfo>();
        public string HadafBazdid { get; set; }
        public string? Name_Sazman { get; set; }
        public string? Vorud_persiandate { get; set; }
        public string? Vorud_Saat { get; set; }
        public string? Noe_Mosaferat { get; set; }
        public string? Eghamat { get; set; }
        public string? PhoneNumber { get; set; }
        public string Bazdid { get; set; }
        public string? Gift { get; set; }
        public string? Lunch { get; set; }
        public string Jalasat { get; set; }
        public string MojavezKhodro { get; set; }
        public string Khodro { get; set; }
        public List<string> Companions { get; set; } = new List<string>();
        public string Tozihat_Darkhast { get; set; }
        public bool? TayidModirVahed { get; set; }
        public string? TozihatModirVahed { get; set; }
        public string? Tozihat_Edary { get; set; }
        public string? TarikhVorud_persiandate { get; set; }
        public string? SaateVorud { get; set; }
        public string? Tozihat_Negahbani { get; set; }
        public string? SaatKhoroj { get; set; }
        public string? Tozihat_DarkhastKonnande { get; set; }
        public string? SharhMavared { get; set; }
        public string? MasolAnjam { get; set; }
        public string? MasolPeygiry { get; set; }
        public string? MohlatAnjam { get; set; }
        public string? Tozihat_ModirAmel { get; set; }
        public string Guid { get; set; }
    }

    public class GuestInfo
    {
        public string FullName { get; set; }
        public string Position { get; set; }
    }


}

