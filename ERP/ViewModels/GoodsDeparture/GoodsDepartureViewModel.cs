using ERP.Models;

namespace ERP.ViewModels.GoodsDeparture
{
    public class SaveForm1
    {
        public int GoodsDepartureID { get; set; }
        public string S_persiandate { get; set; }
        public string Shomareh_Anabr { get; set; }
        public string Name_Anbar { get; set; }
        public string Shomareh_havale { get; set; }
        public string Date_persiandate { get; set; }
        public string Code_TahvilGirandeh { get; set; }
        public string Nam_TahvilGirandeh { get; set; }
        public string Tozihat { get; set; }
        public string Guid { get; set; }
        public string FinanceUserID { get; set; }

    }

    public class SaveForm2
    {
        public int GoodsDepartureID { get; set; }
        public string Taeed { get; set; }
        public string Tozihat_Mali { get; set; }
        public string Guid { get; set; }
    }

    public class SaveForm3
    {
        public int GoodsDepartureID { get; set; }
        public string IsBazrasi { get; set; }
        public string Noe_Hamel { get; set; }
        public List<VehicleExit> VehicleExit { get; set; }
        public string Tozihat_Entezamat { get; set; }
        public string Guid { get; set; }
    }

}