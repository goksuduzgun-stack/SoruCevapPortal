using System.ComponentModel.DataAnnotations;

namespace SoruCevapPortal.ViewModels;

public class AdminLoginViewModel
{
    [Required]
    [Display(Name = "Kullanıcı adı")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
