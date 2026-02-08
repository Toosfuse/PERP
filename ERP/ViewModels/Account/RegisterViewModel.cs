using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ERP.ViewModels.Account
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "نام کاربری")]
        [Remote("IsUserNameInUse", "Account", HttpMethod = "POST",
        AdditionalFields = "__RequestVerificationToken")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "نام")]

        public string FirstName { get; set; }

        [Required]
        [Display(Name = "نام خانوادگی")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "پست سازمانی")]
        public string Post { get; set; }

        [Required]
        [Display(Name = "رمزعبور")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Display(Name = "تکرار رمزعبور")]
        [Compare(nameof(Password))]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}
