using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Nho mat khau?")]
        public bool RememberMe { get; set; }
    }
}