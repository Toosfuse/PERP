namespace ERP.ViewModels.GoodsDeparture
{
    public class IndexGoodsDepartureViewModel
    {
        public int GoodsDepartureID { get; set; }
        public string State { get; set; }
        public string S_persiandate { get; set; }
        public string Guid { get; set; }
        public string? Nam_TahvilGirandeh { get; set; }
        public string? Tozihat_Entezamat { get; set; } // Security comments
        public int SourceTable { get; set; } // برای شناسایی منبع (مثلاً WorkflowAccesses یا WorkflowInstances)
    }
}
