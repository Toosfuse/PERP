namespace ERP.ViewModels.GuestEntry
{
    public class IndexGuestEntryViewModel
    {
        public int GuestEntryID { get; set; }
        public string Req_name { get; set; }
        public string State { get; set; }
        public string Vorud_persiandate { get; set; }
        public string Guid { get; set; }
        public int SourceTable { get; set; } // برای شناسایی منبع (مثلاً WorkflowAccesses یا WorkflowInstances)
    }
}
