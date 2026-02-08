using Microsoft.AspNetCore.Identity;

namespace ERP.Models
{
    public class Roles : IdentityRole
    {
        public string Title { get; set; }

    }
}
