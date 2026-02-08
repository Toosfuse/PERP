namespace ERP.ViewModels.CoInqueryViewModel
{

    public class InqueryWithProductsDto
    {
        public int CompanyInqueryID { get; set; }
        public string NeedNumber { get; set; } // شماره نیاز
        public string ResponseNumber { get; set; } // شماره پاسخ
        public string RegDate { get; set; }
        public string NeedDate { get; set; }
        public string City { get; set; }
        public int Sal { get; set; }
        public double TotalPrice { get; set; }
        public string InqueryResult { get; set; }
        public double? FeeCost { get; set; } // هزینه کارمزد
        public string Guid { get; set; }
        public string Status { get; set; } 
        public List<InqueryProductDto> Products { get; set; }
    }

    public class InqueryProductDto
    {
        public int CoInqueryItemID { get; set; }
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public double Quantity { get; set; }
        public string? Description { get; set; }
        public string Guid { get; set; }
        public double Price { get; set; }
        public double? Price1 { get; set; }
        public double? Price2 { get; set; }
        public double? Price3 { get; set; }
        public double? Score3 { get; set; }
        public double? Price4 { get; set; }
        public double? Price5 { get; set; }
        public double? Price6 { get; set; }
        public bool IsTemp { get; set; }
    }
}
