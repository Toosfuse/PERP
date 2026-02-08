using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ERP.Models
{
    public class Group
    {
        public int GroupID { get; set; }

        [Required(ErrorMessage = "نام گروه الزامی است.")]
        [Display(Name = "نام گروه")]
        public string Name { get; set; }

        public List<UserGroup> UserGroups { get; set; } = new List<UserGroup>();
    }

    public class UserGroup
    {
        public int GroupID { get; set; }

        [Required(ErrorMessage = "شناسه کاربر الزامی است.")]
        public string UserID { get; set; }
    }

}