using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Xac nhan mat khau")]
        [Compare("Password", ErrorMessage = "Mat khau voi mat khau xac nhan khong trung khop.")]
        public string ConfirmPassword { get; set; }
    }
}