namespace ERP.ViewModels.GoodsConsigned
{
    public class IndexGoodsConsignedVM
    {
        public int GoodsConsignedID { get; set; }
        public string State { get; set; }
        public string S_persiandate { get; set; }
        public string Guid { get; set; }
        public int SourceTable { get; set; } // To identify source (e.g., WorkflowAccesses or WorkflowInstances)
        public int TypeSending { get; set; }
    }
}
