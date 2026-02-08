namespace ERP.ViewModels.SMT
{
    public class SMTViewModel
    {
        public long? Id { get; set; }
        public string DataValue { get; set; }
        public string DateCreate { get; set; } 
        public bool? IsDelete { get; set; } = false;
        public long? DailyScanCount {  get; set; }
        
    }

    

}
