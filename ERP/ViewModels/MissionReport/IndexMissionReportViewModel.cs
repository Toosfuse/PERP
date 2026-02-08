namespace ERP.ViewModels.MissionReport
{
    public class IndexMissionReportViewModel
    {
        public int MissionReportID { get; set; }
        public string Req_name { get; set; }
        public string State { get; set; }
        public string Vorud_persiandate { get; set; }
        public string Guid { get; set; }
        public int SourceTable { get; set; } // برای شناسایی منبع (مثلاً WorkflowAccesses یا WorkflowInstances)
    }
}
