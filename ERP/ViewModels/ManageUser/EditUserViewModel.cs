using System.ComponentModel.DataAnnotations;

namespace ERP.ViewModels.ManageUser
{
    public class EditUserViewModel
    {
        [Required(ErrorMessage = "شناسه کاربر الزامی است")]
        public string Id { get; set; }

        [Required(ErrorMessage = "نام کاربری الزامی است")]
        public string UserName { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Post { get; set; }
        public string NewPassword { get; set; }
        public string NationalCode { get; set; }
        public string PhoneNumber { get; set; }
        public string CustCodeAmel { get; set; }
        public string Address { get; set; }
        public string ImageMain { get; set; }
        public string Image { get; set; }
    }
}
