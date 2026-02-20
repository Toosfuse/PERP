namespace ERP.ViewModels.SMT
{
    public class SMTSecondaryViewModel
    {
        public long Id { get; set; }
        public long SMTId { get; set; }
        public string DataValue { get; set; }
        public string DateCreate { get; set; }
        public string SecondaryDate { get; set; }
        public string Username { get; set; }
        public string CreatedAt { get; set; }
    }
    public class SMTSecondaryListViewModel
    {
       
        public string SecondaryDate { get; set; }
        public string SecondaryDatePersian { get; set; }
        public int count { get; set; }
   
    }
}
