using System.ComponentModel.DataAnnotations;

namespace EcommerceApp.ViewModels.Account;

public class ProfileViewModel
{
    [Required]
    [Display(Name = "Tên hiển thị")]
    [StringLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "Số điện thoại")]
    public string? PhoneNumber { get; set; }
}
