using Microsoft.AspNetCore.Identity;

namespace ERP.Models
{
    public class Users : IdentityUser
    {
        public int IntId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Post { get; set; }
        public string Image { get; set; }
        public string? NationalCode { get; set; }
        public string? Address { get; set; }
        public string? CustCodeAmel { get; set; }
        public bool? InTFC { get; set; }
    }
}
