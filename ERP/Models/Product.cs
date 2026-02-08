using System.ComponentModel.DataAnnotations;

namespace ERP.Models
{
    public class Product
    {
        [Key]
        public int ProductID { get; set; }
        public string CodeProduct { get; set; }
        public string NameProduct { get; set; }
        public string Price { get; set; }
        public string? NoeProduct { get; set; }
        public string? Zarib { get; set; }
        public string? NameProductMarket { get; set; }
        public string? QuantityINBox { get; set; }
        public string? Description { get; set; }
        public bool IsMeter { get; set; }
        public string? GroupTypeA { get; set; }
        public string? GroupTypeB { get; set; }
    }
}
