namespace ERP.ViewModels.OutsourcingProduction
{
    public class IndexOutsourcingProductionVM
    {
        public int OutsourcingProductionID { get; set; }
        public string State { get; set; } = string.Empty;
        public string S_persiandate { get; set; } = string.Empty;
        public string Guid { get; set; } = string.Empty;
        public int SourceTable { get; set; }
    }
}