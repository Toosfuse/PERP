namespace ERP.ViewModels.CoInqueryViewModel
{
    public class InqueryIndexViewModel
    {

        public List<string> Companys { get; set; }
        public List<string> Statuses { get; set; }
        public List<string> Results { get; set; }

        public int AllCount { get; set; }
        public Dictionary<string, int> CompanyCounts { get; set; }
        public Dictionary<string, int> StatusCounts { get; set; }
        public Dictionary<string, int> ResultCounts { get; set; }

        public string CurrentStatusFilter { get; set; }
        public string CurrentCompanyFilter { get; set; }
        public string CurrentResultFilter { get; set; }
    }
}
