namespace ERP.ViewModels.CoInqueryViewModel
{
    public class TenderWithProductsDto
    {
        public int CompanyTenderID { get; set; }
        public string City { get; set; }
        public string TenderNumber { get; set; }
        public int ProductID { get; set; }
        public double Quantity { get; set; }
        public double TotalPrice { get; set; }
        public string AlefDate { get; set; }
        public string AllPocketDate { get; set; }
        public string TenderResult { get; set; }
        public double FeeCost { get; set; }
        public string Status { get; set; } 
        public string Guid { get; set; }
        public List<TenderProductDto> Products { get; set; }
    }

    public class TenderProductDto
    {
        public int CoTenderItemID { get; set; }
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public double Quantity { get; set; }
        public string? Description { get; set; }
        public string Guid { get; set; }
        public double Price { get; set; }
        public double? Score { get; set; }
        public double? Price1 { get; set; }
        public double? Score1 { get; set; }
        public double? Price2 { get; set; }
        public double? Score2 { get; set; }
        public double? Price3 { get; set; }
        public double? Score3 { get; set; }
        public double? Price4 { get; set; }
        public double? Score4 { get; set; }
        public double? Price5 { get; set; }
        public double? Score5 { get; set; }
        public double? Price6 { get; set; }
        public double? Score6 { get; set; }
        public bool IsTemp { get; set; }
    }

}
