using ERP.ViewModels.GoodsDeparture;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace ERP.Models
{
    public class GoodsDeparture
    {
        //کالا های خارج
        public int GoodsDepartureID { get; set; }
        public string? State { get; set; } // e.g., "start0", "TayidMali1", "Security2", "End3"
        public string? S_persiandate { get; set; } // Request date (Persian)
        public string? Shomareh_Anabr { get; set; } // Warehouse number
        public string? Name_Anbar { get; set; } // Warehouse name
        public string? Shomareh_havale { get; set; } // Delivery note number
        public string? Date_persiandate { get; set; } // Delivery note date (Persian)
        public string? Code_TahvilGirandeh { get; set; } // Recipient code
        public string? Nam_TahvilGirandeh { get; set; } // Recipient name
        public string? Tozihat { get; set; } // General comments
        public string? Taeed { get; set; } // Finance approval (0: Yes, 1: No)
        public string? Tozihat_Mali { get; set; } // Finance comments
        public string? IsBazrasi { get; set; } // Inspection done (0: Yes, 1: No)
        public string? Noe_Hamel { get; set; } // Carrier type (0: Person, 1: Company vehicle, 2: Non-company vehicle)
        public List<VehicleExit>? VehicleExit { get; set; } = new List<VehicleExit>();
        public string? Tozihat_Entezamat { get; set; } // Security comments
        public string Guid { get; set; } // Workflow instance GUID
    }

    public class VehicleExit
    {
        public string S_persiandate_var { get; set; }
        public string ExitTime { get; set; }
        public string Shomareh_havale_khoruji { get; set; }
        public string Shomareh_barnameh { get; set; }
        public string Shomareh_plak { get; set; }
        public string Phone_ranandeh { get; set; }
        public string Name_ranandeh { get; set; }
    }

}